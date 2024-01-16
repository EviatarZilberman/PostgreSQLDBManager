using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQLDBManager
{
    public interface ISql<T>
    {
        public Task<T> Select(string search, string param);
/*        public string SelectString(string parameter, string search, string where);
        public T SelectById(Guid id);*/
    }
}
