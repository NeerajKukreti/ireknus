using System.ComponentModel.DataAnnotations;

namespace PDFReader.Models
{
    public class WatchlistModel
    {
        public int WATCHLIST_ID { get; set; }

        [Required]
        public string COMPANY_ID { get; set; }

        [Required]
        public string COMPANY_NAME { get; set; }

        public string ACTION { get; set; }
    }
}