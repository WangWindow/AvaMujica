using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using Xunit;

namespace AvaMujica.Test;

public class ApiTests
{
    [Fact]
    public async Task ChatAsync_WithValidPrompt_ShouldReturnContent()
    {
        // Arrange
        var service = ApiService.Instance;
        string testPrompt = "Hello, how are you?";

        // Act
        var (content, reasoning) = await service.ChatAsync(testPrompt);

        // Assert
        Assert.NotNull(content);
        Assert.NotEmpty(content);

        Assert.NotNull(reasoning);
        Assert.NotEmpty(reasoning);
    }
}
