namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public abstract class HandlerBase
    {
        protected async Task<IEnumerable<dynamic>> ExecuteQuery(string connectionString, string cmdText, object? param)
        {
            await using var connection = new SqlConnection(connectionString);
            var result = await connection.QueryAsync(cmdText, param);

            return result;
        }

        protected async Task<IEnumerable<QueryRow>> ExecuteQuery(string connectionString, string cmdText)
        {
            await using var connection = new SqlConnection(connectionString);
            var result = await connection.QueryAsync<QueryRow>(cmdText);

            return result;
        }
    }
}
