namespace WebApp.Services.AI
{
    public class AISettings
    {
        public string OpenAIEndPoint { get; set; } = string.Empty;
        public string OpenAIKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;
        public string DocumentAnalysisEndPoint { get; set; } = string.Empty;
        public string DocumentAnalysisKey { get; set; } = string.Empty;
    }
}
