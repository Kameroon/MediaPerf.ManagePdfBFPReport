using MediaPerf.ManagerPdf.Model.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MediaPerf.ManagerPdf.Repository.Contracts
{
    public interface IPdfRepository
    {
        bool CreateRoyaltyFeePdfFile(string repositoryPath,
           IHeaderPage headerPage,
           IFooterPage footerPage,
           XDocument royaltyFeeDataSet);

        Task<bool> ReadTextFileAsync();
    }
}
