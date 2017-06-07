using System;
using System.IO;
using Mono.Options;
using AddressBrewer.models;

namespace AddressBrewer.services
{
    internal static class ArgParserService
    {
        internal static CliOptions Parse(string[] args)
        {
            var options = new CliOptions();
            var showHelp = false;

            var p = new OptionSet
            {
                {
                    "c|connection=", 
                    "the path to the .sde connection file for the database containing roads. eg: c:\\sgid.sde",
                    v => options.SdeConnectionPath = v
                },
                {
                    "d|database=", "REQUIRED. the name of the database containing the address points. eg: c:\\sgid.sde",
                    v => options.DatabaseName = v
                },
                {
                    "s|server=", "REQUIRED. the name of the server containing the database with the address points. eg: c:\\sgid.sde",
                    v => options.Server = v
                },
                {
                    "ss|sgidserver=", "REQUIRED. the name of the server containing the database with the address points. eg: c:\\sgid.sde",
                    v => options.SGIDServer = v
                },
                {
                    "sd|sgiddatabase=", "REQUIRED. the name of the server containing the database with the address points. eg: c:\\sgid.sde",
                    v => options.SGIDDatabase = v
                },
                {
                    "si|sgidID=", "REQUIRED. the name of the server containing the database with the address points. eg: c:\\sgid.sde",
                    v => options.SGIDID = v
                },
                {
                    "t|type=", "REQUIRED. The output type you want to grind. `CountyUpdate`, `ValidationReport`, ...",
                    v => options.OutputType = (OutputType) Enum.Parse(typeof (OutputType), v)
                },
                {
                    "o|output=",
                    "the location to save the output of this tool. eg: c:\\temp. Defaults to current working directory.",
                    v => options.OutputFile = v
                },
                {
                    "v", "increase the debug message verbosity.",
                    v =>
                    {
                        if (v != null)
                        {
                            options.Verbose = true;
                        }
                    }
                },
                {
                    "h|help", "show this message and exit",
                    v => showHelp = v != null
                }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("Address Brewer: ");
                Console.WriteLine(e.Message);
                ShowHelp(p);

                return null;
            }

            if (showHelp)
            {
                ShowHelp(p);

                return null;
            }

            if (string.IsNullOrEmpty(options.DatabaseName))
            {
                throw new InvalidOperationException(
                    "Missing required option -d for the name of the database.");
            }

            if (string.IsNullOrEmpty(options.Server))
            {
                throw new InvalidOperationException(
                    "Missing required option -s for the name of the server.");
            }

            //if (string.IsNullOrEmpty(options.SdeConnectionPath))
            //{
            //    throw new InvalidOperationException(
            //        "Missing required option -c for the location of the sde connection file.");
            //}

            //if (!new FileInfo(options.SdeConnectionPath).Exists)
            //{
            //    var cwd = Directory.GetCurrentDirectory();
            //    var location = Path.Combine(cwd, options.SdeConnectionPath.TrimStart('\\'));

            //    if (!new FileInfo(location).Exists)
            //    {
            //        throw new InvalidOperationException("The location for the sde file path is not found.");
            //    }

            //    options.SdeConnectionPath = location;
            //}

            if (showHelp)
            {
                ShowHelp(p);
            }

            return options;
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: address brewer [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Options:");

            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
