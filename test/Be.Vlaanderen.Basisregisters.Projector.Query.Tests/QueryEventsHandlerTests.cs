namespace Be.Vlaanderen.Basisregisters.Projector.Query.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Handlers;
    using Xunit;
    using Xunit.Abstractions;

    public class QueryEventsHandlerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public QueryEventsHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("address-registry", "480677", "")]
        [InlineData("building-registry", "11261566", "")]
        //[InlineData("buildingunit-registry", "5667547", "")]
        [InlineData("municipality-registry", "11044", "")]
        [InlineData("parcel-registry", "12015D0290-00_000", "")]
        [InlineData("postal-registry", "1000", "")]
        [InlineData("streetname-registry", "100356", "")]
        public async Task PerformQuery(string registryName, string externalId, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            var request = new QueryEventsRequest(connectionString, registryName, externalId);
            var handler = new QueryEventsHandler();
            var response = await handler.Handle(request);

            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            _testOutputHelper.WriteLine($"Number of results: {response.Values?.Count()}\n");
            if (response.Values != null)
            {
                foreach (var responseValue in response.Values)
                {
                    _testOutputHelper.WriteLine(responseValue.ToString());
                }
            }
        }
    }
}
