namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using FluentAssertions.Collections;
    using TestProjections.Messages;

    public static class FluentAssertionExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> ContainAll<T>(this GenericCollectionAssertions<T> actual, IReadOnlyList<IEvent> expected, Func<T, IEvent, int, bool> containsMessage)
        {
            var constraint = actual.HaveCount(expected.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var index = i;
                constraint = constraint.And.Contain(e => containsMessage(e, expected[index], index));
            }

            return constraint;
        }
    }
}
