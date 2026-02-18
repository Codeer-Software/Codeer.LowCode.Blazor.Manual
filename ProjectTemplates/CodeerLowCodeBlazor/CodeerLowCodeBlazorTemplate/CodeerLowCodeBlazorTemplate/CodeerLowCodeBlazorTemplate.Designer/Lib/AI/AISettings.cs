namespace CodeerLowCodeBlazorTemplate.Designer.Lib.AI
{
    public class AISettings
    {
        public string OpenAIEndPoint { get; set; } = string.Empty;
        public string OpenAIKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;

        public static AISettings Instance { get; } = new();
    }
}
