using MediaPerf.ManagerPdf.Infrastrure.Contracts;
using MediaPerf.ManagerPdf.Infrastrure.Implementations;
using MediaPerf.ManagerPdf.MailService.Contracts;
using MediaPerf.ManagerPdf.MailService.Implementations;
using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Model.Implemenations;
using MediaPerf.ManagerPdf.Model.Implementations;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Implementations;
using MediaPerf.ManagerPdf.Repository.Implementations;
using MediaPerf.ManagerPdf.Service.Manager.Contracts;
using MediaPerf.ManagerPdf.Service.Manager.Implementations;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace ConsoleApp.App
{
    class Program
    {
        #region -- Fields --
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static IHeaderPage _headerPage = null;
        private static IPdfRepository _pdfRepository = null;
        private static IPdfManager _pdfManager = null;
        private static IFooterPage _footerPage = null;
        private static IConsolidateHelper _consolidateHelper = null; 
        #endregion

        static void Main(string[] args)
        {
            _logger.Debug($"****** Démarrage de l'application. [Nom de la machine :] {Environment.MachineName} ********");
            // -- Call initialize --
            Initialize();

            // --  -
            ManageBFPReportRoyaltiesAsync();

            Console.WriteLine("The End ...");

            _logger.Debug($"****** Fermeture de l'application. [Nom de la machine :] {Environment.MachineName} ********");
        }


        #region -- Methods --
        /// <summary>
        /// --   --
        /// </summary>
        private static async void ManageBFPReportRoyaltiesAsync()
        {
            string pdfPath = @"C:\Users\Sweet Family\Desktop\PdfFilesPath";
            pdfPath = @"C:\Users\mMABOU\Desktop\PDFFiles";

            _logger.Debug($"==> Début création du Manager");
            _pdfManager = new PdfManager(_headerPage, _footerPage, _pdfRepository);
            _logger.Debug($"==> Fin création du Manager.");

            Console.WriteLine("\r\n Lancement de l'application.\r\n");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _logger.Debug($"==> Début lecture de la liste des redevance à gérer.");
            bool result = await _pdfManager.ReadTextFileAsync();
            _logger.Debug($"==> Fin lecture de la liste des redevance à gérer.");

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine("\r\n Temps mis pour : \r\n - Récupération des de la selection \r\n - La génération du PDF \r\n - L'envoi de celui-ci par mail : " + Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));
        }
        #endregion

        #region -- Initialisation --
        /// <summary>
        /// -- Gestion des différentes dépendances
        /// -- Initilisation et enregistrement des differents types dans le container --  
        /// </summary>
        private static void Initialize()
        {
            _logger.Debug($"==> Début configuration des differents containers.");
            var container = new UnityContainer();
            container.RegisterType<IHeaderPage, HeaderPage>();
            container.RegisterType<IFooterPage, FooterPage>();
            container.RegisterType<IRoyaltyFee, RoyaltyFee>();
            container.RegisterType<IPdfManager, PdfManager>();
            container.RegisterType<IMailTemplate, MailTemplate>();
            container.RegisterType<IDialogService, DialogService>();
            container.RegisterType<IContactRedevance, ContactRedevance>();
            container.RegisterType<IBfpReportHistoric, BfpReportHistoric>();
            container.RegisterType<IConsolidateHelper, ConsolidateHelper>();
            container.RegisterType<IEmailMessageService, EmailMessageService>();
            container.RegisterType<IConnectionStringHelper, ConnectionStringHelper>();

            // --  --
            _pdfRepository = container.Resolve<PdfRepository>();
            _consolidateHelper = container.Resolve<ConsolidateHelper>();

            _logger.Debug($"==> Fin configuration des differents containers.");
        }
        #endregion
    }
}
