// search-worker.js - Web Worker for PDF search operations
// This worker handles text extraction and search operations off the main thread

let pdfDoc = null;
let textCache = new Map();

// Import PDF.js in the worker context
importScripts('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.min.js');
pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.worker.min.js';

// Message handler
self.onmessage = async function(e) {
    const { type, data, id } = e.data;
    
    try {
        switch (type) {
            case 'LOAD_PDF':
                await loadPDF(data.arrayBuffer, id);
                break;
            case 'SEARCH_TEXT':
                await searchText(data.searchTerm, data.pages, id);
                break;
            case 'EXTRACT_PAGE_TEXT':
                await extractPageText(data.pageNum, id);
                break;
            case 'CLEAR_CACHE':
                clearCache(id);
                break;
            default:
                postMessage({ type: 'ERROR', error: 'Unknown message type', id });
        }
    } catch (error) {
        postMessage({ type: 'ERROR', error: error.message, id });
    }
};

async function loadPDF(arrayBuffer, id) {
    try {
        const typedArray = new Uint8Array(arrayBuffer);
        pdfDoc = await pdfjsLib.getDocument(typedArray).promise;
        textCache.clear();
        
        postMessage({
            type: 'PDF_LOADED',
            data: { numPages: pdfDoc.numPages },
            id
        });
    } catch (error) {
        postMessage({ type: 'ERROR', error: error.message, id });
    }
}

async function searchText(searchTerm, pages, id) {
    if (!pdfDoc) {
        postMessage({ type: 'ERROR', error: 'PDF not loaded', id });
        return;
    }

    const lowerTerm = searchTerm.toLowerCase();
    const foundPages = [];
    const batchSize = 5;
    
    try {
        for (let i = 0; i < pages.length; i += batchSize) {
            const batch = pages.slice(i, i + batchSize);
            
            // Process batch of pages
            const promises = batch.map(async (pageNum) => {
                const text = await getPageText(pageNum);
                return {
                    page: pageNum,
                    hasMatch: text.toLowerCase().includes(lowerTerm),
                    text: text
                };
            });
            
            const results = await Promise.all(promises);
            
            results.forEach(result => {
                if (result.hasMatch) {
                    foundPages.push({
                        page: result.page,
                        text: result.text
                    });
                }
            });
            
            // Send progress update
            postMessage({
                type: 'SEARCH_PROGRESS',
                data: {
                    processed: i + batch.length,
                    total: pages.length,
                    foundSoFar: foundPages.length
                },
                id
            });
        }
        
        postMessage({
            type: 'SEARCH_COMPLETE',
            data: { foundPages },
            id
        });
    } catch (error) {
        postMessage({ type: 'ERROR', error: error.message, id });
    }
}

async function extractPageText(pageNum, id) {
    try {
        const text = await getPageText(pageNum);
        postMessage({
            type: 'PAGE_TEXT_EXTRACTED',
            data: { pageNum, text },
            id
        });
    } catch (error) {
        postMessage({ type: 'ERROR', error: error.message, id });
    }
}

async function getPageText(pageNum) {
    // Check cache first
    if (textCache.has(pageNum)) {
        return textCache.get(pageNum);
    }
    
    const page = await pdfDoc.getPage(pageNum);
    const textContent = await page.getTextContent();
    const text = textContent.items.map(item => item.str).join(' ');
    
    // Cache the result
    textCache.set(pageNum, text);
    
    // Limit cache size
    if (textCache.size > 50) {
        const firstKey = textCache.keys().next().value;
        textCache.delete(firstKey);
    }
    
    return text;
}

function clearCache(id) {
    textCache.clear();
    postMessage({ type: 'CACHE_CLEARED', id });
}

// Handle worker errors
self.onerror = function(error) {
    postMessage({
        type: 'ERROR',
        error: error.message,
        id: null
    });
};