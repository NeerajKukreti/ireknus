// Verbatim.js
$(document).ready(function () {

    // Utilities
    function escapeHtml(str) {
        if (str == null) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function escapeAttr(str) {
        // same as escapeHtml but kept separate for clarity
        return escapeHtml(str);
    }

    function escapeRegex(str) {
        // escape regex special chars
        return String(str).replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    function buildFlexiblePatternFromKey(key) {
        // Split by whitespace, escape tokens, join with pattern that allows tags between words
        var tokens = key.trim().split(/\s+/).filter(Boolean).map(escapeRegex);
        if (tokens.length === 0) return null;
        return tokens.join('\\s*(?:<[^>]+>\\s*)?'); // allows whitespace or HTML tags between words
    }

    // Highlight occurrences inside a phrase string (used in the middle column)
    function highlightPhraseText(phraseText, key) {
        if (!key) return escapeHtml(phraseText || '');
        var pattern = buildFlexiblePatternFromKey(key);
        if (!pattern) return escapeHtml(phraseText || '');
        var re = new RegExp(pattern, 'gi');
        // Replace matches with safe highlighted HTML
        return (phraseText || '').replace(re, function (match) {
            return `<span class="phrase-highlight">${escapeHtml(match)}</span>`;
        });
    }

    // Obtain URLs from global vars declared in the page (your Razor sets these)
    var searchKeywordUrl = typeof window.SearchKeyword !== 'undefined' ? window.SearchKeyword : null;
    var pdfTextUrl = typeof window.pdfText !== 'undefined' ? window.pdfText : null;

    if (!searchKeywordUrl) console.warn('SearchKeyword URL not found (window.SearchKeyword is undefined).');
    if (!pdfTextUrl) console.warn('pdfText URL not found (window.pdfText is undefined).');

    // Click on a keyword (left column)
    $(document).on('click', '.keywords', function () {
        var foundkeywords = $(this).data('foundkeywords'); // expects data-foundkeywords="..."
        var URL = $(this).data('url');

        console.log('Keyword clicked:', foundkeywords, URL);

        $.ajax({
            url: searchKeywordUrl,
            type: 'GET',
            data: { URL: URL, Keyword: foundkeywords },
            beforeSend: function () {
                $('.keywordPhrases').empty().append("<p>Loading phrases...</p>");
            },
            success: function (data) {
                $('.keywordPhrases').empty();
                var cnt = 0;
                // data is expected to be an array where item.FoundKeywords and item.PhraseText exist
                $.each(data, function (index, item) {
                    var phraseList = item.PhraseText || [];
                    var foundKey = item.FoundKeywords || '';

                    $.each(phraseList, function (index1, phrase) {
                        cnt++;
                        // highlight keyword inside phrase (middle column)
                        var phraseHtml = highlightPhraseText(phrase || '', foundKey);

                        // Build html: use data-foundkeywords (lowercase) and data-pageno as numeric occurrence index
                        var htmlContent = `
                            <div class="col1">
                                <div class="cont">
                                    <div data-pageno="${cnt}" 
                                         data-foundkeywords='${escapeAttr(foundKey)}'
                                         class="desc keywordPhrase" style="cursor: pointer;">
                                        ${escapeHtml(item.PDFPageNumber)}: ${phraseHtml || 'No keywords'}
                                    </div>
                                </div>
                            </div>
                        `;
                        $('.keywordPhrases').append(htmlContent);
                    });
                });
            },
            error: function () {
                alert('Error occurred while trying to search keyword phrases.');
            },
            complete: function () {
                // load PDF text into right column (if pdfTextUrl exists)
                if (!pdfTextUrl) return;
                $.ajax({
                    url: pdfTextUrl,
                    type: 'GET',
                    beforeSend: function () {
                        $('.pdfText').empty().append("<p>Loading PDF text...</p>");
                    },
                    success: function (data) {
                        // Normalize whitespace
                        data = data.replace(/\r\n/g, "\n");

                        // Regex: Match "Capitalized words (with spaces/dots) followed by colon"
                        var updatedData = data.replace(/\b([A-Z][A-Za-z .]+?):/g, function (match, name) {
                            return '<br><strong style="color:red;">' + name + ':</strong>';
                        });

                        $('.pdfText').empty().html(updatedData);
                    },
                    error: function () {
                        alert('Error occurred while trying to load PDF text.');
                    }
                });
            }
        });
    });

    // Save original PDF text (if needed to reset later)
    var orgText = $('.pdfText').html();

    // Click on a single phrase (middle column) -> highlight the Nth occurrence in the PDF text
    $(document).on('click', '.keywordPhrase', function () {
        var key = $(this).data('foundkeywords'); // expects data-foundkeywords
        var index = parseInt($(this).data('pageno'), 10);
        var pdfContentHtml = $('.pdfText').html();

        console.log('Phrase clicked. Key:', key, 'Occurrence index:', index);

        if (!key) {
            console.log('No keyword to highlight.');
            return;
        }
        if (!pdfContentHtml) {
            console.log('PDF text not loaded yet.');
            return;
        }

        // Build a flexible regex that allows HTML tags between words
        var pattern = buildFlexiblePatternFromKey(key);
        if (!pattern) {
            console.log('Empty pattern from key.');
            return;
        }
        var regex = new RegExp(pattern, 'gi');

        var currentIndex = 0;
        var uniqueId = 'foundText-' + Date.now();

        // Replace only the desired occurrence
        var updatedText = pdfContentHtml.replace(regex, function (match) {
            currentIndex++;
            if (currentIndex === index) {
                // Wrap the match with a span that has an id so we can scroll to it
                return `<span id="${uniqueId}" class="found-text-highlight">${match}</span>`;
            }
            return match;
        });

        if (currentIndex >= index) {
            $('.pdfText').html(updatedText);
            // Scroll to highlighted element
            setTimeout(function () {
                var target = document.getElementById(uniqueId);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center',
                        inline: 'nearest'
                    });
                } else {
                    console.log('Highlighted element not found after update.');
                }
            }, 60);
        } else {
            console.log('Occurrence not found in PDF text. Found only', currentIndex, 'occurrences.');
        }

        // Visual cue: mark selected phrase
        $('.keywordPhrase').removeClass('active-phrase');
        $(this).addClass('active-phrase');
    });

});
