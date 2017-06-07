using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using AddressBrewer.commands;
using AddressBrewer.contracts;
using AddressBrewer.models;
using Dapper;

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

                const string newCountyAddrsQuery = "SELECT * FROM AddressPoints_FromCounty where objectid > 22560";

                // get a record set of county address point updates 
                using (var con = new SqlConnection(connectionString))
                {
                    // open the sqlconnection
                    con.Open();

                    var countyAddresses = con.Query(newCountyAddrsQuery);

                    // Loop through the supplied county address updates
                    foreach (var countyAddress in countyAddresses)
                    {
                        Console.WriteLine("County Address" + countyAddress.UTAddPtID);

                        // find agrc's nearest address from the counties supplied address to check for a match
                        string nearestAgrcAddrQuery = @"DECLARE @g geometry = (select Shape from AddressPoints_FromCounty where OBJECTID = " + countyAddress.OBJECTID + @") 
                        SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, OBJECTID, UTAddPtID  FROM AddressPoints
                        WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 10 and UTAddPtID = '" + countyAddress.UTAddPtID + @"'
                        ORDER BY Shape.STDistance(@g);";

                        using (var con1 = new SqlConnection(connectionString))
                        {
                            con1.Open();
                            var findNearestAgrcAddr = con1.Query(nearestAgrcAddrQuery);

                            var nearestAgrcAddrs = findNearestAgrcAddr as dynamic[] ?? findNearestAgrcAddr.ToArray();
                            if (nearestAgrcAddrs.Count() != 0)
                            {
                                foreach (var nearestAgrcAddr in nearestAgrcAddrs)
                                {
                                    Console.WriteLine("Nearest AGRC Addr" + nearestAgrcAddr.UTAddPtID);

                                }                                
                            }
                            else
                            {
                                // Check if AddressPoint from county can be verified within a nearby road segment, so we can add it to the agrc address point database
                                Console.WriteLine("Counld not find nearby agrc matching address for this address, check roads to see if it's valid and we'll pass it in as a new address to the agrc address points.");
                            }


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
