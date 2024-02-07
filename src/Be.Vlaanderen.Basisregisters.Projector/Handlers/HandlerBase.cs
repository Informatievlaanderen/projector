namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public abstract class HandlerBase
    {
        protected async Task<IEnumerable<TResult>> ExecuteQuery<TResult>(string connectionString, string cmdText, object? param = null)
        {
            await using var connection = new SqlConnection(connectionString);
            var result = await connection.QueryAsync<TResult>(cmdText, param).NoContext();

            return result;
        }
    }
}
