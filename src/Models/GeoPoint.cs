using System.Text.Json.Serialization;

namespace MeilisearchSyncService.Models;

public sealed class GeoPoint
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    public static GeoPoint? FromCoordinates(double? latitude, double? longitude)
    {
        if (latitude is null || longitude is null)
            return null;
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            return null;
        return new GeoPoint { Lat = latitude.Value, Lng = longitude.Value };
    }
}
