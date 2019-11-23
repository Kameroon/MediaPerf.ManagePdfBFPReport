using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.Repository.RepositoryBase
{
    public class DapperRepository //: DapperRepositoryBase
    {
        private readonly string _connectionString = null;


        public DapperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
    }



    public interface IRespository<T>
    {
        List<T> GetListOfData(object param);

        IEnumerable<T> GetAllData();

        T GetSingleData(int dataId);

        int GetIntegerVlaue(int id);

        string GetStringVlaue(int id);

        bool InsertData(T ourData);

        bool DeleteData(int dataId);

        bool UpdateData(T ourData);
    }
}
