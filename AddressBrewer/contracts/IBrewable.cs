using System.IO;
using AddressBrewer.models;

namespace AddressBrewer.contracts
{
    public interface IBrewable
    {
        void Brew(CliOptions options);
        FileStream CreateOutput();
    }
}
