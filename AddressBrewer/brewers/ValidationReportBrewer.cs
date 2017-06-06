using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddressBrewer.contracts;
using AddressBrewer.models;

namespace AddressBrewer.brewers
{
    public class ValidationReportBrewer: IBrewable
    {
        private readonly CliOptions _options;

        public ValidationReportBrewer(CliOptions options)
        {
            _options = options;
        }

        public void Brew(CliOptions options)
        {
            throw new System.NotImplementedException();
        }

        public FileStream CreateOutput()
        {
            throw new NotImplementedException();
        }

    }
}
