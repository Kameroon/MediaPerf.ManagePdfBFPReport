using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.Service.Manager.Contracts
{
    public interface IPdfManager
    {
        Task<bool> ReadTextFileAsync();
    }
}