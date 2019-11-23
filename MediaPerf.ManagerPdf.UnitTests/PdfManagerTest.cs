using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Model.Implemenations;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Service.Manager.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.Xml.Linq;

namespace MediaPerf.ManagerPdf.UnitTests
{
    [TestClass]
    public class PdfManagerTest
    {

        private Mock<IPdfRepository> _repositoryMock;
        private Mock<IPdfManager> _serviceMock;
        private IHeaderPage _headerPage;
        private IFooterPage _footerPage;

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


            #region --   --
            _bfpReportHistoricDataSet = null;

            XDocument xDocument = null;  //GetXMLReportBFPRedevance(_bfpReportHistoric.Fk_Bfp);
            #endregion
        }

        [TestMethod]
        public void TestMethod1()
        {
            //_repositoryMock.Setup(r => r.)

            var mock = new Mock<IPdfRepository>();
            mock.Setup(m => m.ManagefpPPV());
            var data = mock.Object;
            //mock.Setup(p => p.CreateRoyaltyFeePdfFile(_headerPage, _footerPage)).Returns("Jignesh");
            //HomeController home = new HomeController(mock.Object);
            //string result = home.GetNameById(1);
            //Assert.AreEqual("Jignesh", result);

            //var ser = _serviceMock.
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
