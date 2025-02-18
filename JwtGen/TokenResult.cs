using System.Text.Json.Serialization;

public record TokenResult
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
}