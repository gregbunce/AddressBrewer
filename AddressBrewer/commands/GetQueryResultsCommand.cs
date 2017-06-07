using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddressBrewer.models;

namespace AddressBrewer.commands
{
    public class GetQueryResultsCommand
    {
        private string _sqlQuery;
        private readonly CliOptions _options;


        public GetQueryResultsCommand(CliOptions options)
        {
            //_sqlQuery = sqlQuery;
            _options = options;
        }

        public ListDictionary ReturnValues(string sqlQuery)
        {
            try
            {
                ListDictionary list = new ListDictionary();
                _sqlQuery = sqlQuery;

                //Dictionary<string, object> sqlResult = new Dictionary<string, object>();
                
                var connectionString = @"Persist Security Info=False;Integrated Security=true;Initial Catalog=" +_options.DatabaseName + @";server=" + _options.Server;

                // get a record set of county address point updates 
                using (var con1 = new SqlConnection(connectionString))
                {
                    // open the sqlconnection
                    con1.Open();

                    // create a sqlcommand - allowing for a subset of records from the table
                    using (var command1 = new SqlCommand(_sqlQuery, con1))

                        // create a sqldatareader
                    using (var reader1 = command1.ExecuteReader())
                    {
                        if (!reader1.HasRows) return null;
                        // loop through the record set
                        while (reader1.Read())
                        {
                            list.Add("OBJECTID", reader1["OBJECTID"].ToString());
                        }


                        // use LINQ to get result row as a dictionary
                        //return Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                        
                    }
                }

                return list;
                //return Enumerable.Range(0, reader1.FieldCount).ToDictionary(reader1.GetName, reader1.GetValue);
                //Console.Read();
            }
            catch (Exception e)
            {
                return null;
                Console.WriteLine(e);
            }
        }

            

        

        
    }
}
