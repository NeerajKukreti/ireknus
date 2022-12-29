using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PDFReader.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFReader
{
    internal class PDFSearch
    {
        public static async Task<List<FetchedKeywords>> Search(int ReportID, string PDFUrl, List<String> searchList)
        {
            List<FetchedKeywords> fetchedKeywords = new List<FetchedKeywords>();
            int page = 0;
            int emptyPageCtn = 0;
            int TotalPage = 0;

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                PdfReader pdfReader = new PdfReader(PDFUrl);
                Dictionary<int, string> strlist = new Dictionary<int, string>();
                TotalPage = pdfReader.NumberOfPages;

                for (page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    ITextExtractionStrategy strategy1 = new LocationTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    currentPageText = currentPageText + " "+ PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy1);
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

                    //if (!string.IsNullOrEmpty(keywords.ToString()))
                    //    strlist.Add(page, keywords.ToString());

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
            catch (Exception ex)
            {
            }

            await DB.InsertFoundKeywords(fetchedKeywords, TotalPage, emptyPageCtn == TotalPage, ReportID);

            return fetchedKeywords;
        }
    }
}