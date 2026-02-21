using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using POC_AIPoweredFAQAPI.DTOs;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Services
{
    public class FaqService : IFaqService
    {
        private readonly IPromptBuilder _promptBuilder;
        private readonly IContextRetriever _contextRetriever;
        private readonly IAiClient _aiClient;
        private readonly IConversationRepository _conversationRepository;

        public FaqService(IPromptBuilder promptBuilder, IContextRetriever contextRetriever, IAiClient aiClient, IConversationRepository conversationRepository)
        {
            _promptBuilder = promptBuilder;
            _contextRetriever = contextRetriever;
            _aiClient = aiClient;
            _conversationRepository = conversationRepository;
        }

        public async Task<FaqAskResponse> AskAsync(FaqAskRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Question is null || string.IsNullOrWhiteSpace(request.Question))
                throw new ArgumentException("Question is required");

            if (request.Question.Length > 2000) throw new ArgumentException("Question too long");

            // basic injection check
            if (Regex.IsMatch(request.Question, "<script|DROP TABLE|;--", RegexOptions.IgnoreCase))
                throw new ArgumentException("Invalid question");

            var context = await _contextRetriever.RetrieveAsync(request.Question, cancellationToken);
            var system = _promptBuilder.BuildSystemPrompt();
            var user = _promptBuilder.BuildUserPrompt(request.Question, context);

            var chatReq = new OpenAiChatRequest
            {
                Model = "gpt-5-mini",
                Messages = new List<OpenAiChatMessage> { new OpenAiChatMessage { Role = "system", Content = system }, new OpenAiChatMessage { Role = "user", Content = user } },
                // use model-appropriate parameter name - newer models expect "max_completion_tokens"
                MaxCompletionTokens = 512,
            };

            var aiResp = await _aiClient.SendChatAsync(chatReq, cancellationToken);
            var answer = aiResp.Choices.FirstOrDefault()?.Message.Content ?? "";

            var conv = new Conversation { Question = request.Question, Answer = answer };
            await _conversationRepository.AddAsync(conv, cancellationToken);

            return new FaqAskResponse { Answer = answer, Confidence = "0.8", Timestamp = DateTimeOffset.UtcNow };
        }
    }
}
