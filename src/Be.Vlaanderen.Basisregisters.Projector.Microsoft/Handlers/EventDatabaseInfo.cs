namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Handlers
{
    internal class EventDatabaseInfo
    {
        public string? ExternalId { get; }
        public string? InternalId { get; }
        public string? DetailSchemaName { get; }
        public string? DetailTableName { get; }
        public string? DetailJoinColumnName { get; }
        public string? MainSchemaName { get; }
        public string? MainTableName { get; }
        public string? MainJoinColumnName { get; }
        public string? Sql { get; }

        public EventDatabaseInfo(
            string externalId,
            string internalId,
            string detailSchemaName,
            string detailTableName,
            string detailJoinColumnName,
            string mainSchemaName,
            string mainTableName,
            string mainJoinColumnName)
        {
            ExternalId = externalId;
            InternalId = internalId;
            DetailSchemaName = detailSchemaName;
            DetailTableName = detailTableName;
            DetailJoinColumnName = detailJoinColumnName;
            MainSchemaName = mainSchemaName;
            MainTableName = mainTableName;
            MainJoinColumnName = mainJoinColumnName;
        }

        public EventDatabaseInfo(string sql)
        {
            Sql = sql;
        }
    }
}
