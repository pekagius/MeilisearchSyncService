using System.Text.Json;
using Meilisearch;
using MeilisearchSyncService.Configuration;
using MeilisearchSyncService.Models;

namespace MeilisearchSyncService.Sync;

public sealed class CategorySyncStrategy : BaseSyncStrategy<CategoryDocument>
{
    public CategorySyncStrategy(string connectionString, MeilisearchClient meiliClient, SyncOptions options, ILogger<CategorySyncStrategy> logger)
        : base(connectionString, meiliClient, options, logger) { }

    public override string IndexName => "categories";
    public override string MartenTable => "public.mt_doc_category";

    protected override CategoryDocument? MapDocument(JsonElement data)
    {
        var id = data.GetProperty("Id").GetGuid();
        return new CategoryDocument
        {
            Id = id.ToString(),
            Slug = GetValueObjectString(data, "Slug", "Value") ?? string.Empty,
            Name = GetValueObjectString(data, "Name", "Value") ?? string.Empty,
            NameDe = GetValueObjectString(data, "NameDe", "Value"),
            NameEn = GetValueObjectString(data, "NameEn", "Value"),
            Description = GetValueObjectString(data, "Description", "Value"),
            ParentId = GetNullableGuidString(data, "ParentId"),
            SortOrder = GetInt(data, "SortOrder"),
            Level = GetInt(data, "Level"),
            Path = GetValueObjectString(data, "Path", "Value") ?? string.Empty,
            Icon = GetValueObjectString(data, "Icon", "Value"),
            Color = GetValueObjectString(data, "Color", "Value"),
            CoverImageUrl = GetString(data, "CoverImageUrl"),
            IsActive = GetBool(data, "IsActive"),
            IsVisible = GetBool(data, "IsVisible"),
            IsFeatured = GetBool(data, "IsFeatured"),
            EventCount = GetInt(data, "EventCount"),
            CreatedAtUtc = ParseTimestamp(data, "CreatedAtUtc"),
            UpdatedAtUtc = ParseTimestamp(data, "UpdatedAtUtc"),
        };
    }

    protected override Settings GetIndexSettings() => new()
    {
        SearchableAttributes = ["name", "name_de", "name_en", "slug", "description", "path"],
        FilterableAttributes = ["parent_id", "level", "is_active", "is_visible", "is_featured"],
        SortableAttributes = ["sort_order", "event_count", "name"]
    };

    /// <summary>
    /// CategoryService uses value objects (CategoryName, CategorySlug, etc.)
    /// that serialize as { "Value": "..." } in Marten JSONB.
    /// </summary>
    private static string? GetValueObjectString(JsonElement el, string prop, string innerProp)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind == JsonValueKind.Null) return null;
        if (v.ValueKind == JsonValueKind.String) return v.GetString();
        if (v.ValueKind == JsonValueKind.Object && v.TryGetProperty(innerProp, out var inner)) return inner.GetString();
        return null;
    }

    private static string? GetString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static bool GetBool(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;
    private static int GetInt(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;
    private static string? GetNullableGuidString(JsonElement el, string prop) => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static long ParseTimestamp(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(v.GetString(), out var dto)) return dto.ToUnixTimeSeconds();
        return 0;
    }
}
