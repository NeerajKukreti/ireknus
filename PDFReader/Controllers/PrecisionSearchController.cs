using PDFReader.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class PrecisionSearchController : Controller
    {
        private readonly string _precisionSearchFolder;
        private readonly string _currentPdfPath;

        public PrecisionSearchController()
        {
            // Initialize AppData folder path for PrecisionSearch
            string appDataPath = $"{AppContext.BaseDirectory}\\App_Data";

            _precisionSearchFolder = Path.Combine(appDataPath, "PrecisionSearch");
            _currentPdfPath = Path.Combine(_precisionSearchFolder, "current.pdf");
            
            // Ensure directory exists
            if (!Directory.Exists(_precisionSearchFolder))
            {
                Directory.CreateDirectory(_precisionSearchFolder);
            }
        }

        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            return View();
        }

        // POST: Upload PDF file
        [HttpPost]
        public ActionResult UploadPDF(HttpPostedFileBase file)
        {
            try
            {
                // Validate file input
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Please select a PDF file to upload." });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".pdf" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only PDF files (.pdf) are allowed." });
                }

                // Validate file size (max 50MB for PDFs)
                const int maxFileSize = 50 * 1024 * 1024; // 50MB
                if (file.ContentLength > maxFileSize)
                {
                    return Json(new { success = false, message = "File size must be less than 50MB." });
                }

                // Delete previous PDF file if exists
                if (System.IO.File.Exists(_currentPdfPath))
                {
                    try
                    {
                        System.IO.File.Delete(_currentPdfPath);
                        System.Diagnostics.Debug.WriteLine("Previous PDF file deleted successfully.");
                    }
                    catch (Exception deleteEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not delete previous PDF file: {deleteEx.Message}");
                        // Continue with upload even if deletion fails
                    }
                }

                // Save new PDF file to AppData/PrecisionSearch/current.pdf
                file.SaveAs(_currentPdfPath);

                // Return success with file information
                return Json(new
                {
                    success = true,
                    message = "PDF file uploaded and stored successfully.",
                    fileName = file.FileName,
                    fileSize = file.ContentLength,
                    storedPath = _currentPdfPath
                });
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework)
                System.Diagnostics.Debug.WriteLine($"Error uploading PDF file: {ex.Message}");

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the PDF file. Please try again."
                });
            }
        }

        // POST: Upload text file
        [HttpPost]
        public async Task<ActionResult> UploadTextFile(HttpPostedFileBase textFile, string pageSkipRanges)
        {
            try
            {
                // Validate file input
                if (textFile == null || textFile.ContentLength == 0)
                    return Json(new { success = false, message = "Please select a text file to upload." });

                // Validate file extension
                var allowedExtensions = new[] { ".txt", ".text" };
                var fileExtension = Path.GetExtension(textFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return Json(new { success = false, message = "Only text files (.txt, .text) are allowed." });

                // Validate file size (max 10MB)
                const int maxFileSize = 10 * 1024 * 1024; // 10MB
                if (textFile.ContentLength > maxFileSize)
                    return Json(new { success = false, message = "File size must be less than 10MB." });

                // Check if PDF file exists
                if (!System.IO.File.Exists(_currentPdfPath))
                    return Json(new { success = false, message = "No PDF file found. Please upload a PDF file first." });

                // Read the text file content
                string fileContent;
                using (var reader = new StreamReader(textFile.InputStream, Encoding.UTF8))
                {
                    fileContent = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(fileContent))
                    return Json(new { success = false, message = "The uploaded text file appears to be empty." });

                //Read skip ranges from textbox input
                var skipRanges = ParsePageRanges(pageSkipRanges);
                var skipInformation = string.IsNullOrWhiteSpace(pageSkipRanges)
                    ? "No pages skipped."
                    : $"Skipping {skipRanges.Count} page range(s): {pageSkipRanges}";

                //Parse headed keyword file
                var parseResult = ParseTextFileContent(fileContent);

                if (parseResult.TotalKeywordCount == 0)
                    return Json(new { success = false, message = "No valid keywords found in the text file." });

                //Read PDF file once
                byte[] pdfBytes = System.IO.File.ReadAllBytes(_currentPdfPath);
                var allPhrases = new List<FetchedPhrase>();

                //Process keywords grouped by header
                foreach (var head in parseResult.HeadKeywordMap)
                {
                    foreach (var keyword in head.Value)
                    {
                        try
                        {
                            using (var pdfStream = new MemoryStream(pdfBytes))
                            {
                                var phrases = await PDFSearch.GetPhrasesWithPageFilter(pdfStream, keyword, skipRanges);
                                if (phrases != null && phrases.Any())
                                {
                                    allPhrases.AddRange(phrases);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing keyword '{keyword}': {ex.Message}");
                            // Continue with other keywords
                        }
                    }
                }

                //Return results for UI display
                return Json(new
                {
                    success = true,
                    message = $"Processed {parseResult.TotalKeywordHeadCount} header(s) and {parseResult.TotalKeywordCount} keywords.",
                    headersCount = parseResult.TotalKeywordHeadCount,
                    keywordsCount = parseResult.TotalKeywordCount,
                    phrasesFound = allPhrases.Count,
                    headKeywordMap = parseResult.HeadKeywordMap,
                    phrases = allPhrases,
                    pageSkipRanges = skipRanges,
                    skipInfo = skipInformation
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading text file: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the text file. Please try again."
                });
            }
        }


        // Helper method to parse text file content with enhanced format
        private ParsedTextFileContent ParseTextFileContent(string fileContent)
        {
            var result = new ParsedTextFileContent();

            try
            {
                // ensure dictionary is initialized (in case model constructor wasn't updated)
                if (result.HeadKeywordMap == null)
                    result.HeadKeywordMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                // remove BOM if present
                if (!string.IsNullOrEmpty(fileContent) && fileContent[0] == '\uFEFF')
                    fileContent = fileContent.Substring(1);

                string currentHeader = null;
                const string defaultHeader = "Default";

                // Split by lines, clean up, ignore blanks
                var lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(l => l.Trim())
                                       .Where(l => !string.IsNullOrWhiteSpace(l))
                                       .ToList();

                foreach (var line in lines)
                {
                    if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        // It's a header line like "#Head 1"
                        currentHeader = line.TrimStart('#').Trim();

                        if (string.IsNullOrWhiteSpace(currentHeader))
                        {
                            // fallback header name if header line was just "#"
                            currentHeader = defaultHeader;
                        }

                        if (!result.HeadKeywordMap.ContainsKey(currentHeader))
                            result.HeadKeywordMap[currentHeader] = new List<string>();
                    }
                    else
                    {
                        // If we found a keyword before any header, place it under Default header
                        if (string.IsNullOrEmpty(currentHeader))
                        {
                            currentHeader = defaultHeader;
                            if (!result.HeadKeywordMap.ContainsKey(currentHeader))
                                result.HeadKeywordMap[currentHeader] = new List<string>();
                        }

                        // It's a keyword line
                        result.HeadKeywordMap[currentHeader].Add(line);
                        result.TotalKeywordCount++;
                    }
                }

                result.TotalKeywordHeadCount = result.HeadKeywordMap?.Count ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing text file content: {ex.Message}");
            }

            return result;
        }


        // Helper method to parse page ranges like "1-10, 21-90"
        private List<PageRange> ParsePageRanges(string pageRangeText)
        {
            var ranges = new List<PageRange>();

            try
            {
                var rangeParts = pageRangeText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in rangeParts)
                {
                    var trimmedPart = part.Trim();
                    
                    if (trimmedPart.Contains('-'))
                    {
                        var rangeSplit = trimmedPart.Split('-');
                        if (rangeSplit.Length == 2 && 
                            int.TryParse(rangeSplit[0].Trim(), out int start) && 
                            int.TryParse(rangeSplit[1].Trim(), out int end))
                        {
                            ranges.Add(new PageRange { Start = start, End = end });
                        }
                    }
                    else if (int.TryParse(trimmedPart, out int singlePage))
                    {
                        ranges.Add(new PageRange { Start = singlePage, End = singlePage });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing page ranges: {ex.Message}");
            }

            return ranges;
        }

        // Helper method to get current PDF info
        [HttpGet]
        public ActionResult GetCurrentPDFInfo()
        {
            try
            {
                if (System.IO.File.Exists(_currentPdfPath))
                {
                    var fileInfo = new FileInfo(_currentPdfPath);
                    return Json(new
                    {
                        success = true,
                        exists = true,
                        fileName = "current.pdf",
                        fileSize = fileInfo.Length,
                        lastModified = fileInfo.LastWriteTime
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        exists = false,
                        message = "No PDF file currently stored."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error checking PDF file status."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper classes for parsing results
        
    }
}