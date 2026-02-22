using POC_AIPoweredFAQAPI.Interfaces;
using System.Text.RegularExpressions;

namespace POC_AIPoweredFAQAPI.Services
{
    public class PromptInjectionValidator : IPromptValidator
    {
        private static readonly List<string> DangerousPatterns = new()
        {
            // Role override attempts
            "ignore previous instructions",
            "disregard above instructions",
            "act as system",
            "you are now",
            "pretend to be",
            "simulate",

            // System prompt extraction
            "show system prompt",
            "reveal hidden instructions",
            "display configuration",
            "print internal prompt",

            // Data exfiltration
            "give me api key",
            "show database password",
            "access token",
            "connection string",

            // Jailbreak style attacks
            "bypass restrictions",
            "disable safety",
            "remove guardrails",
            "developer mode"
        };

        private static readonly Regex CodeBlockRegex =
            new(@"```.*?```", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex HtmlScriptRegex =
            new(@"<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public void Validate(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Question cannot be empty.");

            if (question.Length > 2000)
                throw new ArgumentException("Question too long");

            if (!IsSafe(question))
                throw new ArgumentException("Potential prompt injection detected.");
        }

        public bool IsSafe(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return false;

            var lower = question.ToLowerInvariant();

            // Pattern matching
            if (DangerousPatterns.Any(pattern => lower.Contains(pattern)))
                return false;

            // Suspicious code blocks
            if (CodeBlockRegex.IsMatch(question))
                return false;

            // Script injection
            if (HtmlScriptRegex.IsMatch(question))
                return false;

            // Excessively long input (possible attack)
            if (question.Length > 5000)
                return false;

            return true;
        }

        public string Sanitize(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return string.Empty;

            var sanitized = question;

            // Remove code blocks
            sanitized = CodeBlockRegex.Replace(sanitized, string.Empty);

            // Remove script tags
            sanitized = HtmlScriptRegex.Replace(sanitized, string.Empty);

            // Remove dangerous phrases
            foreach (var pattern in DangerousPatterns)
            {
                sanitized = Regex.Replace(
                    sanitized,
                    Regex.Escape(pattern),
                    string.Empty,
                    RegexOptions.IgnoreCase);
            }

            return sanitized.Trim();
        }
    }
}
