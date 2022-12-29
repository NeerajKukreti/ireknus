using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PDFReader.Models
{
    public class KeywordModel
    {
        [Key]
        public int KID { get; set; }
        [Required]
        public string KEYWORD { get; set; }
        public string ACTION { get; set; }
        public string SMID { get; set; }
        public string SM_NAME { get; set; }
        public List<SelectListItem> KEYWORDSET { get; set; }
    }

    public class KeywordSetModel
    {
        [Key]
        public string SMID { get; set; }
        [Required]
        public string SM_NAME { get; set; }

        public string ACTION { get; set; }
    }
}