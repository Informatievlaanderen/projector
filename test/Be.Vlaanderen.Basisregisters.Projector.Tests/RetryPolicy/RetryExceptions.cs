namespace Be.Vlaanderen.Basisregisters.Projector.Tests.RetryPolicy
{
    using System;

    internal class RetryException : Exception { }

    internal class DoNotRetryException : Exception { }
}
