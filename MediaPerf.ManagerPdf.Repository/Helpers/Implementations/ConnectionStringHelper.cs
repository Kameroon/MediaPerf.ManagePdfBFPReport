
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using NLog;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MediaPerf.ManagerPdf.Repository.Helpers.Implementations
{
    public class ConnectionStringHelper : IConnectionStringHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string GetConnectionString()
        {
            _logger.Debug($"==> Debut récupération de chaine de connection");
            string connetionString = @"Data Source=srv-sqlusi14\usi;Initial Catalog=USI_Recette;Persist Security Info=True;User ID=usiuser;Password=usiuser;Application Name=Bfp; Connect Timeout=0;";

            if (ConfigurationManager.ConnectionStrings["MyDbConnectionString"] == null)
            {
                return connetionString;
            }
            _logger.Debug($"==> Fin récupération de chaine de connection");

            return connetionString;
            //return ConfigurationManager.ConnectionStrings["MyDbConnectionString"].ConnectionString;
        }

        /// <summary>
        /// -- !!!!!!!!!!!!!!!!!!!!!!!!! --
        /// </summary>
        /// <returns></returns>
        public SqlConnection CreatDataBaseConnection()
        {
            try
            {
                string connetionString = @"Data Source=srv-sqlusi14\usi;Initial Catalog=USI_Recette;Persist Security Info=True;User ID=usiuser;Password=usiuser;Application Name=Bfp; Connect Timeout=0;";

                SqlConnection myConn = new SqlConnection(connetionString);

                return myConn;
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.ToString());
                throw;
            }
        }
    }
}
