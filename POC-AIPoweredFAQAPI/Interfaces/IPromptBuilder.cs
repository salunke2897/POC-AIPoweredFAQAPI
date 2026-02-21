using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IPromptBuilder
{
    string BuildSystemPrompt();
    string BuildUserPrompt(string question, IEnumerable<FaqItem> context);
}
