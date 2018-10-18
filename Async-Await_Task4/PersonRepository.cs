using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Async_Await_Task4
{
    public class PersonRepository
    {
        private readonly IDbConnection _dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PersonsDatabaseConnectionString"].ConnectionString);

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _dbConnection.QueryAsync<Person>("SELECT * FROM Persons");
        }

        public async Task<Person> AddAsync(Person person)
        {
            var sqlQuery =
                "INSERT INTO Persons(FirstName,LastName,Age) values(@FirstName,@LastName,@Age); SELECT CAST(SCOPE_IDENTITY() as int)";
            var returnId = await this._dbConnection.QueryAsync<int>(sqlQuery, person);
            person.Id = returnId.SingleOrDefault();

            return person;
        }

        public async Task<bool> UpdateAsync(Person person, string columnName)
        {
            var sqlQuery = $"update Persons set {columnName}=@{columnName} Where Id=@Id";
            var affectedrows = await this._dbConnection.ExecuteAsync(sqlQuery, person);

            return affectedrows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sqlQuery = "Delete from Persons where Id=@Id";
            var affectedrows = await this._dbConnection.ExecuteAsync(sqlQuery, new { Id = id });

            return affectedrows > 0;
        }
    }
}
