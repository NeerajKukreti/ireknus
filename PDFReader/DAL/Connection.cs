using System.Configuration;

namespace PDFReader
{
    public class Connection
    {
        private static string connectionString = string.Empty;

        public static string MyConnection()
        {
            return connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }
    }
}