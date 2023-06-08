using System;

namespace PDFReader.Models
{
    public class StateModel
    {
        public int Record_ID { get; set; }
        public string Proposal_No { get; set; }
        public string File_No { get; set; }
        public string Proposal_Name { get; set; }
        public string Company_or_Proponent { get; set; }
        public string Current_Status { get; set; }
        public string Location { get; set; }
        public string Important_Dates { get; set; }
        public string Category { get; set; }
        public string Attached_Files { get; set; }
        public string Files_Detail { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }
}