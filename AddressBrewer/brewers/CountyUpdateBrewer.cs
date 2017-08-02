using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using AddressBrewer.commands;
using AddressBrewer.contracts;
using AddressBrewer.models;
//using Dapper;

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
                string countyFeatClassName = "SUMMIT_ETL";  // this will be a parameter passed in on the command line tool


                // CREATE DATA TABLES FOR COUNTY ETL AND STATEWIDE ADDRESS POINTS FOR THIS COUNTY
                // GET DATA TABLE FOR COUNTY ETL 
                var dataTableCountyEtlAddrPnts = new DataTable();
                using (var con = new SqlConnection(connectionStringUTAP))
                {
                    string getCountyEtlAddrPnts = "SELECT * FROM " + countyFeatClassName;
                    con.Open();
                    //var countyUpdates = con.Query(newCountyUpdatesQuery);
                    SqlCommand command = new SqlCommand(getCountyEtlAddrPnts, con);
                    SqlDataReader reader = command.ExecuteReader();
                    //while (reader.Read())
                    //{
                    //}
                    // Load the datareader results into a datatable
                    dataTableCountyEtlAddrPnts.Load(reader);
                }
                Console.WriteLine(dataTableCountyEtlAddrPnts.Rows.Count.ToString());
                //// GET DATA TABLE FOR COUNTY'S STATEWIDE ADDRESS POINTS
                //var dataTableStatewideCoAddrPnts = new DataTable();
                //string getStatewideCoAddrPnts = "SELECT * FROM STATEWIDEADDRPNTS WHERE CountyID = " + 49043;
                //using (var con = new SqlConnection(connectionStringUTAP))
                //{
                //    con.Open();
                //    //var countyUpdates = con.Query(newCountyUpdatesQuery); // this method uses dapper library
                //    SqlCommand command = new SqlCommand(getStatewideCoAddrPnts, con);
                //    SqlDataReader reader = command.ExecuteReader();
                //    //while (reader.Read())
                //    //{
                //    //}
                //    // Load the datareader results into a datatable
                //    dataTableStatewideCoAddrPnts.Load(reader);
                //}

                
                // ITERATE THROUGH THE COUNTY ETL POINTS AND CHECK FOR CHANGES (in reverse order so we can delete rows when there's no change)
                ////foreach (DataRow row in dataTableCountyEtlAddrPnts.Rows)
                for (int i = dataTableCountyEtlAddrPnts.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = dataTableCountyEtlAddrPnts.Rows[i];
                    // 1. CHECK FOR NO CHANGE

                    // Get the geometry for the current ETL point.
                    var filteredDataTable = dataTableCountyEtlAddrPnts.Select(@"OBJECTID = " + row["OBJECTID"]);
                    string geomEtlPnt = string.Empty;
                    if (filteredDataTable.Length == 1)
                    {
                        geomEtlPnt = filteredDataTable[0]["SHAPE"].ToString();
                    }
                    //var geom = filteredDataTable[0]["SHAPE"].ToString();
                    //for (int i = 0; i < filteredDataTable.Length; i++)
                    //{
                    //    Console.WriteLine(filteredDataTable[i]["StreetName"]);
                    //    geom = filteredDataTable[0]["SHAPE"].ToString();
                    //}                    
                    //var filteredDataTable1 = dataTableStatewideCoAddrPnts.Select(@"Shape.STDistance(" + geom + ") < 1 ORDER BY Shape.STDistance(" + geom + ")");


                    // Query the StatewideAddrPnts feature class to see we get a match
                    using (var con = new SqlConnection(connectionStringUTAP))
                    {
                        string checkForStatewideAddrPntMatch = @"DECLARE @g geometry;
                                                                SET @g = geometry::STGeomFromText('" + geomEtlPnt + @"',26912);
                                                                SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, OBJECTID FROM STATEWIDEADDRPNTS
                                                                WHERE AddNum = " + row["AddNum"] + @" and
                                                                AddSystem = '" + row["AddSystem"] + @"' and
                                                                AddNumSuffix = '" + row["AddNumSuffix"] + @"' and
                                                                PrefixDir = '" + row["PrefixDir"] + @"' and
                                                                StreetName = '" + row["StreetName"] + @"' and
                                                                StreetType = '" + row["StreetType"] + @"' and
                                                                SuffixDir = '" + row["SuffixDir"] + @"' and
                                                                LandmarkName = '" + row["LandmarkName"] + @"' and
                                                                Building = '" + row["Building"] + @"' and
                                                                UnitType = '" + row["UnitType"] + @"' and
                                                                UnitID = '" + row["UnitID"] + @"' and
                                                                City = '" + row["City"] + @"' and
                                                                ZipCode = '" + row["ZipCode"] + @"' and
                                                                PtLocation = '" + row["PtLocation"] + @"' and
                                                                PtType = '" + row["PtType"] + @"' and
                                                                Structure = '" + row["Structure"] + @"' and
                                                                ParcelID = '" + row["ParcelID"] + @"' and
                                                                CountyID = '" + row["CountyID"] + @"' and
                                                                AddSource = '" + row["AddSource"] + @"' and
                                                                USNG = '" + row["USNG"] + @"' and
                                                                Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 1
                                                                ORDER BY Shape.STDistance(@g)";

                        con.Open();
                        SqlCommand command = new SqlCommand(checkForStatewideAddrPntMatch, con);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            // Remove this row/record from the datatable
                            row.Delete();

                            //UpdateLogValueEtlFeatureClass.Execute(connectionStringUTAP, countyFeatClassName, Convert.ToInt32(row["OBJECTID"]), "NC");
                            //while (reader.Read())
                            //{
                            //    Console.WriteLine(String.Format("{0}", reader["DISTANCE"]));
                            //    Console.WriteLine(String.Format("{0}", reader["UTAddPtID"]));
                            //}   
                        }
                        else
                        {
                            //UpdateLogValueEtlFeatureClass.Execute(connectionStringUTAP, countyFeatClassName, Convert.ToInt32(row["OBJECTID"]), "CHANGED");
                        }
                    }

                    //break;
                    
                }

                dataTableCountyEtlAddrPnts.AcceptChanges();
                // Reload the datatable to elimintate the records the have NC in the AGRC_LOG field
                Console.WriteLine(dataTableCountyEtlAddrPnts.Rows.Count.ToString());
                //dataTableCountyEtlAddrPnts.Reset();
                //Console.WriteLine(dataTableCountyEtlAddrPnts.Rows.Count.ToString());
                //using (var con = new SqlConnection(connectionStringUTAP))
                //{
                //    string getCountyEtlAddrPnts = "SELECT * FROM " + countyFeatClassName + " WHERE AGRC_LOG = 'CHANGED'";
                //    con.Open();
                //    //var countyUpdates = con.Query(newCountyUpdatesQuery);
                //    SqlCommand command = new SqlCommand(getCountyEtlAddrPnts, con);
                //    SqlDataReader reader = command.ExecuteReader();
                //    //while (reader.Read())
                //    //{
                //    //}
                //    // Load the datareader results into a datatable
                //    dataTableCountyEtlAddrPnts.Load(reader);
                //}
                //Console.WriteLine(dataTableCountyEtlAddrPnts.Rows.Count.ToString());



                // 2. CHECK FOR SPATIAL CHANGE (FOUND ATTRIBUTE MATCH, DIFF GEOM)

                // 3. CHECK FOR ATTRIBUTE CHANGES (EVAL NEARBY PNTS AND ROADS)

                // 4. FALIED ALL OTHER CHECKS, MUST BE NEW

                // 5. CHECK FOR DELETED POINTS (VIA LOOPING THROUGH THE SGID POINTS AND CHECKING AGAINST THE COUNTY'S PNTS)





