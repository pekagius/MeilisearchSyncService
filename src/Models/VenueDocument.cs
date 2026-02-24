using System.Text.Json.Serialization;

namespace MeilisearchSyncService.Models;

public sealed class VenueDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("address_line1")] public string? AddressLine1 { get; set; }
    [JsonPropertyName("address_line2")] public string? AddressLine2 { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("postal_code")] public string? PostalCode { get; set; }
    [JsonPropertyName("country")] public string Country { get; set; } = "DE";
    [JsonPropertyName("_geo")] public GeoPoint? Geo { get; set; }
    [JsonPropertyName("venue_type")] public string? VenueType { get; set; }
    [JsonPropertyName("capacity")] public int? Capacity { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("website_url")] public string? WebsiteUrl { get; set; }
    [JsonPropertyName("phone_number")] public string? PhoneNumber { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("cover_image_url")] public string? CoverImageUrl { get; set; }
    [JsonPropertyName("amenities")] public List<string> Amenities { get; set; } = [];
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("is_verified")] public bool IsVerified { get; set; }
    [JsonPropertyName("data_source")] public string? DataSource { get; set; }
    [JsonPropertyName("created_at_utc")] public long CreatedAtUtc { get; set; }
    [JsonPropertyName("updated_at_utc")] public long UpdatedAtUtc { get; set; }
}
