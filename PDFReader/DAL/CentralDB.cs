using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PDFReader
{
    public class CentralDB
    {
        #region Central Alert
        public async static Task<List<CentralModel>> CheckandLogNewChanges()
        {
            using (var connection = new SqlConnection(Connection.MyConnection()))
            {
                connection.Open();

                var resultSet = connection
                    .QueryAsync<CentralModel>("_sp_CentralAlert",
                    null, commandType: CommandType.StoredProcedure).Result;

               return resultSet.ToList();
            }
        }

        #endregion
    }
}