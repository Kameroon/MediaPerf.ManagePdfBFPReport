using FluentFTP;
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
using System.IO;
using System.Linq;
using System.Net;
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
            string pdfPath = @"C:\FTP\UserLoginTest\Rapport.txt";
            Upload(pdfPath);

            ;

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
        private static void ManageBFPReportRoyaltiesAsync()
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
            bool result = _pdfManager.ReadTextFileAsync();
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


        #region --   --

        /// <summary>  "ftp://127.0.0.1/Rapport.txt"
        /// --  --  Port FileZila Server  Host : localhost; Port: ; Password : "testUser"  uName: "testUser"
        /// </summary>
        /// <param name="filePath"></param>
        public static void Upload(string filePath)
        {
            string fTPAddress = "ftp://127.0.0.1";
            string username = "testUser";
            string password = "testUser";

            try
            {
                #region -- OK --
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(fTPAddress + "/" +
                       Path.GetFileName(filePath));

                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                FileStream stream = File.OpenRead(filePath);
                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer, 0, buffer.Length);
                stream.Close();
                #endregion

                #region -- FluentClient--

                // connect to the FTP server
                using (FtpClient client = new FtpClient())
                {
                    client.Host = fTPAddress;
                    
                    client.Credentials = new NetworkCredential(username, password);
                    client.Connect();

                    // upload a file
                    client.UploadFile(@"C:\Users\Sweet Family\Desktop\PdfFilesPath\Output.pdf", "/htdocs/big.txt");

                    //// rename the uploaded file
                    //client.Rename("/htdocs/big.txt", "/htdocs/big2.txt");

                    //// download the file again
                    //client.DownloadFile(@"C:\MyVideo_2.mp4", "/htdocs/big2.txt");

                    System.Web.UI.WebControls.FileUpload fileUpload = new System.Web.UI.WebControls.FileUpload();
                    if (fileUpload.HasFile)
                    {
                        string contentType = fileUpload.PostedFile.ContentType;
                        if (contentType == "image/jpeg")
                        {
                            int fileSize = fileUpload.PostedFile.ContentLength;

                            if (fileSize <= 5200)
                            {

                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception exception)
            {

                throw;
            }
        }

        public static void Upload2(/*FileInf*/string  file)
        {
            string fTPAddress = "ftp://127.0.0.1";
            string username = "testUser";
            string password = "testUser";

            var client = new FtpClient
            {
                Host = fTPAddress,
                Port = 21,
                Credentials = new NetworkCredential(username, password)
            };
            //client.ValidateCertificate += OnValidateCertificate;
            //client.DataConnectionType = FtpDataConnectionType.PASV;
            //client.EncryptionMode = _encryptionMode;

            //client.Connect();
            //client.SetWorkingDirectory(Path);

            //PluginFtp.UploadFile(client, file);
            //Task.InfoFormat("[PluginFTPS] file {0} sent to {1}.", file.Path, Server);

            client.Disconnect();
        }
        #endregion
    }
}
