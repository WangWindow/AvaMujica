using AvaMujica.Console;

try
{
    // 加载API配置
    var config = await MyApi.LoadConfigAsync();
    var api = new MyApi(config);

    // 定义用户输入的提示词
    string userPrompt = "你好，请介绍一下你自己。";

    // 调用API并处理响应
    Console.WriteLine("正在等待API响应...");
    await api.ChatAsync(
        userPrompt,
        token =>
        {
            Console.Write(token); // 实时打印返回的内容
        }
    );
    Console.WriteLine("\n完成！");
}
catch (Exception ex)
{
    Console.WriteLine($"错误：{ex.Message}");
}

Console.WriteLine("按任意键退出...");
Console.ReadKey();
