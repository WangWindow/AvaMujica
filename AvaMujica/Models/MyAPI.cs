/*
 * @FilePath: MyAPI.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-16 19:05:48
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-16 19:10:21
 * 2025 by WangWindow, All Rights Reserved.
 * @Descripttion:
 */
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
