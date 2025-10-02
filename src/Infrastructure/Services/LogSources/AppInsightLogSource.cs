using Application.Interfaces;
using Azure.Identity;
using Azure.Monitor.Query;
using Domain.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace Infrastructure.Services.LogSources;

public class AppInsightsLogSource : ILogSourceService
{
    private readonly AppInsightsSettings _settings;
    private readonly LogsQueryClient _logsQueryClient;

    public AppInsightsLogSource(IOptions<AppInsightsSettings> settingsOptions)
    {
        _settings = settingsOptions.Value;
        var credential = new ClientSecretCredential(_settings.TenantId, _settings.ClientId, _settings.ClientSecret);
        _logsQueryClient = new LogsQueryClient(credential);
    }

    public async Task<string> GetLogsAsync(DateTimeOffset from, DateTimeOffset to, string query)
    {
        var sb = new StringBuilder();
        string workspaceResourceId = $"/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroupName}/providers/Microsoft.OperationalInsights/workspaces/{_settings.WorkspaceName}";

        // Xây dựng KQL query
        string kqlQuery = "union traces, exceptions " +
                          $"| where timestamp >= datetime({from.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}) " +
                          $"and timestamp <= datetime({to.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ})";

        if (!string.IsNullOrWhiteSpace(query))
        {
            kqlQuery += $" | where message contains '{query}' or exception contains '{query}'";
        }

        try
        {
            var response = await _logsQueryClient.QueryWorkspaceAsync(
                workspaceId: workspaceResourceId,
                query: kqlQuery,
                timeRange: new QueryTimeRange(from,to)
            );

            foreach (var table in response.Value.AllTables)
            {
                foreach (var row in table.Rows)
                {
                    string logLine = $"{table.Name}: {row["message"] ?? row["exception"] ?? "No message"}";
                    sb.AppendLine(logLine);
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error querying AppInsights: {ex.Message}");
        }

        return sb.ToString();
    }
}