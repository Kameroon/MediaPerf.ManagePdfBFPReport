using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using MediaPerf.ManagerPdf.Service.Manager.Contracts;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.Service.Manager.Implementations
{
    public class PdfManager : IPdfManager
    {
        // -- https://forums.asp.net/t/2000508.aspx?Pdf+File+Creation+itextsharp+multiple+user+at+sametime --
        // -- https://www.codeproject.com/Articles/691723/Csharp-Generate-and-Deliver-PDF-Files-On-Demand-fr --
        // -- https://stackoverflow.com/questions/2321526/pdfptable-as-a-header-in-itextsharp  --
        // -- https://www.davepaquette.com/archive/2018/01/22/loading-an-object-graph-with-dapper.aspx --

        #region -- Fields --
        private IHeaderPage _headerPage = null;
        private IFooterPage _footerPage = null;
        private IPdfRepository _pdfRepository = null;
        
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region -- Properties --

        #endregion

        #region --  --
        public PdfManager(IHeaderPage headerPage,
            IFooterPage footerPage,
            IPdfRepository pdfRepository)
        {
            _logger.Debug($"==> Début initialisation du Manager");
            _footerPage = footerPage;
            _headerPage = headerPage;
            _pdfRepository = pdfRepository;
            _logger.Debug($"==> Fin initialisation du Manager.");
        }
        #endregion

        #region -- Methods --
        /// <summary>
        /// --  --
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReadTextFileAsync()
        {
            return _pdfRepository.ReadTextFileAsync();            
        }
        #endregion
    }
}