////                        // FIND NEAREST SGID ADDRESS POINT (within a 1000 meters). 
////                        // find agrc's nearest address from the counties supplied address
////                        string nearestAgrcAddrQuery = @"DECLARE @g geometry = (select Shape from AddressPoints_FromCounty where OBJECTID = " + countyUpdate.OBJECTID + @") 
////                        SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, OBJECTID, UTAddPtID  FROM AddressPoints
////                        WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 1000
////                        ORDER BY Shape.STDistance(@g);";

////                        using (var con1 = new SqlConnection(connectionStringUTAP))
////                        {
////                            con1.Open();
////                            var findNearestAgrcAddr = con1.Query(nearestAgrcAddrQuery);

////                            var nearestAgrcAddrs = findNearestAgrcAddr as dynamic[] ?? findNearestAgrcAddr.ToArray();

////                            // AN SGID ADDRESS WAS FOUND WITHIN 1000 METERS
////                            if (nearestAgrcAddrs.Count() != 0)
////                            {
////                                // Check if the attributes match and if the distance is within 1 meter - to see if it's the same point without any changes.
////                                foreach (var nearestAgrcAddr in nearestAgrcAddrs)
////                                {
////                                    //// maybe return a list of fields that did not match
////                                    // Compare the address fields
////                                    CompareAddrPntAttributesCommand.Execute();

