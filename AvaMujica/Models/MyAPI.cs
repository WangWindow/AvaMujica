namespace AvaMujica.Models;

public class MyAPI
{
    public string APIKey { get; set; } = "YourAPIKey";
    public string APIUrl { get; set; } = "https://api.example.com";

    public void SetAPI(string apiKey, string apiUrl)
    {
        APIKey = apiKey;
        APIUrl = apiUrl;
    }

    public void ResetAPI()
    {
        APIKey = "YourAPIKey";
        APIUrl = "https://api.example.com";
    }
}
