namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Configuration
{
    internal interface IConnectedProjectionCatchUpSettings
    {
        int CatchUpPageSize { get; }
        int CatchUpUpdatePositionMessageInterval { get; }
    }
}
