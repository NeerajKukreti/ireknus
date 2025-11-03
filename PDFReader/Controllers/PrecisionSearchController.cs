using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using iTextSharp.text;
using Newtonsoft.Json;
using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

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
        public async Task<ActionResult> UploadTextFile(HttpPostedFileBase textFile)
        {
            try
            {
                // Validate file input
                if (textFile == null || textFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Please select a text file to upload." });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".txt", ".text" };
                var fileExtension = Path.GetExtension(textFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only text files (.txt, .text) are allowed." });
                }

                // Validate file size (max 10MB)
                const int maxFileSize = 10 * 1024 * 1024; // 10MB
                if (textFile.ContentLength > maxFileSize)
                {
                    return Json(new { success = false, message = "File size must be less than 10MB." });
                }

                // Check if PDF file exists
                if (!System.IO.File.Exists(_currentPdfPath))
                {
                    return Json(new { success = false, message = "No PDF file found. Please upload a PDF file first." });
                }

                // Read the text file content
                string fileContent;
                using (var reader = new StreamReader(textFile.InputStream, Encoding.UTF8))
                {
                    fileContent = reader.ReadToEnd();
                }

                // Basic validation of content
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return Json(new { success = false, message = "The uploaded file appears to be empty." });
                }

                // Parse enhanced text format: Keywords and Page Skip ranges
                var parseResult = ParseTextFileContent(fileContent);

                if (!parseResult.Keywords.Any())
                {
                    return Json(new { success = false, message = "No valid keywords found in the text file." });
                }

                // Read PDF file as stream
                byte[] pdfBytes = System.IO.File.ReadAllBytes(_currentPdfPath);
                var allPhrases = new List<FetchedPhrase>();

                // Process each keyword against the stored PDF with page filtering
                foreach (var keyword in parseResult.Keywords)
                {
                    try
                    {
                        using (var pdfStream = new MemoryStream(pdfBytes))
                        {
                            var phrases = await PDFSearch.GetPhrasesWithPageFilter(pdfStream, keyword, parseResult.PageSkipRanges);
                            if (phrases != null && phrases.Any())
                            {
                                allPhrases.AddRange(phrases);
                            }
                        }
                    }
                    catch (Exception keywordEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing keyword '{keyword}': {keywordEx.Message}");
                        // Continue with other keywords even if one fails
                    }
                }

                // Return results for display in view
                return Json(new
                {
                    success = true,
                    message = $"Processed {parseResult.Keywords.Count} keywords from text file.",
                    keywordsCount = parseResult.Keywords.Count,
                    phrasesFound = allPhrases.Count,
                    keywords = parseResult.Keywords,
                    phrases = allPhrases,
                    pageSkipRanges = parseResult.PageSkipRanges,
                    skipInfo = parseResult.SkipInfo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework)
                System.Diagnostics.Debug.WriteLine($"Error uploading text file: {ex.Message}");

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the file. Please try again."
                });
            }
        }

        // Helper method to parse text file content with enhanced format
        private ParsedTextFileContent ParseTextFileContent(string fileContent)
        {
            var result = new ParsedTextFileContent
            {
                Keywords = new List<string>(),
                PageSkipRanges = new List<PageRange>(),
                SkipInfo = ""
            };

            try
            {
                // Split content into lines
                var lines = fileContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(line => line.Trim())
                                      .Where(line => !string.IsNullOrWhiteSpace(line))
                                      .ToList();

                // Parse keywords line
                var keywordLine = lines.FirstOrDefault(l => l.StartsWith("Keyword:", StringComparison.OrdinalIgnoreCase));
                if (keywordLine != null)
                {
                    var keywordContent = keywordLine.Substring("Keyword:".Length).Trim();
                    result.Keywords = keywordContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(k => k.Trim())
                                                  .Where(k => !string.IsNullOrWhiteSpace(k))
                                                  .ToList();
                }

                // Parse page skip line
                var pageSkipLine = lines.FirstOrDefault(l => l.StartsWith("Page Skip:", StringComparison.OrdinalIgnoreCase));
                if (pageSkipLine != null)
                {
                    var pageSkipContent = pageSkipLine.Substring("Page Skip:".Length).Trim();
                    result.PageSkipRanges = ParsePageRanges(pageSkipContent);
                    result.SkipInfo = $"Skipping {result.PageSkipRanges.Count} page range(s): {pageSkipContent}";
                }

                // Fallback: if no "Keyword:" prefix found, treat entire content as comma-separated keywords
                if (!result.Keywords.Any())
                {
                    result.Keywords = fileContent.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(k => k.Trim())
                                                .Where(k => !string.IsNullOrWhiteSpace(k) && !k.StartsWith("Page Skip:", StringComparison.OrdinalIgnoreCase))
                                                .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing text file content: {ex.Message}");
                
                // Fallback to simple comma-separated parsing
                result.Keywords = fileContent.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(k => k.Trim())
                                            .Where(k => !string.IsNullOrWhiteSpace(k))
                                            .ToList();
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