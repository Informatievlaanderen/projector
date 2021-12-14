namespace Be.Vlaanderen.Basisregisters.Projector.Query.Tests
{
    using Handlers;
    using Xunit;

    public class StringExtensionsTests
    {
        private const string Pattern = "PersistentLocalId = {{ :businessId: }}";

        [Fact]
        public void FindStringContainingMustaches()
        {
            var result = Pattern.StringWithMustaches();
            Assert.Equal("{{ :businessId: }}", result);
        }

        [Fact]
        public void FindStringBetweenMustaches()
        {
            var result = Pattern.StringBetweenMustaches();
            Assert.Equal(":businessId:", result);
        }
    }
}
