using FluentAssertions;
using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Model.Implemenations;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Implementations;
using MediaPerf.ManagerPdf.Service.Manager.Implementations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnitTestProject
{
    [TestFixture]
    public class NUnitTests
    {

        public IHeaderPage _headerPage = null;
        public IFooterPage _footerPage = null;
        public IRoyaltyFee _royaltyFee = null;

        public NUnitTests()
        {
            _headerPage = new HeaderPage();
            _footerPage = new FooterPage();
            _royaltyFee = new RoyaltyFee();
        }

        [Test]
        public void Test()
        {
            string path = @"C:\Users\Sweet Family\Desktop\PdfFilesPath";
            string xmlPath = @"C:\Users\Sweet Family\source\repos\ConsoleAppGeneratePDFFile\ConsoleAppITextSharpPDF\Products.xml";

            XDocument xDocument = XDocument.Load(xmlPath);

            Mock<IPdfRepository> mock = new Mock<IPdfRepository>();

            mock.Setup(x => x.CreateRoyaltyFeePdfFile(path, 
                _headerPage, 
                _footerPage, 
                xDocument)).Returns(CreateRoyaltyFeePdfFile);

            var control = new PdfManager(_headerPage,
                _footerPage, mock.Object);

            control.CreateRoyaltyFeePdfFile(xDocument);

            control.Should().NotBeNull();
        }


        private bool CreateRoyaltyFeePdfFile()
        {
            return true;
        }

    }
}
