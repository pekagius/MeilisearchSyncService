using System.Text.Json;
using Dapper;
using Meilisearch;
using MeilisearchSyncService.Configuration;
using Npgsql;

namespace MeilisearchSyncService.Sync;

public abstract class BaseSyncStrategy<TDocument> : ISyncStrategy where TDocument : class
{
    private readonly string _connectionString;
    private readonly MeilisearchClient _meiliClient;
    private readonly SyncOptions _options;
    private readonly ILogger _logger;

    protected BaseSyncStrategy(string connectionString, MeilisearchClient meiliClient, SyncOptions options, ILogger logger)
    {
        _connectionString = connectionString;
        _meiliClient = meiliClient;
        _options = options;
        _logger = logger;
    }

    public abstract string IndexName { get; }
    public abstract string MartenTable { get; }
    protected abstract TDocument? MapDocument(JsonElement data);
    protected abstract Settings GetIndexSettings();

    public async Task<int> SyncIncrementalAsync(DateTimeOffset lastSync, CancellationToken ct)
    {
        _logger.LogInformation("[{Index}] Incremental sync since {LastSync}", IndexName, lastSync);
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var sql = $"SELECT data FROM {MartenTable} WHERE mt_last_modified > @lastSync ORDER BY mt_last_modified ASC";
        var rows = await conn.QueryAsync<string>(new CommandDefinition(sql, new { lastSync = lastSync.UtcDateTime }, cancellationToken: ct));
        return await UpsertDocumentsAsync(rows, ct);
    }

    public async Task<int> SyncFullAsync(CancellationToken ct)
    {
        _logger.LogInformation("[{Index}] Full sync starting", IndexName);
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var sql = $"SELECT data FROM {MartenTable} ORDER BY mt_last_modified ASC";
        var rows = await conn.QueryAsync<string>(new CommandDefinition(sql, cancellationToken: ct));
        return await UpsertDocumentsAsync(rows, ct);
    }

    public async Task ConfigureIndexAsync(CancellationToken ct)
    {
        _logger.LogInformation("[{Index}] Configuring index settings", IndexName);
        var index = _meiliClient.Index(IndexName);
        var task = await _meiliClient.CreateIndexAsync(IndexName, "id");
        await _meiliClient.WaitForTaskAsync(task.TaskUid, cancellationToken: ct);
        var settings = GetIndexSettings();
        var settingsTask = await index.UpdateSettingsAsync(settings);
        await _meiliClient.WaitForTaskAsync(settingsTask.TaskUid, cancellationToken: ct);
        _logger.LogInformation("[{Index}] Index configured successfully", IndexName);
    }

    private async Task<int> UpsertDocumentsAsync(IEnumerable<string> jsonRows, CancellationToken ct)
    {
        var documents = new List<TDocument>();
        var deletedIds = new List<string>();

        foreach (var json in jsonRows)
        {
            try
            {
                var element = JsonDocument.Parse(json).RootElement;
                if (IsDeletedDocument(element))
                {
                    var id = ExtractId(element);
                    if (id is not null) deletedIds.Add(id);
                    continue;
                }
                var doc = MapDocument(element);
                if (doc is not null) documents.Add(doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{Index}] Failed to parse document", IndexName);
            }
        }

        var totalSynced = 0;
        var index = _meiliClient.Index(IndexName);

        foreach (var batch in documents.Chunk(_options.BatchSize))
        {
            ct.ThrowIfCancellationRequested();
            var task = await index.AddDocumentsAsync(batch, cancellationToken: ct);
            await _meiliClient.WaitForTaskAsync(task.TaskUid, cancellationToken: ct);
            totalSynced += batch.Length;
        }

        if (deletedIds.Count > 0)
        {
            var deleteTask = await index.DeleteDocumentsAsync(deletedIds, ct);
            await _meiliClient.WaitForTaskAsync(deleteTask.TaskUid, cancellationToken: ct);
            _logger.LogInformation("[{Index}] Deleted {Count} documents", IndexName, deletedIds.Count);
        }

        _logger.LogInformation("[{Index}] Synced {Count} documents", IndexName, totalSynced);
        return totalSynced;
    }

    /// <summary>
    /// Check if a document is soft-deleted. Override in subclasses for different JSON casing.
    /// Default implementation checks PascalCase "IsDeleted" property.
    /// </summary>
    protected virtual bool IsDeletedDocument(JsonElement element)
    {
        if (element.TryGetProperty("IsDeleted", out var isDeleted) && isDeleted.GetBoolean())
            return true;
        return false;
    }

    /// <summary>
    /// Extract the document ID. Override in subclasses for different JSON casing.
    /// Default implementation checks PascalCase "Id" property.
    /// </summary>
    protected virtual string? ExtractId(JsonElement element)
    {
        if (element.TryGetProperty("Id", out var id))
            return id.GetGuid().ToString();
        return null;
    }
}
