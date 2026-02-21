using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;
using System.Text;

namespace POC_AIPoweredFAQAPI.Services;

public class PromptBuilder : IPromptBuilder
{
    public string BuildSystemPrompt()
    {
        return "You are a helpful assistant answering FAQ questions using provided context.";
    }

    public string BuildUserPrompt(string question, IEnumerable<FaqItem> context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Use the following context to answer the question:");
        foreach (var item in context)
        {
            sb.AppendLine($"Q: {item.Question}");
            sb.AppendLine($"A: {item.Answer}");
        }
        sb.AppendLine("---");
        sb.AppendLine(question);
        return sb.ToString();
    }
}
