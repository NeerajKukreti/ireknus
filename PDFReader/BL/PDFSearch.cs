using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PDFReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace PDFReader
{
    internal class PDFSearch
    {
        private static MemoryCache _cache = MemoryCache.Default;

        public static string SetData(string pdfText)
        {
            string key = "PDF_Text";
            string data = pdfText;

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30); // Cache for 30 minutes
            _cache.Set(key, data, policy);

            return data;
        }

        public static async Task<List<FetchedKeywords>> Search(int ReportID, string PDFUrl, List<string> searchList)
        {
            List<FetchedKeywords> fetchedKeywords = new List<FetchedKeywords>();
            int page = 0;
            int emptyPageCtn = 0;
            int? TotalPage = 0;

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                PdfReader pdfReader = null;
                try
                {
                    pdfReader = new PdfReader(PDFUrl);
                }
                catch { }

                if (pdfReader == null)
                {
                    PDFUrl = PDFUrl.Replace("AttachLive", "AttachHis");
                    pdfReader = new PdfReader(PDFUrl);
                }

                Dictionary<int, string> strlist = new Dictionary<int, string>();
                TotalPage = pdfReader?.NumberOfPages;

                for (page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    ITextExtractionStrategy strategy1 = new LocationTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    currentPageText = currentPageText + " " + PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy1);
                    currentPageText =
                        Regex.Replace(currentPageText.Replace(" \n", " ").Replace("\n", " "), @"\s+", " ");

                    if (string.IsNullOrEmpty(currentPageText)) { emptyPageCtn++; continue; }

                    List<string> keywords = new List<string>();

                    searchList.ForEach(keyword =>
                    {
                        var pattern = @"\b" + keyword.ToLower() + @"\b";

                        Regex rgx = new Regex(pattern);
                        Match match = rgx.Match(currentPageText.ToLower());

                        if (match.Success)
                        {
                            keywords.Add(keyword);
                        }
                    });

                    keywords.ForEach(keyword =>
                    {
                        fetchedKeywords.Add(new FetchedKeywords
                        {
                            ReportID = ReportID,
                            FoundKeywords = keyword,
                            PDFPageNumber = page
                        });
                    });
                }

                pdfReader.Close();

            }
            catch { }

            await DB.InsertFoundKeywords(fetchedKeywords, (TotalPage ?? 0), emptyPageCtn == TotalPage, ReportID);

            return fetchedKeywords;
        }

        public static async Task<List<FetchedPhrase>> GetPhrases(string PDFUrl, string keyword)
        {
            List<FetchedPhrase> fetchedPhrases = new List<FetchedPhrase>();

            int page = 0;
            int emptyPageCtn = 0;
            int? TotalPage = 0;

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                PdfReader pdfReader = null;
                try
                {
                    pdfReader = new PdfReader(PDFUrl);
                }
                catch { }

                if (pdfReader == null)
                {
                    PDFUrl = PDFUrl.Replace("AttachLive", "AttachHis");
                    pdfReader = new PdfReader(PDFUrl);
                }

                Dictionary<int, string> strlist = new Dictionary<int, string>();
                TotalPage = pdfReader?.NumberOfPages;
                
                var pageText = new StringBuilder();

                for (page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    List<string> results = new List<string>();
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    ITextExtractionStrategy strategy1 = new LocationTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy1);
                    pageText.Append(currentPageText);
                    currentPageText =
                        Regex.Replace(currentPageText.Replace(" \n", " ").Replace("\n", " "), @"\s+", " ");

                    if (string.IsNullOrEmpty(currentPageText)) { emptyPageCtn++; continue; }

                    results.AddRange(TextSearcher.SearchAllWithContext(currentPageText, keyword).ToList());
                    fetchedPhrases.Add(new FetchedPhrase
                    {
                        ReportID = 0,
                        FoundKeywords = keyword,
                        PDFPageNumber = page,
                        PhraseText = results
                    });
                }

                SetData(pageText.ToString().Replace("\n", "</br>"));
            }
            catch { }

            return fetchedPhrases;
        }
        public static async Task<List<FetchedKeywords>> SearchPara(int pageNo, string PDFUrl, string keyword = "Spark Capital")
        {
            List<FetchedKeywords> fetchedKeywords = new List<FetchedKeywords>();
            int page = pageNo;
            int emptyPageCtn = 0;
            int? TotalPage = 0;

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                PdfReader pdfReader = null;
                try
                {
                    pdfReader = new PdfReader(PDFUrl);
                }
                catch { }

                if (pdfReader == null)
                {
                    PDFUrl = PDFUrl.Replace("AttachLive", "AttachHis");
                    pdfReader = new PdfReader(PDFUrl);
                }

                TotalPage = pdfReader?.NumberOfPages;

                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                ITextExtractionStrategy strategy1 = new LocationTextExtractionStrategy();
                string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, 3, strategy);
                //currentPageText = currentPageText + " " + PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy1);
                currentPageText = currentPageText.Replace("\n", "\\n").Replace("\r", "\\r");
                string pattern = @"\n*:";
                string[] segments = Regex.Split(currentPageText, pattern);

                pdfReader.Close();

                // Split text into paragraphs (assuming paragraphs are separated by double newlines or single newline)
                string[] paragraphs =
                    currentPageText.Split(new[] { "\r\n\r\n", "\n\n", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Find the paragraph containing the keyword
                int keywordParaIndex = -1;
                for (int i = 0; i < paragraphs.Length; i++)
                {
                    if (paragraphs[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        keywordParaIndex = i;
                        break;
                    }
                }

                if (keywordParaIndex == -1)
                {
                    // Keyword not found, return empty list
                    return fetchedKeywords;
                }

                // Search for "?" in the paragraph containing the keyword or previous paragraphs
                int questionParaIndex = keywordParaIndex;
                while (questionParaIndex >= 0)
                {
                    if (paragraphs[questionParaIndex].Contains("?"))
                    {
                        break;
                    }
                    questionParaIndex--;
                }

                if (questionParaIndex == -1)
                {
                    // No paragraph with "?" found, return empty list
                    return fetchedKeywords;
                }

                // Collect the paragraph with "?" and next three paragraphs
                List<string> resultParagraphs = new List<string>();
                for (int i = questionParaIndex; i < paragraphs.Length && resultParagraphs.Count < 4; i++)
                {
                    resultParagraphs.Add(paragraphs[i]);
                }

                // Add each paragraph to the result as a FetchedKeywords object
                foreach (var para in resultParagraphs)
                {
                    fetchedKeywords.Add(new FetchedKeywords
                    {
                        ReportID = 0, // No ReportID in this context
                        FoundKeywords = keyword,
                        PDFPageNumber = page,
                        PageText = para
                    });
                }
            }
            catch { }

            return fetchedKeywords;
        }
    }
}