using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Sync;
using Microsoft.Extensions.Options;

namespace MeilisearchSyncService.Workers;

public sealed class SyncWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly SyncOptions _options;
    private readonly ILogger<SyncWorker> _logger;
    private readonly Dictionary<string, DateTimeOffset> _lastSyncTimes = new();
    private DateTimeOffset _lastFullSync = DateTimeOffset.MinValue;
    private bool _indexesConfigured;

    public SyncWorker(IServiceProvider services, IOptions<SyncOptions> options, ILogger<SyncWorker> logger)
    {
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MeilisearchSyncWorker starting. Interval: {Interval}s, FullSync: {FullSync}min",
            _options.IntervalSeconds, _options.FullSyncIntervalMinutes);

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var strategies = scope.ServiceProvider.GetRequiredService<IEnumerable<ISyncStrategy>>();

                if (!_indexesConfigured)
                {
                    await ConfigureAllIndexesAsync(strategies, stoppingToken);
                    _indexesConfigured = true;
                }

                var needsFullSync = (DateTimeOffset.UtcNow - _lastFullSync).TotalMinutes >= _options.FullSyncIntervalMinutes;

                foreach (var strategy in strategies)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        if (needsFullSync || !_lastSyncTimes.ContainsKey(strategy.IndexName))
                        {
                            var count = await strategy.SyncFullAsync(stoppingToken);
                            _lastSyncTimes[strategy.IndexName] = DateTimeOffset.UtcNow;
                            _logger.LogInformation("[{Index}] Full sync completed: {Count} documents", strategy.IndexName, count);
                        }
                        else
                        {
                            var lastSync = _lastSyncTimes[strategy.IndexName];
                            var count = await strategy.SyncIncrementalAsync(lastSync, stoppingToken);
                            _lastSyncTimes[strategy.IndexName] = DateTimeOffset.UtcNow;
                            if (count > 0)
                                _logger.LogInformation("[{Index}] Incremental sync: {Count} documents", strategy.IndexName, count);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "[{Index}] Sync failed", strategy.IndexName);
                    }
                }

                if (needsFullSync) _lastFullSync = DateTimeOffset.UtcNow;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Sync cycle failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }
        _logger.LogInformation("MeilisearchSyncWorker stopping");
    }

    private async Task ConfigureAllIndexesAsync(IEnumerable<ISyncStrategy> strategies, CancellationToken ct)
    {
        _logger.LogInformation("Configuring Meilisearch indexes...");
        foreach (var strategy in strategies)
        {
            try { await strategy.ConfigureIndexAsync(ct); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Index}] Failed to configure index", strategy.IndexName);
                throw;
            }
        }
        _logger.LogInformation("All indexes configured successfully");
    }
}
