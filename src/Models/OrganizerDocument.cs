using System.Text.Json.Serialization;

namespace MeilisearchSyncService.Models;

public sealed class OrganizerDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("legal_name")] public string? LegalName { get; set; }
    [JsonPropertyName("organizer_type")] public string OrganizerType { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("phone_number")] public string? PhoneNumber { get; set; }
    [JsonPropertyName("website_url")] public string? WebsiteUrl { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("country")] public string Country { get; set; } = "DE";
    [JsonPropertyName("logo_url")] public string? LogoUrl { get; set; }
    [JsonPropertyName("cover_image_url")] public string? CoverImageUrl { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("social_links")] public List<SocialLinkDto> SocialLinks { get; set; } = [];
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("verification_level")] public string VerificationLevel { get; set; } = "Unverified";
    [JsonPropertyName("data_source")] public string? DataSource { get; set; }
    [JsonPropertyName("created_at_utc")] public long CreatedAtUtc { get; set; }
    [JsonPropertyName("updated_at_utc")] public long UpdatedAtUtc { get; set; }
}

public sealed class SocialLinkDto
{
    [JsonPropertyName("platform")] public string Platform { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
}
