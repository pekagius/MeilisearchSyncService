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

    /// <summary>
    /// CategoryService stores Marten JSON in camelCase (id, name, slug, …)
    /// while other services use PascalCase. This mapper handles camelCase properties.
    /// </summary>
    protected override CategoryDocument? MapDocument(JsonElement data)
    {
        var id = GetGuid(data, "id");
        if (id == Guid.Empty) return null;

        return new CategoryDocument
        {
            Id = id.ToString(),
            Slug = GetValueObjectString(data, "slug", "value") ?? string.Empty,
            Name = GetValueObjectString(data, "name", "value") ?? string.Empty,
            NameDe = GetValueObjectString(data, "nameDe", "value"),
            NameEn = GetValueObjectString(data, "nameEn", "value"),
            Description = GetValueObjectString(data, "description", "value"),
            ParentId = GetNullableGuidString(data, "parentId"),
            SortOrder = GetInt(data, "sortOrder"),
            Level = GetInt(data, "level"),
            Path = GetValueObjectString(data, "path", "value") ?? string.Empty,
            Icon = GetValueObjectString(data, "icon", "value"),
            Color = GetValueObjectString(data, "color", "value"),
            CoverImageUrl = GetString(data, "coverImageUrl"),
            IsActive = GetBool(data, "isActive"),
            IsVisible = GetBool(data, "isVisible"),
            IsFeatured = GetBool(data, "isFeatured"),
            EventCount = GetInt(data, "eventCount"),
            CreatedAtUtc = ParseTimestamp(data, "createdAtUtc"),
            UpdatedAtUtc = ParseTimestamp(data, "updatedAtUtc"),
        };
    }

    /// <summary>
    /// Override to handle camelCase "isDeleted" property.
    /// </summary>
    protected override bool IsDeletedDocument(JsonElement element)
    {
        if (element.TryGetProperty("isDeleted", out var isDeleted) && isDeleted.ValueKind == JsonValueKind.True)
            return true;
        return false;
    }

    /// <summary>
    /// Override to handle camelCase "id" property.
    /// </summary>
    protected override string? ExtractId(JsonElement element)
    {
        if (element.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            return id.GetGuid().ToString();
        return null;
    }

    protected override Settings GetIndexSettings() => new()
    {
        SearchableAttributes = ["name", "name_de", "name_en", "slug", "description", "path"],
        FilterableAttributes = ["parent_id", "level", "is_active", "is_visible", "is_featured"],
        SortableAttributes = ["sort_order", "event_count", "name"]
    };

    /// <summary>
    /// CategoryService uses value objects (CategoryName, CategorySlug, etc.)
    /// that serialize as { "value": "..." } in Marten JSONB (camelCase).
    /// </summary>
    private static string? GetValueObjectString(JsonElement el, string prop, string innerProp)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind == JsonValueKind.Null) return null;
        if (v.ValueKind == JsonValueKind.String) return v.GetString();
        if (v.ValueKind == JsonValueKind.Object && v.TryGetProperty(innerProp, out var inner)) return inner.GetString();
        return null;
    }

    private static Guid GetGuid(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String && Guid.TryParse(v.GetString(), out var guid))
            return guid;
        return Guid.Empty;
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
