using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;
using Domain.MentionParsing.Models;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using SlackNet.WebApi;
using Serilog;
using Application.Interfaces;
using Application.Models;

namespace Infrastructure.Services.FakeLogs
{
  
    public class FakeLogGenerateService: IFakeLogGenerateService
    {
        private readonly string systemMessage = "You are a helpful assistant that MUST return a single JSON object only (no extra text). " +
                        "The JSON must match this schema: { id, type, message, stackTrace, code, severity, generatedAt }. " +
                        "Use realistic programming-style stack traces and error messages. Keep stackTrace optional (null if none).";
        private readonly MentionParserOptions _options;
        public FakeLogGenerateService(IOptions<MentionParserOptions> mentionParserOptionsOption)
        {
            _options = mentionParserOptionsOption.Value;
        }
        public async Task<GeneratedError> GenerateFakeLog(string language,string severity)
        {
            var chatClient = CreateChatClient(_options);
            var userPrompt = new StringBuilder();
            userPrompt.AppendLine($"Generate one random error.");
            userPrompt.AppendLine($"Language: {language ?? "en"}");
            userPrompt.AppendLine($"Severity: {severity ?? "medium"}");
            userPrompt.AppendLine("Return EXACTLY a single JSON object and nothing else.");
            var messages= new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemMessage),
                new ChatMessage(ChatRole.User, "Generate a fake error log in JSON format.")
            };
            var response =await chatClient.GetResponseAsync<GeneratedError>(messages);            
            return response.Result;


        }
        private  IChatClient CreateChatClient(MentionParserOptions options)
        {
            var endpoint = new Uri(options.Endpoint, UriKind.Absolute);

            AzureOpenAIClient azure = new AzureOpenAIClient(endpoint, new AzureKeyCredential(options.ApiKey!));

            ChatClient azureChat = azure.GetChatClient(options.DeploymentName);

            IChatClient baseClient = azureChat.AsIChatClient();

            return new ChatClientBuilder(baseClient)
                .UseFunctionInvocation()
                .Build();
        }
    }
}
