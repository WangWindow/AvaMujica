using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaMujica.Console;

/// <summary>
/// 用于保存 API 配置信息
/// </summary>
public class ApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiBase { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
}

/// <summary>
/// 自定义调用 OpenAI API
/// </summary>
public class MyApi
{
    private readonly ApiConfig _config;
    private readonly HttpClient _httpClient;

    public MyApi(ApiConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
    }

    // 从 api.json 加载配置，若不合法则要求用户输入
    public static async Task<ApiConfig> LoadConfigAsync(string configPath = "api.json")
    {
        if (File.Exists(configPath))
        {
            try
            {
                string json = await File.ReadAllTextAsync(configPath);
                ApiConfig config = JsonSerializer.Deserialize<ApiConfig>(json) ?? new ApiConfig();
                if (IsConfigValid(config))
                {
                    return config;
                }
                throw new Exception("配置项不完整");
            }
            catch (Exception ex)
            {
                throw new Exception($"加载 {configPath} 失败: {ex.Message}");
            }
        }
        throw new FileNotFoundException($"配置文件 {configPath} 不存在");
    }

    // 调用 OpenAI API
    public async Task ChatAsync(string userPrompt, Action<string>? onReceiveToken = null)
    {
        string url = $"{_config.ApiBase}/chat/completions";

        var requestData = new
        {
            model = _config.Model,
            messages = new object[]
            {
                new { role = "system", content = _config.SystemPrompt },
                new { role = "user", content = userPrompt },
            },
            stream = true,
        };

        string requestJson = JsonSerializer.Serialize(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("data:"))
                {
                    string data = line["data:".Length..].Trim();
                    if (data == "[DONE]")
                        break;

                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(data);
                        var root = jsonDoc.RootElement;
                        if (
                            root.TryGetProperty("choices", out var choices)
                            && choices.GetArrayLength() > 0
                        )
                        {
                            var delta = choices[0].GetProperty("delta");
                            if (delta.TryGetProperty("content", out var contentElement))
                            {
                                string? token = contentElement.GetString();
                                onReceiveToken?.Invoke(token ?? string.Empty);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"解析返回数据错误: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"调用 API 失败: {ex.Message}");
        }
    }

    // 检查配置项是否完整
    private static bool IsConfigValid(ApiConfig config)
    {
        return !string.IsNullOrWhiteSpace(config.ApiKey)
            && !string.IsNullOrWhiteSpace(config.ApiBase)
            && !string.IsNullOrWhiteSpace(config.Model)
            && !string.IsNullOrWhiteSpace(config.SystemPrompt);
    }
}
