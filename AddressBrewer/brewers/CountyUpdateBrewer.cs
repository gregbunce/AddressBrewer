using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using AddressBrewer.contracts;
using AddressBrewer.models;

namespace AddressBrewer.brewers
{
    public class CountyUpdateBrewer: IBrewable
    {
        private readonly CliOptions _options;

        public CountyUpdateBrewer(CliOptions options)
        {
            _options = options;
        }

        public void Brew(CliOptions options)
        {
            var startTime = DateTime.Now;
            Console.WriteLine("Begin creating County Update: " + DateTime.Now);

            try
            {
                var connectionString = @"Persist Security Info=False;Integrated Security=true;Initial Catalog=" +_options.DatabaseName + @";server=" + _options.Server;

                const string countyUpdates = "SELECT * FROM ADDRPTNS_COUNTYGDB";

                // get a record set of county address point updates 
                using (var con1 = new SqlConnection(connectionString))
                {
                    // open the sqlconnection
                    con1.Open();

                    // create a sqlcommand - allowing for a subset of records from the table
                    using (var command1 = new SqlCommand(countyUpdates, con1))

                    // create a sqldatareader
                    using (var reader1 = command1.ExecuteReader())
                    {
                        if (!reader1.HasRows) return;
                        // loop through the record set
                        while (reader1.Read())
                        {
                            Console.WriteLine(reader1["FullAdd"].ToString());







                        }
                        
                    }
                }


                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        public FileStream CreateOutput()
        {
            throw new NotImplementedException();
        }
    }
}
