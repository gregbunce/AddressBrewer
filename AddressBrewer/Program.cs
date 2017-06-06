using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AddressBrewer.brewers;
using AddressBrewer.contracts;
using AddressBrewer.models;
using  AddressBrewer.services;


namespace AddressBrewer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CliOptions options;

            try
            {
                options = ArgParserService.Parse(args);
                if (options == null)
                {
                    return;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write("address brewer: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("press any key to continue");
                Console.ReadKey();

                return;
            }

            // Connect to database.

            // brew
            IBrewable brewer;
            switch (options.OutputType)
            {
                case OutputType.CountyUpdate:
                {
                    brewer = new CountyUpdateBrewer();
                    break;
                }

                case OutputType.ValidationReport:
                {
                    brewer = new ValidationReportBrewer();
                    break;
                }

                default:
                {
                    return;
                }
            }

            brewer.Brew();


        }
    }
}
