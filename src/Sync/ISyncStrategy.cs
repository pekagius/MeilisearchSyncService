namespace MeilisearchSyncService.Sync;

public interface ISyncStrategy
{
    string IndexName { get; }
    string MartenTable { get; }
    Task<int> SyncIncrementalAsync(DateTimeOffset lastSync, CancellationToken ct);
    Task<int> SyncFullAsync(CancellationToken ct);
    Task ConfigureIndexAsync(CancellationToken ct);
}
