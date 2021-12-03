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

        // use connectionString "Server=tcp:127.0.0.1,19001;Database=REGISTRY-DATABASE;User=basisregisters;Password=ySbATyGpjfPzd4XH7nNs2SNYPrV7sKEvcmnGt6FmETD8rSkbxqEyJ32U2gafEqxn7H3WHpP6uvM7NxA3dm9YECqugx3w4r9fhxxF"

        [Theory]
        [InlineData("", "AddressRegistryExtract", "Address", "Complete", "0")]
        [InlineData("", "MunicipalityRegistryExtract", "Municipality", "OfficialLanguages", "[\"dutch\",\"french\"]")]
        public async Task PerformQuery(string connectionString, string schemaName, string tableName, string filterKey, string filterValue)
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