////                                    // Check the distance to see if it moved.


////                                    // Check if the UTAddPtIDs match, if not then the county has done an update
////                                    if (nearestAgrcAddr.UTAddPtID == countyUpdate.UTAddPtID)
////                                    {
////                                        Console.WriteLine("Matched the AGRC Nearest Address");
////                                    }
////                                    else
////                                    {
////                                        // The attributes don't match, check what has changed and update them if it apears to be the same original address.
////                                        Console.WriteLine("Did not match the AGRC Nearest Address of: " + nearestAgrcAddr.UTAddPtID);
////                                    }
////                                }
////                            }
////                            else
////                            {
////                                // it's new, validate it against the roads
////                                // Check if AddressPoint from county can be verified within a nearby road segment, so we can add it to the agrc address point database
////                                //Console.WriteLine("Counld not find nearby agrc matching address for this address, check roads to see if it's valid and we'll pass it in as a new address to the agrc address points.");
////                                Console.WriteLine("No nearby addresses found, check if it can be validated with nearby road.");

////                                Console.WriteLine(countyUpdate.Shape.ToString());
////                                string geomPoint = @"geometry::STPointFromText('" + countyUpdate.Shape.ToString() + @"', 26912)";

////                                string nearestAgrcRoadQuery =
////                                @"DECLARE @g geometry
////                                SET @g = " + geomPoint + @";
////                                SELECT TOP(1) Shape.STDistance(@g) as DISTANCE, L_F_ADD, L_T_ADD, R_F_ADD, R_T_ADD, OBJECTID, FULLNAME FROM TRANSPORTATION.ROADS
////                                WHERE Shape.STDistance(@g) is not null and Shape.STDistance(@g) < 400 and STREETNAME = '" + countyUpdate.StreetName + @"' and STREETTYPE = '" + countyUpdate.StreetType + @"'
////                                ORDER BY Shape.STDistance(@g);";

////                                using (var con2 = new SqlConnection(connectionStringSGID))
////                                {
////                                    con2.Open();
////                                    var findNearestAgrcRoad = con2.Query(nearestAgrcRoadQuery);

////                                    var nearestAgrcRoads = findNearestAgrcRoad as dynamic[] ?? findNearestAgrcRoad.ToArray();
////                                    if (nearestAgrcRoads.Count() != 0)
////                                    {
////                                        foreach (var nearestAgrcRoad in nearestAgrcRoads)
////                                        {
////                                            Console.WriteLine("Nearest SGID Road: " + nearestAgrcRoad.FULLNAME);
////                                        }
////                                    }
////                                    else
////                                    {
////                                    }
////                                }

////                            }
////                        }



                Console.WriteLine("done!");
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
