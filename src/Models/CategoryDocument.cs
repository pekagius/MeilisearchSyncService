using System.Text.Json.Serialization;

namespace MeilisearchSyncService.Models;

public sealed class CategoryDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("name_de")] public string? NameDe { get; set; }
    [JsonPropertyName("name_en")] public string? NameEn { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("parent_id")] public string? ParentId { get; set; }
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("cover_image_url")] public string? CoverImageUrl { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("is_visible")] public bool IsVisible { get; set; }
    [JsonPropertyName("is_featured")] public bool IsFeatured { get; set; }
    [JsonPropertyName("event_count")] public int EventCount { get; set; }
    [JsonPropertyName("created_at_utc")] public long CreatedAtUtc { get; set; }
    [JsonPropertyName("updated_at_utc")] public long UpdatedAtUtc { get; set; }
}
