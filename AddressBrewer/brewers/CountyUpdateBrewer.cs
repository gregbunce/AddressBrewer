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
                var connectionStringSGID = @"Data Source=" + _options.SgidServer + @";Initial Catalog=" + _options.SgidDatabase + @";User ID=" + _options.SgidId + @";Password=" + _options.SgidId + @"";

                const string newCountyUpdatesQuery = "SELECT * FROM AddressPoints_FromCounty";

                // get a record set of county address point updates 
                using (var con = new SqlConnection(connectionStringUTAP))
                {
                    // open the sqlconnection
                    con.Open();

                    var countyUpdates = con.Query(newCountyUpdatesQuery);

                    // Loop through the supplied county address updates
                    foreach (var countyUpdate in countyUpdates)
                    {
                        Console.WriteLine("County Address" + countyUpdate.UTAddPtID);

                        // find agrc's nearest address from the counties supplied address
                        string nearestAgrcAddrQuery = @"DECLARE @g geometry = (select Shape from AddressPoints_FromCounty where OBJECTID = " + countyUpdate.OBJECTID + @") 
                        SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, OBJECTID, UTAddPtID  FROM AddressPoints
                        WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 400
                        ORDER BY Shape.STDistance(@g);";

                        using (var con1 = new SqlConnection(connectionStringUTAP))
                        {
                            con1.Open();
                            var findNearestAgrcAddr = con1.Query(nearestAgrcAddrQuery);

                            var nearestAgrcAddrs = findNearestAgrcAddr as dynamic[] ?? findNearestAgrcAddr.ToArray();
                            if (nearestAgrcAddrs.Count() != 0)
                            {
                                // Check if the attributes match and if the distance is within 1 meter - to see if it's the same point without any changes.
                                foreach (var nearestAgrcAddr in nearestAgrcAddrs)
                                {
                                    // Compare the address fields
                                    CompareAddrPntAttributesCommand.Execute();

                                    // Check the distance to see if it moved.


                                    // Check if the UTAddPtIDs match, if not then the county has done an update
                                    if (nearestAgrcAddr.UTAddPtID == countyUpdate.UTAddPtID)
                                    {
                                        Console.WriteLine("Matched the AGRC Nearest Address");
                                    }
                                    else
                                    {
                                        // The attributes don't match, check what has changed and update them if it apears to be the same original address.
                                        Console.WriteLine("Did not match the AGRC Nearest Address of: " + nearestAgrcAddr.UTAddPtID);
                                        
                                    }
                                }                                
                            }
                            else
                            {
                                // it's new, validate it against the roads
                                // Check if AddressPoint from county can be verified within a nearby road segment, so we can add it to the agrc address point database
                                //Console.WriteLine("Counld not find nearby agrc matching address for this address, check roads to see if it's valid and we'll pass it in as a new address to the agrc address points.");
                                Console.WriteLine("No nearby addresses found, check if it can be validated with nearby road.");

                                Console.WriteLine(countyUpdate.Shape.ToString());
                                string geomPoint = @"geometry::STPointFromText('" + countyUpdate.Shape.ToString() + @"', 26912)";

                                string nearestAgrcRoadQuery = 
                                @"DECLARE @g geometry
                                SET @g = " + geomPoint + @";
                                SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, L_F_ADD, L_T_ADD, R_F_ADD, R_T_ADD, OBJECTID, FULLNAME FROM TRANSPORTATION.ROADS
                                WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 400 and STREETNAME = '" + countyUpdate.StreetName + @"' and STREETTYPE = '" + countyUpdate.StreetType + @"'
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
