using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.Repository.Helpers
{
    public class DapperHelper
    {
        public static bool ExecuteProc(SqlConnection sqlConnection, 
            string sql, List<SqlParameter> paramList = null)
        {
            try
            {
                using (SqlConnection conn = sqlConnection)
                {
                    DynamicParameters dp = new DynamicParameters();
                    if (paramList != null)
                        foreach (SqlParameter sp in paramList)
                            dp.Add(sp.ParameterName, sp.SqlValue, sp.DbType);
                    conn.Open();
                    return conn.Execute(sql, dp, commandType: CommandType.StoredProcedure) > 0;
                }
            }
            catch (Exception e)
            {
                //do logging
                return false;
            }
        }

        //public static IEnumerable<T> ExecuteProcedure<T>(this SqlConnection connection,
        //      string storedProcedure,
        //      object parameters = null,
        //      int commandTimeout = 180)
        //{
        //    try
        //    {
        //        if (connection.State != ConnectionState.Open)
        //        {
        //            connection.Close();
        //            connection.Open();
        //        }

        //        if (parameters != null)
        //        {
        //            return connection.Query<T>(storedProcedure, parameters,
        //                commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        //        }
        //        else
        //        {
        //            return connection.Query<T>(storedProcedure,
        //                commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        connection.Close();
        //        throw ex;
        //    }
        //    finally
        //    {
        //        connection.Close();
        //    }

        //}
    }

}
