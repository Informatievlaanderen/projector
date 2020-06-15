namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Moq;
    using SqlStreamStore.Streams;
    using Match = System.Text.RegularExpressions.Match;

    public class MessageHandlingInvalidNumberOfExecutions : Exception
    {
        public MessageHandlingInvalidNumberOfExecutions(IEnumerable<StreamMessage> messages, Times expected, int actual)
            : base(FormatMessage(messages, expected, actual))
        { }

        private static string FormatMessage(IEnumerable<StreamMessage> messages, Times expected, int actual)
        {
            var messageIds = string.Join(", ", messages.Select(message => message.StreamId));
            return $"Expected handling messages [{messageIds}] to be executed {FormatExpectation(expected)}, but were executed {actual} time{(actual == 1 ? "" : "s")}.";
        }

        private static string FormatExpectation(Times expected)
        {
            const string capitalMatch = "capital";
            const string expectedMatch = "expected";

            var regex = new Regex($@"(?<{capitalMatch}>[A-Z])|(?<{expectedMatch}>\(\d+\))");

            string ExpectationReplace(Match match)
            {
                if (match.Groups[capitalMatch].Value == match.Value)
                    return $" {match.Value.ToLower()}";
                if (match.Groups[expectedMatch].Value == match.Value)
                    return $"{Regex.Replace(match.Value, @"[\(\)]", " ")}times";

                return match.Value;
            }

            return regex
                .Replace(expected.ToString(), ExpectationReplace)
                .Trim();
        }
    }
}
