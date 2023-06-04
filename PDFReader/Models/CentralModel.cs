using System;

namespace PDFReader.Models
{
    public class CentralModel
    {
        public int Record_ID { get; set; }
        public string Proposal_No { get; set; }
        public string MOEFCC_File_No { get; set; }
        public string Project_Name { get; set; }
        public string Company { get; set; }
        public string Proposal_Status { get; set; }
        public string Location { get; set; }
        public string Important_Dates { get; set; }
        public string Category { get; set; }
        public string Company_or_Proponent { get; set; }
        public string Type_of_project { get; set; }
        public string Attached_Files { get; set; }
        public string Files_Detail { get; set; }
        public string Acknowlegment { get; set; }
        public string Pagination { get; set; }
        public string input_company_name { get; set; }
        public string subsidiary_name { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }
}