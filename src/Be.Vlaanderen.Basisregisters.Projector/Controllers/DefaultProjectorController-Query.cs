namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;

    public abstract partial class DefaultProjectorController
    {
        private readonly Dictionary<string, string> _connectionStringBySchema = new Dictionary<string, string>();

        [HttpGet("query/{schema}/{table}")]
        public async Task<IActionResult> Query(
            [FromRoute] string schema,
            [FromRoute] string table)
        {
            // https://stackoverflow.com/a/30152027/412692
            var tableNameRegex = new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$");

            // https://stackoverflow.com/a/4978062/412692
            var columnNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

            if (!tableNameRegex.IsMatch(schema) || !tableNameRegex.IsMatch(table) || !_connectionStringBySchema.ContainsKey(schema.ToUpperInvariant()))
                return BadRequest("Invalid schema or table name.");

            if (Request.Query.Count == 0)
                return BadRequest("Please specify a filter in the query string.");

            var select = new StringBuilder($"SELECT * FROM [{schema}].[{table}] WHERE ");

            var a = Request.Query
                .ToDictionary(x => x.Key, y => (object)y.Value.ToString())
                .ToExpando();

            foreach (var keyValue in Request.Query)
            {
                if (!columnNameRegex.IsMatch(keyValue.Key))
                    return BadRequest("Invalid query parameter.");

                select.Append($"[{keyValue.Key}] = @{keyValue.Key} AND ");
            }

            select.Length -= 4;
            select.Append(";");

            IEnumerable<dynamic> result;
            await using (var connection = new SqlConnection(_connectionStringBySchema[schema.ToUpperInvariant()]))
            {
                try
                {
                    result = await connection.QueryAsync(select.ToString(), a);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 207)
                        return BadRequest("Invalid query parameter.");

                    throw;
                }
            }

            return Ok(result);
        }

        protected void RegisterConnectionString(string schema, string connectionString)
            => _connectionStringBySchema.Add(schema.ToUpperInvariant(), connectionString);
    }
}
