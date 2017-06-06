using System.IO;
namespace AddressBrewer.contracts
{
    public interface IBrewable
    {
        void Brew();
        FileStream CreateOutput();
    }
}
