using AvaMujica.Models;
using AvaMujica.Services;

// Arrange
var service = ApiService.Instance;
string testPrompt = "Hello, how are you?";

// Act
var (content, reasoning) = await service.ChatAsync(testPrompt);

// Print
Console.WriteLine($"Reasoning: {reasoning}");
Console.WriteLine($"Content: {content}");
