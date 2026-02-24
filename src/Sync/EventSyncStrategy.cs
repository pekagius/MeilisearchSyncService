using System.Text.Json;
using Meilisearch;
using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Models;

namespace MeilisearchSyncService.Sync;

public sealed class EventSyncStrategy : BaseSyncStrategy<EventDocument>
{
    public EventSyncStrategy(string connectionString, MeilisearchClient meiliClient, SyncOptions options, ILogger<EventSyncStrategy> logger)
        : base(connectionString, meiliClient, options, logger) { }

    public override string IndexName => "events";
    public override string MartenTable => "public.mt_doc_event";

    protected override EventDocument? MapDocument(JsonElement data)
    {
        var id = data.GetProperty("Id").GetGuid();
        var doc = new EventDocument
        {
            Id = id.ToString(),
            Slug = GetString(data, "Slug") ?? string.Empty,
            Status = GetString(data, "Status") ?? "Draft",
            IsFeatured = GetBool(data, "IsFeatured"),
            IsDeleted = GetBool(data, "IsDeleted"),
            Language = GetString(data, "Language"),
            TimeZoneId = GetString(data, "TimeZoneId") ?? "UTC",
            AttendanceMode = GetString(data, "AttendanceMode") ?? "Offline",
            VenueId = GetNullableGuidString(data, "VenueId"),
            VenueName = GetString(data, "VenueName"),
            OrganizerId = GetNullableGuidString(data, "OrganizerId"),
            OnlineEventUrl = GetString(data, "OnlineEventUrl"),
            Capacity = GetNullableInt(data, "Capacity"),
            CoverImageUrl = GetString(data, "CoverImageUrl"),
            PriceMin = GetNullableDecimal(data, "PriceMin"),
            PriceMax = GetNullableDecimal(data, "PriceMax"),
            Currency = GetString(data, "Currency"),
            IsFree = GetBool(data, "IsFree"),
        };

        if (data.TryGetProperty("Name", out var name))
        {
            doc.NameDefault = GetString(name, "Default") ?? string.Empty;
            doc.NameDe = GetString(name, "De");
            doc.NameEn = GetString(name, "En");
        }

        if (data.TryGetProperty("Description", out var desc))
        {
            doc.DescriptionDefault = GetString(desc, "Default");
            doc.DescriptionDe = GetString(desc, "De");
            doc.DescriptionEn = GetString(desc, "En");
        }

        if (data.TryGetProperty("ShortDescription", out var shortDesc) && shortDesc.ValueKind != JsonValueKind.Null)
            doc.ShortDescription = GetString(shortDesc, "Default");

        if (data.TryGetProperty("StartDateUtc", out var start))
            doc.StartDateUtc = ParseTimestamp(start);
        if (data.TryGetProperty("EndDateUtc", out var end) && end.ValueKind != JsonValueKind.Null)
            doc.EndDateUtc = ParseTimestamp(end);

        doc.CreatedAtUtc = data.TryGetProperty("CreatedAtUtc", out var created) ? ParseTimestamp(created) : 0;
        doc.UpdatedAtUtc = data.TryGetProperty("UpdatedAtUtc", out var updated) ? ParseTimestamp(updated) : 0;

        doc.Geo = GeoPoint.FromCoordinates(GetNullableDouble(data, "VenueLatitude"), GetNullableDouble(data, "VenueLongitude"));

        if (data.TryGetProperty("Categories", out var cats) && cats.ValueKind == JsonValueKind.Array)
        {
            foreach (var cat in cats.EnumerateArray())
            {
                var catId = GetNullableGuidString(cat, "CategoryId");
                if (catId is not null)
                {
                    doc.CategoryIds.Add(catId);
                    if (GetBool(cat, "IsPrimary")) doc.PrimaryCategoryId = catId;
                }
            }
        }

        if (data.TryGetProperty("Tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
            foreach (var tag in tags.EnumerateArray())
                doc.Tags.Add(tag.GetString() ?? string.Empty);

        if (data.TryGetProperty("Participants", out var participants) && participants.ValueKind == JsonValueKind.Array)
        {
            foreach (var p in participants.EnumerateArray())
            {
                if (p.TryGetProperty("Name", out var pName))
                {
                    var pNameStr = pName.ValueKind == JsonValueKind.Object ? GetString(pName, "Default") : pName.GetString();
                    if (!string.IsNullOrEmpty(pNameStr)) doc.ParticipantsNames.Add(pNameStr);
                }
            }
        }

        if (data.TryGetProperty("AgeRestriction", out var age) && age.ValueKind == JsonValueKind.Object)
            doc.MinAge = GetNullableInt(age, "MinimumAge");

        return doc;
    }

    protected override Settings GetIndexSettings() => new()
    {
        SearchableAttributes = ["name_default", "name_de", "name_en", "description_default", "description_de", "description_en", "slug", "tags", "venue_name", "participants_names"],
        FilterableAttributes = ["status", "is_featured", "is_free", "attendance_mode", "category_ids", "primary_category_id", "organizer_id", "venue_id", "language", "is_deleted", "start_date_utc", "end_date_utc", "min_age", "_geo"],
        SortableAttributes = ["start_date_utc", "end_date_utc", "created_at_utc", "updated_at_utc", "price_min"]
    };

    private static string? GetString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static bool GetBool(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;
    private static int? GetNullableInt(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;
    private static double? GetNullableDouble(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;
    private static decimal? GetNullableDecimal(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : null;
    private static string? GetNullableGuidString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static long ParseTimestamp(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(el.GetString(), out var dto)) return dto.ToUnixTimeSeconds();
        if (el.ValueKind == JsonValueKind.Number) return el.GetInt64();
        return 0;
    }
}
