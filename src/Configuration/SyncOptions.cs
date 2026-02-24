namespace MeilisearchSyncService.Configuration;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public int IntervalSeconds { get; set; } = 60;
    public int FullSyncIntervalMinutes { get; set; } = 60;
    public int BatchSize { get; set; } = 1000;
    public MeilisearchOptions Meilisearch { get; set; } = new();
    public DatabaseConnections Databases { get; set; } = new();
}

public sealed class MeilisearchOptions
{
    public string Url { get; set; } = "http://meilisearch.default.svc.cluster.local:7700";
    public string MasterKey { get; set; } = string.Empty;
}

public sealed class DatabaseConnections
{
    public string Events { get; set; } = string.Empty;
    public string Venues { get; set; } = string.Empty;
    public string Categories { get; set; } = string.Empty;
    public string Organizers { get; set; } = string.Empty;
}
