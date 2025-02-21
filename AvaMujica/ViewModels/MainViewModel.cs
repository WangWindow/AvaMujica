namespace AvaMujica.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to AvaMujica!";

    public string Title { get; set; } = "AvaMujica";
}
