using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenServer
{
    SoaInt,
    SoaExt,
    Intra
}