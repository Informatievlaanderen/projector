namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Handlers
{
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        private static string Pattern(string left, string right) => $"{left}(.*){right}";

        private static Match Match(string source, string left, string right) => Regex.Match(source, Pattern(left, right), RegexOptions.IgnoreCase);

        private static string StringContainingBetween(this string source, string left, string right) => Match(source, left, right).Value;

        private static string Between(this string source, string left, string right) => Match(source, left, right).Groups[1].Value;

		public static string StringWithMustaches(this string source) => StringContainingBetween(source, "{{", "}}").Trim();

        public static string StringBetweenMustaches(this string source) => source.Between("{{", "}}").Trim();
	}
}
