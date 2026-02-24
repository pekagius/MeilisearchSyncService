using System.Text.Json;
using Meilisearch;
using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Models;

namespace MeilisearchSyncService.Sync;

public sealed class OrganizerSyncStrategy : BaseSyncStrategy<OrganizerDocument>
{
    public OrganizerSyncStrategy(string connectionString, MeilisearchClient meiliClient, SyncOptions options, ILogger<OrganizerSyncStrategy> logger)
        : base(connectionString, meiliClient, options, logger) { }

    public override string IndexName => "organizers";
    public override string MartenTable => "public.mt_doc_organizer";

    protected override OrganizerDocument? MapDocument(JsonElement data)
    {
        var id = data.GetProperty("Id").GetGuid();
        var doc = new OrganizerDocument
        {
            Id = id.ToString(),
            Slug = GetString(data, "Slug") ?? string.Empty,
            Name = GetString(data, "Name") ?? string.Empty,
            LegalName = GetString(data, "LegalName"),
            OrganizerType = GetString(data, "OrganizerType") ?? string.Empty,
            Email = GetString(data, "Email"),
            PhoneNumber = GetString(data, "PhoneNumber"),
            WebsiteUrl = GetString(data, "WebsiteUrl"),
            City = GetString(data, "City"),
            Country = GetString(data, "Country") ?? "DE",
            LogoUrl = GetString(data, "LogoUrl"),
            CoverImageUrl = GetString(data, "CoverImageUrl"),
            Description = GetString(data, "Description"),
            IsActive = GetBool(data, "IsActive"),
            VerificationLevel = GetString(data, "VerificationLevel") ?? "Unverified",
            DataSource = GetString(data, "DataSource"),
            CreatedAtUtc = ParseTimestamp(data, "CreatedAtUtc"),
            UpdatedAtUtc = ParseTimestamp(data, "UpdatedAtUtc"),
        };

        if (data.TryGetProperty("SocialLinks", out var links) && links.ValueKind == JsonValueKind.Array)
        {
            foreach (var link in links.EnumerateArray())
            {
                doc.SocialLinks.Add(new SocialLinkDto
                {
                    Platform = GetString(link, "Platform") ?? string.Empty,
                    Url = GetString(link, "Url") ?? string.Empty,
                });
            }
        }
        return doc;
    }

    protected override Settings GetIndexSettings() => new()
    {
        SearchableAttributes = ["name", "legal_name", "slug", "description", "city"],
        FilterableAttributes = ["organizer_type", "is_active", "verification_level", "city", "country"],
        SortableAttributes = ["name", "created_at_utc"]
    };

    private static string? GetString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static bool GetBool(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;
    private static long ParseTimestamp(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(v.GetString(), out var dto)) return dto.ToUnixTimeSeconds();
        return 0;
    }
}
