internal record TokenConfiguration
{
    public required string Name { get; init; }
    public TokenServer TokenServer { get; init; }
    public Environment Environment { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public string? Audience { get; init; }
}