namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Handlers
{
    using System.Collections.Generic;

    public record Result<TValue>
        where TValue : class
    {
        public bool IsSuccess { get; init; }
        public IEnumerable<TValue>? Values { get; init; }
        public string? Error { get; init; }

        public static Result<TValue> Success(IEnumerable<TValue> values)
        {
            return new Result<TValue>
            {
                IsSuccess = true,
                Values = values
            };
        }

        public static Result<TValue> Failure(string error)
        {
            return new Result<TValue>
            {
                IsSuccess = false,
                Error = error
            };
        }
    }
}
