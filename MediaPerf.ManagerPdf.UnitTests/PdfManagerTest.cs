using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Model.Implemenations;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Implementations;
using MediaPerf.ManagerPdf.Service.Manager.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace MediaPerf.ManagerPdf.UnitTests
{
    [TestClass]
    public class PdfManagerTest
    {

        private DataSet _adressTemple = null;
        private DataSet _royaltyFeeDataSet = null;
        private DataSet _headerDataSetTemple = null;
        private DataSet _footerDataSetTemple = null;

        private Mock<IPdfRepository> _repositoryMock;
        private Mock<IPdfManager> _serviceMock;
        private IHeaderPage _headerPage;
        private IFooterPage _footerPage;

        private IConsolidateHelper _consolidateHelper;
        private DataSet _bfpReportHistoricDataSet;

        public PdfManagerTest()
        {

        }


        [TestInitialize]
        public void Initialize()
        {
            _serviceMock = new Mock<IPdfManager>();
            _repositoryMock = new Mock<IPdfRepository>();

            _headerPage = new HeaderPage();
            _footerPage = new FooterPage();
            _consolidateHelper = new ConsolidateHelper();


            #region --   --
            _bfpReportHistoricDataSet = null;

            XDocument xDocument = null;  //GetXMLReportBFPRedevance(_bfpReportHistoric.Fk_Bfp);
            #endregion
        }

        [TestMethod]
        public void TestMethod1()
        {
            //_repositoryMock.Setup(r => r.)

            #region -- *********************** --
            DataSet dataSet = BuildPdfTemplate();

            #region -- Manage Pdf Header page -- 
            if ((_headerDataSetTemple != null) && (_headerDataSetTemple.Tables.Count > 0) &&
                (_headerDataSetTemple.Tables[0].Rows.Count > 0))
            {
                //_headerPage = _consolidateHelper.ConsolidateHeader(_headerDataSetTemple,
                //                _adressTemple,
                //                _bfpReportHistoric.Fk_S_ModeleEditionBfp,
                //                _bfpReportHistoric.Fk_S_StTarif,
                //                _headerPage);
            }
            #endregion

            #region -- Manage Pdf footer page --
            if ((_footerDataSetTemple != null) && (_footerDataSetTemple.Tables.Count > 0) &&
                (_footerDataSetTemple.Tables[0].Rows.Count != 0))
            {
                // --  Loading the pdf footer data  --
                _footerPage = _consolidateHelper.ConsolidateFooter(_footerDataSetTemple, _footerPage);
            }
            #endregion

            //_consolidateHelper.ConsolidateHeader();



            #endregion
            var mock = new Mock<IPdfRepository>();
            mock.Setup(m => m.ManagefpPPV());
            var data = mock.Object;


            //mock.Setup(p => p.CreateRoyaltyFeePdfFile(_headerPage, _footerPage)).Returns("Jignesh");
            //HomeController home = new HomeController(mock.Object);
            //string result = home.GetNameById(1);
            //Assert.AreEqual("Jignesh", result);

            //var ser = _serviceMock.
        }


        private DataSet BuildPdfTemplate()
        {
            #region -- Manage Template --
            DataTable headerTable = new DataTable();
            headerTable.Columns.Add("PathLogoEntete");
            headerTable.Columns.Add("AdresseMpf");
            headerTable.Columns.Add("Prestataire");
            headerTable.Columns.Add("AdressePrestataire");
            headerTable.Columns.Add("Destinataire");
            headerTable.Columns.Add("DateLivraison");
            headerTable.Columns.Add("DateBfp");
            headerTable.Columns.Add("IdBFP");
            headerTable.Columns.Add("BfpParam1");
            headerTable.Columns.Add("BfpParam2");
            headerTable.Columns.Add("BfpParam3");
            headerTable.Columns.Add("BfpParam4");
            headerTable.Columns.Add("BfpParam7");
            headerTable.Columns.Add("NomComplet");
            headerTable.Columns.Add("Telephone");
            headerTable.Columns.Add("Mail");
            headerTable.Columns.Add("NbPv");
            headerTable.Columns.Add("NbCmp");

            string imgPath = "https://ftp.mediaperf.com/img/logo.gif";
            byte[] byte_array = Encoding.Unicode.GetBytes(imgPath);

            MemoryStream ms4 = new MemoryStream(byte_array);
            //Image image = Image.FromStream(ms4, true, true);
            //image.Save(@"C:\Users\Administrator\Desktop\imageTest.png", System.Drawing.Imaging.ImageFormat.Png);

            headerTable.Rows.Add(byte_array, 
                "Adresse MPF", 
                "Prestataire", 
                "Adresse prestataire", 
                "Destinataire", 
                "21/11/2019", 
                "34564",
                "A l'attention du Directeur du Magasin et/ou du Service Comptable",
                "Nous vous prions  de bien vouloir établir la facture correspondante à ce relevé."  + 
                     "NOUS VOUS RAPPELONS QU'IL EST NECESSAIRE DE JOINDRE A VOTRE FACTURE LA COPIE DE CE" + 
                     "RELEVE AINSI QU'UN RIB AFIN D'EFFECTUER LE VIREMENT A 45 JOURS FIN DE MOIS.",
                "A l'attention du Service Comptable", "_",
                "Ceci n'est pas une Facture ", 
                "Nom complet",
                "01 02 03 04 05 06", 
                "Email", 
                "35", 
                "68");

            DataTable footerTable = new DataTable();
            footerTable.Columns.Add("TotalHT");
            footerTable.Columns.Add("TotalTVA");
            footerTable.Columns.Add("TotalTTC");
            footerTable.Columns.Add("BfpParam4");
            footerTable.Columns.Add("BfpParam5");
            footerTable.Columns.Add("BfpParam6");
            footerTable.Columns.Add("BfpParam7");
            footerTable.Columns.Add("BfpParam8");
            footerTable.Columns.Add("IdBFP");
            footerTable.Columns.Add("TxTva");

            footerTable.Rows.Add("18496,59",
                "3625,32",
                "22121,91", 
                "Ceci n'est pas une Facture ",
                "_", "5 quai de Dion-Bouton – 92816 Puteaux Cedex  Tél 01 40 99 21 21 – Fax 01 40 99 80 30  Société Anonyme au capital de 555.112.61€uros",
                "_", "5 quai de Dion-Bouton – 92816 Puteaux Cedex  Tél 01 40 99 21 21 – Fax 01 40 99 80 30  Société R.C. Nanterre  B 332 403 997  -  TVA Intra FR1133240397  ",
                "34564",
                "19,6");

            DataTable adressDataTable = new DataTable();
            adressDataTable.Columns.Add("Prestataire");
            adressDataTable.Columns.Add("AdressPrestataire");

            adressDataTable.Rows.Add("Mediaperformances - Recrutement", "5 QUAI DE DION BOUTON CEDEX   92806 PUTEAUX");
            #endregion

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(headerTable);
            dataSet.Tables.Add(footerTable);
            dataSet.Tables.Add(adressDataTable);

            return dataSet;
        }


        [TestMethod]
        public void CreateContact()
        {
            //// Arrange
            //var contact = Contact.CreateContact(-1, "Stephen", "Walther", "555-5555", "steve@somewhere.com");

            //// Act
            //var result = _service.CreateContact(contact);

            //// Assert
            //Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateContactRequiredFirstName()
        {
            //// Arrange
            //var contact = Contact.CreateContact(-1, string.Empty, "Walther", "555-5555", "steve@somewhere.com");

            //// Act
            //var result = _service.CreateContact(contact);

            //// Assert
            //Assert.IsFalse(result);
            //var error = _modelState["FirstName"].Errors[0];
            //Assert.AreEqual("First name is required.", error.ErrorMessage);
        }
    }
}
