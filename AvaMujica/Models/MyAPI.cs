namespace AvaMujica.Models;

public class MyApi
{
    public string ApiKey { get; set; } = "YourAPIKey";
    public string ApiUrl { get; set; } = "https://api.example.com";

    public void SetApi(string apiKey, string apiUrl)
    {
        ApiKey = apiKey;
        ApiUrl = apiUrl;
    }

    public void ResetApi()
    {
        ApiKey = "YourAPIKey";
        ApiUrl = "https://api.example.com";
    }
}
