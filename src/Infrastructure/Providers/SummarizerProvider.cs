using Microsoft.Extensions.Options;
using Application.Interfaces;
using Infrastructure.AiFactory;
using Infrastructure.Options;
using Infrastructure.Services.Summarizer;

namespace Infrastructure.Providers
{
    public class SummarizerProvider
    {
        private readonly AiProcessingOptions _options;

        public SummarizerProvider(IOptions<AiProcessingOptions> options)
        {
            _options = options.Value;
        }

        public IChunkProcessorService GetChunkProcessor()
        {
            var providerName = !string.IsNullOrEmpty(_options.TaskSettings?.ChunkProcessorProvider)
            ? _options.TaskSettings.ChunkProcessorProvider
            : _options.DefaultProvider;

            return (IChunkProcessorService)CreateServiceByName(providerName);
        }

        public IMergeProcessorService GetMergeProcessor()
        {
            var providerName = !string.IsNullOrEmpty(_options.TaskSettings?.MergeProcessorProvider)
            ? _options.TaskSettings.MergeProcessorProvider
            : _options.DefaultProvider;

            return (IMergeProcessorService)CreateServiceByName(providerName);
        }

        private object CreateServiceByName(string name)
        {
            var providerConfig = _options.Providers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (providerConfig == null)
                throw new InvalidOperationException($"AI Provider with name '{name}' not found in configuration.");

            switch (providerConfig.Type)
            {
                case "AzureOpenAI":
                    var azureOptions = new AzureOpenAIOptions
                    {
                        Endpoint = providerConfig.Settings.Endpoint!,
                        ApiKey = providerConfig.Settings.ApiKey!,
                        DeploymentName = providerConfig.Settings.DeploymentName!
                    };
                    var factory = new AzureOpenAIFactory(azureOptions);
                    return new AzureSummarizerService(
                        factory,
                        providerConfig.Settings.ModelNameForTokenCount,
                        providerConfig.Settings.ModelTokenLimit,
                        providerConfig.Settings.SafeMargin
                    );

                // Thêm các case cho "Gemini", "ChatGPT"
                default:
                    throw new InvalidOperationException($"Unsupported AI provider type: '{providerConfig.Type}'");
            }
        }
    }
}
