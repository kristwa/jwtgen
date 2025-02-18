using System.Text.Json;

internal class StorageHelper
{
    private string folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".jwtgen");
    private string path = "configs.json";
    private List<TokenConfiguration> _configurations = new();

    public StorageHelper()
    {
        EnsureAndLoadFile();
    }
    
    private string ConfigFilePath => Path.Combine(folder, path);

    private void EnsureAndLoadFile()
    {
        EnsureFile();
        Load();
    }

    private void Load()
    {
        var json = File.ReadAllText(ConfigFilePath);
        _configurations = JsonSerializer.Deserialize<List<TokenConfiguration>>(json) ?? [];
    }

    private void EnsureFile()
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        if (!File.Exists(ConfigFilePath))
        {
            File.WriteAllText(ConfigFilePath, "[]");
        }
    }


    public void Save(string name, TokenServer server, Environment environment, string clientId, string clientSecret,
        string? audience)
    {
        var config = new TokenConfiguration()
        {
            Name = name,
            TokenServer = server,
            Environment = environment,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Audience = audience
        };

        _configurations.Insert(0, config);
        SaveFile();
    }

    public bool Delete(string name)
    {
        var config = _configurations.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (config is not null)
        {
            _configurations.Remove(config);
            SaveFile();
            return true;
        }

        return false;
    }

    private void SaveFile()
    {
        var json = JsonSerializer.Serialize(_configurations);
        File.WriteAllText(ConfigFilePath, json);
    }

    public bool Exists(string name)
    {
        return _configurations.Any(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }

    public TokenConfiguration? GetConfiguration(string name)
    {
        return _configurations.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }

    public List<TokenConfiguration> ListConfigurations()
    {
        return _configurations;
    }
}