namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    public class QueryHandler : HandlerBase, IRequestHandler<QueryRequest, Result<object>>
    {
        private static string? GetCmdText(string schema, string table, Dictionary<string, string?> query)
        {
            // https://stackoverflow.com/a/4978062/412692
            var columnNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

            var cmdTextBuilder = new StringBuilder($"select top 100 * from [{schema}].[{table}] where ");

            foreach (var key in query.Select(x => x.Key))
            {
                if (!columnNameRegex.IsMatch(key))
                {
                    return null;
                }

                cmdTextBuilder.Append($"[{key}] = @{key} and ");
            }

            cmdTextBuilder.Length -= 4; // remove last 'and '
            cmdTextBuilder.Append(';');
            return cmdTextBuilder.ToString();
        }

        private static object GetParam(Dictionary<string, string?> query) =>
            query
                .ToDictionary(x => x.Key, y => (object?)y.Value)!
                .ToExpando();

        public async Task<Result<object>> Handle(QueryRequest request)
        {
            // https://stackoverflow.com/a/30152027/412692
            var tableNameRegex = new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$");

            if (!tableNameRegex.IsMatch(request.Schema) || !tableNameRegex.IsMatch(request.Table))
            {
                return Result<dynamic>.Failure("Invalid schema or table name.");
            }

            if (request.Query is null || request.Query.Count == 0)
            {
                return Result<dynamic>.Failure("Please specify a filter in the query string.");
            }

            // get sql statement
            var cmdText = GetCmdText(request.Schema, request.Table, request.Query);
            if (cmdText == null)
            {
                return Result<dynamic>.Failure("Invalid query parameter.");
            }

            // get sql params
            var param = GetParam(request.Query);

            // execute sql
            try
            {
                var result = await ExecuteQuery<dynamic>(request.ConnectionString, cmdText, param).ConfigureAwait(false);
                return Result<dynamic>.Success(result);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 207)
                {
                    return Result<dynamic>.Failure("Invalid query parameter.");
                }

                throw;
            }
        }
    }

    public record QueryRequest(string ConnectionString, string Schema, string Table, Dictionary<string, string?>? Query);
}
