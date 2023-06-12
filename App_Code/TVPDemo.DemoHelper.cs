using System;
using System.Data;
using System.Data.SqlClient;


namespace TVPDemo
{
    public class DemoHelper
    {

        // Connection string. Change to fit to your environment.
        private const string connstr =
                //"Application Name=TVPdemo;Integrated Security=SSPI;" +
                //"Data Source=.;Initial Catalog=tempdb";
                "Server=DESKTOP-ET38L0F;Database=Solko_OfferSystem;User Id = sa; Password=1";


        // Procedure to print messages from SQL Server, errors or informational
        // messages.
        public static void PrintSqlMsgs(SqlErrorCollection msgs)
        {
            foreach (SqlError e in msgs)
            {
                Console.WriteLine(
                   "Msg {0}, Severity {1}, State: {2}, Procedure {3}, Line no: {4}",
                   e.Number.ToString(), e.Class.ToString(), e.State.ToString(),
                   e.Procedure, e.LineNumber.ToString()
                );
                Console.WriteLine(e.Message);
            }
        }

        // Handler for messages from SQL Server. For this demo, we do not
        // distinguish between errors and informational messages.
        public static void SqlInfoMessage(object sender,
                                           SqlInfoMessageEventArgs ea)
        {
            PrintSqlMsgs(ea.Errors);
        }

        // Simple generic routine to print the contents of a data set.
        public static void PrintDataSet(DataSet ds)
        {
            Console.WriteLine("============= Dataset =======================");
            if (ds.Tables.Count == 0)
            {
                Console.WriteLine("Empty dataset");
            }
            else
            {
                foreach (DataTable tbl in ds.Tables)
                {
                    Console.WriteLine("----------------------------------------");
                    foreach (DataColumn col in tbl.Columns)
                    {
                        Console.Write(col.ColumnName + "\t");
                    }
                    Console.WriteLine();
                    foreach (DataRow row in tbl.Rows)
                    {
                        foreach (DataColumn col in tbl.Columns)
                        {
                            Console.Write(row[col].ToString() + "\t");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        // This helper routine sets up our SQL Connection.
        public static SqlConnection setup_connection()
        {
            // Create the connection.
            SqlConnection cn = new SqlConnection(connstr);

            // Handle user errors with callbacks, rather than exception.
            cn.InfoMessage += SqlInfoMessage;
            cn.FireInfoMessageEventOnUserErrors = true;

            cn.Open();

            return cn;
        }
    }
}