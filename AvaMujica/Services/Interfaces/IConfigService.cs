using AvaMujica.Models;

namespace AvaMujica.Services;

public interface IConfigService
{
    string? GetValue(string key, string? defaultValue = null);
    void SetConfig(string key, string value);
    Config LoadFullConfig();
    void SaveFullConfig(Config config);
}
