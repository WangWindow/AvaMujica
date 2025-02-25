using System;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.ViewModels;
using Xunit;

namespace AvaMujica.Tests;

public class MainViewModelTests
{
    [Fact]
    public async Task TestApiCall()
    {
        // 准备配置
        var config = new ApiConfig
        {
            ApiKey = "sk-014c8e8d1d244f4caa57b61fd1fe8830",
            ApiBase = "https://api.deepseek.com/v1",
            Model = "deepseek-chat",
            SystemPrompt = "You are a helpful assistant.",
        };

        var api = new MyApi(config);
        string response = "";

        // 执行API调用
        await api.ChatAsync(
            "你好",
            token =>
            {
                response += token;
                Console.WriteLine($"收到响应: {token}"); // 用于调试
            }
        );

        // 验证结果
        Assert.NotEmpty(response);
    }

    [Fact]
    public void TestMainViewModelInitialization()
    {
        var viewModel = new MainViewModel();

        Assert.NotNull(viewModel.ChatMessages);
        Assert.Empty(viewModel.ChatMessages);
        Assert.False(viewModel.IsSiderOpen);
    }
}
