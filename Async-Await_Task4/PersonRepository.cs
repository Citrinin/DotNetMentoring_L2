using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Async_Await_Task4
{
    public class PersonRepository
    {
        private readonly IDbConnection _dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        public List<Person> GetAll()
        {
            return _dbConnection.Query<Person>("SELECT * FROM Persons").ToList();
        }
    }
}
