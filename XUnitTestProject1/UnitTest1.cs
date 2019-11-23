using Autofac.Extras.Moq;
using MediaPerf.ManagerPdf.Repository.Contracts;
using System;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using (var mock = AutoMock.GetLoose())
            {

                mock.Mock<IPdfRepository>()
                    .Setup(x => x.ManagefpPPV())
                    .Returns(true);


            }
        }
    }
}
