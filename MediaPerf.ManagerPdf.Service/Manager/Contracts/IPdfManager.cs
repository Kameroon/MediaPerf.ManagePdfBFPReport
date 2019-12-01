using System.Threading.Tasks;
using System.Xml.Linq;

namespace MediaPerf.ManagerPdf.Service.Manager.Contracts
{
    public interface IPdfManager
    {
        //Task<bool> ReadTextFileAsync();
        bool ReadTextFileAsync();


        bool CreateRoyaltyFeePdfFile(XDocument xDocument);
    }
}