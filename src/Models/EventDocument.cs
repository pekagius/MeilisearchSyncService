using System.Text.Json.Serialization;

namespace MeilisearchSyncService.Models;

public sealed class EventDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name_default")] public string NameDefault { get; set; } = string.Empty;
    [JsonPropertyName("name_de")] public string? NameDe { get; set; }
    [JsonPropertyName("name_en")] public string? NameEn { get; set; }
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("description_default")] public string? DescriptionDefault { get; set; }
    [JsonPropertyName("description_de")] public string? DescriptionDe { get; set; }
    [JsonPropertyName("description_en")] public string? DescriptionEn { get; set; }
    [JsonPropertyName("short_description")] public string? ShortDescription { get; set; }
    [JsonPropertyName("category_ids")] public List<string> CategoryIds { get; set; } = [];
    [JsonPropertyName("primary_category_id")] public string? PrimaryCategoryId { get; set; }
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = [];
    [JsonPropertyName("start_date_utc")] public long StartDateUtc { get; set; }
    [JsonPropertyName("end_date_utc")] public long? EndDateUtc { get; set; }
    [JsonPropertyName("timezone_id")] public string TimeZoneId { get; set; } = "UTC";
    [JsonPropertyName("attendance_mode")] public string AttendanceMode { get; set; } = "Offline";
    [JsonPropertyName("venue_id")] public string? VenueId { get; set; }
    [JsonPropertyName("venue_name")] public string? VenueName { get; set; }
    [JsonPropertyName("_geo")] public GeoPoint? Geo { get; set; }
    [JsonPropertyName("online_event_url")] public string? OnlineEventUrl { get; set; }
    [JsonPropertyName("capacity")] public int? Capacity { get; set; }
    [JsonPropertyName("organizer_id")] public string? OrganizerId { get; set; }
    [JsonPropertyName("participants_names")] public List<string> ParticipantsNames { get; set; } = [];
    [JsonPropertyName("cover_image_url")] public string? CoverImageUrl { get; set; }
    [JsonPropertyName("price_min")] public decimal? PriceMin { get; set; }
    [JsonPropertyName("price_max")] public decimal? PriceMax { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("is_free")] public bool IsFree { get; set; }
    [JsonPropertyName("min_age")] public int? MinAge { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "Draft";
    [JsonPropertyName("is_featured")] public bool IsFeatured { get; set; }
    [JsonPropertyName("is_deleted")] public bool IsDeleted { get; set; }
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("created_at_utc")] public long CreatedAtUtc { get; set; }
    [JsonPropertyName("updated_at_utc")] public long UpdatedAtUtc { get; set; }
}
