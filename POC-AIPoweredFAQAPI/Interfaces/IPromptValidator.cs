namespace POC_AIPoweredFAQAPI.Interfaces
{
    public interface IPromptValidator
    {
        /// <summary>
        /// Validate the incoming user question and throw an ArgumentException when invalid.
        /// </summary>
        /// <param name="question">User supplied question text.</param>
        void Validate(string question);

        /// <summary>
        /// Returns true when the question appears safe.
        /// </summary>
        bool IsSafe(string question);

        /// <summary>
        /// Returns a sanitized version of the question (best-effort).
        /// </summary>
        string Sanitize(string question);
    }
}
