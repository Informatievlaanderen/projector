namespace Be.Vlaanderen.Basisregisters.Projector.Query.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Handlers;
    using Xunit;
    using Xunit.Abstractions;

    public class QueryHandlerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public QueryHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("AddressRegistryExtract", "Address", "Complete", "0", "")]
        [InlineData("MunicipalityRegistryExtract", "Municipality", "OfficialLanguages", "[\"dutch\",\"french\"]", "")]
        public async Task PerformQuery(string schemaName, string tableName, string filterKey, string filterValue, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            var request = new QueryRequest(connectionString, schemaName, tableName, new Dictionary<string, string?> { [filterKey] = filterValue });
            var handler = new QueryHandler();
            var response = await handler.Handle(request);

            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            _testOutputHelper.WriteLine($"Number of results: {response.Values?.Count()}");
        }
    }
}
