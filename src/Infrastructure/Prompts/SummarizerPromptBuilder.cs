using System.Text.Json;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;
using Infrastructure.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.AI;

namespace Infrastructure.Prompts
{
    public static class SummarizerPromptBuilder
    {
        #region Prompts for Chunk Processing

        /// <summary>
        /// (CẬP NHẬT) Lấy template cho prompt xử lý lô.
        /// Chứa hướng dẫn chi tiết về persona, ngữ cảnh, và các bước thực hiện.
        /// </summary>
        public static string GetBatchChunkPromptTemplate(InputLanguage language)
        {
            string languageName = language.GetDescription();
            // Sử dụng verbatim string (@"") để dễ đọc hơn
            return $@"You are a Senior Site Reliability Engineer (SRE) specializing in advanced log analysis.
                Your mission is to analyze log chunks to identify and summarize critical errors.

                **INSTRUCTIONS:**
                Analyze EVERY ITEM in the incoming JSON array. For each item, perform the following steps:
                1.  **Identify the Main Error:** Find the core error message or exception.
                2.  **Find the Root Cause:** Based ONLY on the provided log lines, deduce the most likely technical root cause.
                3.  **Suggest a Fix:** Provide a specific, actionable fix suggestion.
                4.  **Suggest Next Checks:** List areas or queries an engineer should check for further investigation.

                **CRITICAL OUTPUT RULES:**
                - The final response MUST be a single JSON array of `BatchSummarizeResponseItem` objects.
                - **JSON Structure:** The JSON structure and all its property names (keys) like `Summarize`, `Error`, `RootCause`, `FixSuggestion`, `NextChecks`, etc., MUST remain in English exactly as specified.
                - **Content Language:** The STRING VALUES for the `Error`, `RootCause`, `FixSuggestion`, and `NextChecks` properties MUST be written in 
                            {languageName.ToUpper()}.
                - **Formatting:** In all generated {languageName} text, wrap technical terms, type names, method names, or code snippets with backticks (`).
                - Respond ONLY with the JSON array. DO NOT add any explanations or text outside of it.";
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích MỘT LÔ chunk log.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildBatchChunkPrompt(IEnumerable<BatchSummarizeRequestItem> batch)
        {
            var jsonInput = JsonSerializer.Serialize(batch, JsonOptions.Default);
            return new[]
            {
                new ChatMessage(ChatRole.System, GetBatchChunkPromptTemplate(InputLanguage.English)),
                new ChatMessage(ChatRole.User, jsonInput)
            };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích MỘT chunk log.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildChunkPrompt(string logChunk, string chunkId = "single-chunk")
        {
            // Tạo một batch chỉ chứa một item duy nhất
            var singleItemBatch = new[]
            {
                new BatchSummarizeRequestItem { ChunkId = chunkId, LogChunk = logChunk }
            };

            return BuildBatchChunkPrompt(singleItemBatch);
        }

        #endregion

        #region Prompts for Merging and Reporting

        /// <summary>
        /// (CẬP NHẬT) Prompt hợp nhất nhiều tóm tắt thành một.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildMergePrompt(IEnumerable<SummarizerResponse> partialSummaries, InputLanguage language)
        {
            var jsonParts = partialSummaries.Select(p => JsonSerializer.Serialize(p, JsonOptions.Default));
            var joined = string.Join("\n\n---\n\n", jsonParts);

            // Prompt đã được cập nhật với yêu cầu ngôn ngữ rõ ràng hơn
            var systemPrompt = $@"You are an expert AI assistant specializing in log correlation and summarization.
                Your task is to merge multiple individual error summaries into a single, consolidated list.

                **MERGE PROCEDURE:**
                1.  **Group:** Combine items that have semantically identical `Error` fields.
                2.  **Aggregate:** When merging, SUM the `Count` values.
                3.  **Refine:** For `RootCause`, `FixSuggestion`, and `NextChecks`, synthesize the information into a single, concise, and definitive explanation from the merged items.
                4.  **Sort:** Sort the final list in descending order of `Count`.
                5.  **Format:** Wrap technical terms, filenames, and method names in the output strings with backticks (`).

                **CRITICAL OUTPUT RULE:**
                - The final JSON output object MUST be a single `SummarizerResponse` with `IntentType=Summarize`.
                - **ALL TEXTUAL CONTENT (`Error`, `RootCause`, `FixSuggestion`, `NextChecks`) MUST BE WRITTEN IN {language.GetDescription().ToUpper()}.** This is a strict requirement.
                - Respond ONLY with the JSON object, with no extra explanations.";

            return new[]
            {
        new ChatMessage(ChatRole.System, systemPrompt),
        new ChatMessage(ChatRole.User, $"Here are the individual summaries to be merged:\n{joined}")
    };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt tạo báo cáo cho Slack.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildReportPrompt(IEnumerable<SummarizerResponse> partialSummaries, string dateRange, InputLanguage language)
        {
            var joined = string.Join("\n\n---\n\n", partialSummaries.Select(p => JsonSerializer.Serialize(p.Summarize, JsonOptions.Default)));

            string languageName = language.GetDescription();

            var systemPrompt = $@"You are a DevOps communication AI.
                Your task is to create a concise daily error report suitable for posting in a Slack channel.

                **REPORT REQUIREMENTS:**
                1.  **Title:** Create a succinct title reflecting the system's general status during the provided time range.
                2.  **Error List:** List the top errors (max 5). For each, include the `Error` message and `Count`.
                3.  **Emoji:** Assign a relevant emoji for each error to indicate severity: 🔴 for critical, 🟡 for warnings.

                **CRITICAL OUTPUT RULES:**
                - The final JSON output MUST be a single `SummarizerResponse` object with `IntentType=Report`.
                - **JSON Structure:** All JSON property names (keys) like `Title`, `Errors`, `Error`, `Count`, `Emoji` MUST remain in English.
                - **Content Language:** The STRING VALUES for the `Title` and `Error` properties MUST be written in {languageName.ToUpper()}.
                - Respond ONLY with the JSON object.";

            return new[]
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, $"Time Range: {dateRange}\nSummary Data:\n{joined}")
            };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích sâu.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildAnalyzePrompt(IEnumerable<TraceLog> logs, string dateRange, InputLanguage language)
        {
            var logContent = string.Join("\n", logs.Select(l => $"[{l.Timestamp:u}] [{l.Level}] {l.Source}: {l.Message}"));

            string languageName = language.GetDescription();

            var systemPrompt = $@"You are an expert in incident investigation and root cause analysis (RCA).
                Your mission is to perform a deep analysis on the provided log sequence to identify the SINGLE most likely root cause for the main incident.

                **ANALYSIS PROCEDURE:**
                1.  **Identify Primary Incident:** Scan the logs to find the main failure point or anomalous behavior.
                2.  **Trace Backwards:** From the failure point, work backwards to find preceding events or warnings that could be causal.
                3.  **Extract Evidence:** Extract specific log lines that serve as evidence supporting your conclusion.
                4.  **Conclude Root Cause:** State a single, concise conclusion about the root cause.
                5.  **Suggest Fixes and Next Checks.**

                **CRITICAL OUTPUT RULES:**
                - The final JSON output MUST be a single `SummarizerResponse` object with `IntentType=Analyze`.
                - **JSON Structure:** All JSON property names (keys) like `Error`, `RootCause`, `Evidence`, `FixSuggestion`, `NextChecks` MUST remain in English.
                - **Content Language:** The STRING VALUES for these properties MUST be written in {languageName.ToUpper()}.
                - **Formatting:** In the generated {languageName} text, wrap technical terms with backticks (`).
                - Respond ONLY with the JSON object.";

            return new[]
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, $"Time Range: {dateRange}\nFull logs for analysis:\n{logContent}")
            };
        }

        #endregion
    }
}
