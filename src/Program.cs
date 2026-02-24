using Meilisearch;
using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Sync;
using MeilisearchSyncService.Workers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog();

    var syncOptions = builder.Configuration
        .GetSection(SyncOptions.SectionName)
        .Get<SyncOptions>() ?? new SyncOptions();
    builder.Services.Configure<SyncOptions>(builder.Configuration.GetSection(SyncOptions.SectionName));

    builder.Services.AddSingleton(_ => new MeilisearchClient(
        syncOptions.Meilisearch.Url,
        syncOptions.Meilisearch.MasterKey));

    builder.Services.AddScoped<ISyncStrategy>(sp =>
        new EventSyncStrategy(
            syncOptions.Databases.Events,
            sp.GetRequiredService<MeilisearchClient>(),
            syncOptions,
            sp.GetRequiredService<ILogger<EventSyncStrategy>>()));

    builder.Services.AddScoped<ISyncStrategy>(sp =>
        new VenueSyncStrategy(
            syncOptions.Databases.Venues,
            sp.GetRequiredService<MeilisearchClient>(),
            syncOptions,
            sp.GetRequiredService<ILogger<VenueSyncStrategy>>()));

    builder.Services.AddScoped<ISyncStrategy>(sp =>
        new CategorySyncStrategy(
            syncOptions.Databases.Categories,
            sp.GetRequiredService<MeilisearchClient>(),
            syncOptions,
            sp.GetRequiredService<ILogger<CategorySyncStrategy>>()));

    builder.Services.AddScoped<ISyncStrategy>(sp =>
        new OrganizerSyncStrategy(
            syncOptions.Databases.Organizers,
            sp.GetRequiredService<MeilisearchClient>(),
            syncOptions,
            sp.GetRequiredService<ILogger<OrganizerSyncStrategy>>()));

    builder.Services.AddHostedService<SyncWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MeilisearchSyncService terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
