using Azure.AI.OpenAI;
using Azure;
using Infrastructure.Options;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Infrastructure.AiFactory
{
    public class AzureOpenAIFactory
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;

        public AzureOpenAIFactory(AzureOpenAIOptions opts)
        {
            if (opts == null) throw new ArgumentNullException(nameof(opts));
            if (string.IsNullOrEmpty(opts.Endpoint)) throw new ArgumentException("Endpoint is required.", nameof(opts));
            if (string.IsNullOrEmpty(opts.ApiKey)) throw new ArgumentException("ApiKey is required.", nameof(opts));
            if (string.IsNullOrEmpty(opts.DeploymentName)) throw new ArgumentException("DeploymentName is required.", nameof(opts));

            // 1. Tạo Uri và credential
            var endpoint = new Uri(opts.Endpoint, UriKind.Absolute);
            var credential = new AzureKeyCredential(opts.ApiKey);

            // 2. Tạo và lưu trữ client. Client này chỉ sống trong phạm vi của factory.
            _client = new AzureOpenAIClient(endpoint, credential);

            // 3. Lưu trữ deployment name để sử dụng sau
            _deploymentName = opts.DeploymentName;
        }

        public IChatClient CreateChatClient()
        {
            return CreateClientForDeployment(_deploymentName);
        }

        private IChatClient CreateClientForDeployment(string deploymentName)
        {
            ChatClient chatClient = _client.GetChatClient(deploymentName);

            return new ChatClientBuilder(chatClient.AsIChatClient())
                .UseFunctionInvocation()
                .Build();
        }
    }
}
