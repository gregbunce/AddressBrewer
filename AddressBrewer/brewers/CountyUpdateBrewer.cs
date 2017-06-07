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
                var connectionStringUTAP = @"Persist Security Info=False;Integrated Security=true;Initial Catalog=" +_options.DatabaseName + @";server=" + _options.Server;
                var connectionStringSGID = @"Data Source=" + _options.SGIDServer + @";Initial Catalog=" + _options.SGIDDatabase + @";User ID=" + _options.SGIDID + @";Password=" + _options.SGIDID + @"";

                const string newCountyAddrsQuery = "SELECT * FROM AddressPoints_FromCounty where objectid > 22560";

                // get a record set of county address point updates 
                using (var con = new SqlConnection(connectionStringUTAP))
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
                        WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 20
                        ORDER BY Shape.STDistance(@g);";

                        using (var con1 = new SqlConnection(connectionStringUTAP))
                        {
                            con1.Open();
                            var findNearestAgrcAddr = con1.Query(nearestAgrcAddrQuery);

                            var nearestAgrcAddrs = findNearestAgrcAddr as dynamic[] ?? findNearestAgrcAddr.ToArray();
                            if (nearestAgrcAddrs.Count() != 0)
                            {
                                foreach (var nearestAgrcAddr in nearestAgrcAddrs)
                                {
                                    // Check if the UTAddPtIDs match, if not then the county has done an update
                                    if (nearestAgrcAddr.UTAddPtID == countyAddress.UTAddPtID)
                                    {
                                        Console.WriteLine("Matched the AGRC Nearest Address");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Did not match the AGRC Nearest Address of: " + nearestAgrcAddr.UTAddPtID);
                                    }
                                }                                
                            }
                            else
                            {
                                // Check if AddressPoint from county can be verified within a nearby road segment, so we can add it to the agrc address point database
                                //Console.WriteLine("Counld not find nearby agrc matching address for this address, check roads to see if it's valid and we'll pass it in as a new address to the agrc address points.");
                                Console.WriteLine("No nearby addresses found, check if it can be validated with nearby road.");

                                Console.WriteLine(countyAddress.Shape.ToString());
                                string geomPoint = @"geometry::STPointFromText('" + countyAddress.Shape.ToString() + @"', 26912)";

                                string nearestAgrcRoadQuery = 
                                @"DECLARE @g geometry
                                SET @g = " + geomPoint + @";
                                SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, L_F_ADD, L_T_ADD, R_F_ADD, R_T_ADD, OBJECTID, FULLNAME FROM TRANSPORTATION.ROADS
                                WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 400 and STREETNAME = '" + countyAddress.StreetName + @"' and STREETTYPE = '" + countyAddress.StreetType + @"'
                                ORDER BY Shape.STDistance(@g);";

                                using (var con2 = new SqlConnection(connectionStringSGID))
                                {
                                    con2.Open();
                                    var findNearestAgrcRoad = con2.Query(nearestAgrcRoadQuery);

                                    var nearestAgrcRoads = findNearestAgrcRoad as dynamic[] ?? findNearestAgrcRoad.ToArray();
                                    if (nearestAgrcRoads.Count() != 0)
                                    {
                                        foreach (var nearestAgrcRoad in nearestAgrcRoads)
                                        {
                                            Console.WriteLine("Nearest SGID Road: " + nearestAgrcRoad.FULLNAME);
                                        }
                                    }
                                    else
                                    {
                                    }
                                }




                            }
                        }
                    }
                }




                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
            }
        }



        public FileStream CreateOutput()
        {
            throw new NotImplementedException();
        }
    }
}
