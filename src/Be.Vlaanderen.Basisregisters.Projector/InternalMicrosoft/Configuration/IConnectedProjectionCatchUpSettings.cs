namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Configuration
{
    internal interface IConnectedProjectionCatchUpSettings
    {
        int CatchUpPageSize { get; }
        int CatchUpUpdatePositionMessageInterval { get; }
    }
}
