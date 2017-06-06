using System;
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

        public void Brew()
        {
            var startTime = DateTime.Now;
            Console.WriteLine("Begin creating County Update: " + DateTime.Now);

            var connectionString = _options.SdeConnectionPath;

            try
            {



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
