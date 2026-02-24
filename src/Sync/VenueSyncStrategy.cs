using System.Text.Json;
using Meilisearch;
using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Models;

namespace MeilisearchSyncService.Sync;

public sealed class VenueSyncStrategy : BaseSyncStrategy<VenueDocument>
{
    public VenueSyncStrategy(string connectionString, MeilisearchClient meiliClient, SyncOptions options, ILogger<VenueSyncStrategy> logger)
        : base(connectionString, meiliClient, options, logger) { }

    public override string IndexName => "venues";
    public override string MartenTable => "public.mt_doc_venue";

    protected override VenueDocument? MapDocument(JsonElement data)
    {
        var id = data.GetProperty("Id").GetGuid();
        return new VenueDocument
        {
            Id = id.ToString(),
            Name = GetString(data, "Name") ?? string.Empty,
            Slug = GetString(data, "Slug") ?? string.Empty,
            AddressLine1 = GetString(data, "AddressLine1"),
            AddressLine2 = GetString(data, "AddressLine2"),
            City = GetString(data, "City"),
            PostalCode = GetString(data, "PostalCode"),
            Country = GetString(data, "Country") ?? "DE",
            Geo = GeoPoint.FromCoordinates(GetNullableDouble(data, "Latitude"), GetNullableDouble(data, "Longitude")),
            VenueType = GetString(data, "VenueType"),
            Capacity = GetNullableInt(data, "Capacity"),
            Description = GetString(data, "Description"),
            WebsiteUrl = GetString(data, "WebsiteUrl"),
            PhoneNumber = GetString(data, "PhoneNumber"),
            Email = GetString(data, "Email"),
            CoverImageUrl = GetString(data, "CoverImageUrl"),
            Amenities = GetStringList(data, "Amenities"),
            IsActive = GetBool(data, "IsActive"),
            IsVerified = GetBool(data, "IsVerified"),
            DataSource = GetString(data, "DataSource"),
            CreatedAtUtc = ParseTimestamp(data, "CreatedAtUtc"),
            UpdatedAtUtc = ParseTimestamp(data, "UpdatedAtUtc"),
        };
    }

    protected override Settings GetIndexSettings() => new()
    {
        SearchableAttributes = ["name", "slug", "city", "description", "address_line1", "amenities"],
        FilterableAttributes = ["venue_type", "is_active", "is_verified", "city", "country", "capacity", "_geo"],
        SortableAttributes = ["name", "created_at_utc", "capacity"]
    };

    private static string? GetString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static bool GetBool(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;
    private static int? GetNullableInt(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;
    private static double? GetNullableDouble(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;
    private static List<string> GetStringList(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array) return [];
        return arr.EnumerateArray().Where(a => a.ValueKind == JsonValueKind.String).Select(a => a.GetString()!).ToList();
    }
    private static long ParseTimestamp(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(v.GetString(), out var dto)) return dto.ToUnixTimeSeconds();
        return 0;
    }
}
