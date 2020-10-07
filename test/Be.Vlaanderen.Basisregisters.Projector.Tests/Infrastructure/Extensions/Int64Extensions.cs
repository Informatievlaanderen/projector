namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;

    public static class Int64Extensions
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        public static long WithMinimumValueOf(this long value, long minimum)
            => Math.Max(value, minimum);

        public static long WithMaximumValueOf(this long value, long maximum)
            => Math.Min(value, maximum);

        public static long CreateRandomLowerValue(this long value)
        {
            var maxSubtract = (int)(long.MaxValue - Math.Abs(value)).WithMaximumValueOf(100);
            return value - Random.Next(0, maxSubtract);
        }

        public static long CreateRandomHigherValue(this long value)
        {
            var maxAdd = (int)(long.MaxValue - Math.Abs(value)).WithMaximumValueOf(100);
            return value + Random.Next(0, maxAdd);
        }
    }
}
