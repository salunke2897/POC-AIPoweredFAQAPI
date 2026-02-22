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
        private readonly IPromptValidator _promptValidator;

        public FaqService(IPromptBuilder promptBuilder, IContextRetriever contextRetriever, IAiClient aiClient, IConversationRepository conversationRepository, IPromptValidator promptValidator)
        {
            _promptBuilder = promptBuilder;
            _contextRetriever = contextRetriever;
            _aiClient = aiClient;
            _conversationRepository = conversationRepository;
            _promptValidator = promptValidator;
        }

        public async Task<FaqAskResponse> AskAsync(FaqAskRequest request, CancellationToken cancellationToken = default)
        {
            //Validate for prompt injection patterns
            _promptValidator.Validate(request.Question);
            request.Question = _promptValidator.Sanitize(request.Question);

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
