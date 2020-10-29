namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Configuration
{
    internal interface IConnectedProjectionCatchUpSettings
    {
        int CatchUpPageSize { get; }
    }
}
