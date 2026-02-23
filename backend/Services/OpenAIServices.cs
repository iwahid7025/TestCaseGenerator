using System.Text.Json;
using OpenAI.Chat;
using Backend.Models;

namespace Backend.Services;

public class OpenAIService
{
    private readonly ChatClient _chatClient;
    private readonly string _model;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _logger = logger;
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key is not configured.");
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        _chatClient = new ChatClient(_model, apiKey);
    }

    public async Task<List<TestCaseGroup>> GenerateTestCasesAsync(string featureDescription, string? applicationName)
    {
        var prompt = BuildPrompt(featureDescription, applicationName);

        try
        {
            _logger.LogInformation("Sending request to OpenAI for test case generation.");

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(GetSystemPrompt()),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                MaxOutputTokenCount = 4000,
                Temperature = 0.3f  // Lower = more consistent/structured output
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);
            var rawJson = completion.Content[0].Text;

            _logger.LogInformation("Received response from OpenAI. Parsing...");
            return ParseOpenAIResponse(rawJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API.");
            throw;
        }
    }

    // ─── System Prompt ────────────────────────────────────────────────────────
    private static string GetSystemPrompt()
    {
        return """
            You are an expert QA engineer and test architect with 15 years of experience.
            Your job is to generate comprehensive, well-structured test cases from feature descriptions.

            RULES:
            1. Always respond with valid JSON only — no explanation, no markdown, no code fences.
            2. Group test cases into logical categories.
            3. Every test case must have clear, actionable steps.
            4. Assign realistic priorities: High, Medium, or Low.
            5. Cover all angles: happy path, negative, edge cases, boundary values, and UI/UX where relevant.
            6. Use professional QA terminology.
            7. Test case IDs must follow the pattern: TC-001, TC-002, etc.
            """;
    }

    // ─── User Prompt ─────────────────────────────────────────────────────────
    private static string BuildPrompt(string featureDescription, string? applicationName)
    {
        var appContext = string.IsNullOrWhiteSpace(applicationName)
            ? "an application"
            : $"an application called '{applicationName}'";

        return $$"""
            Generate comprehensive test cases for {{appContext}}.

            FEATURE DESCRIPTION:
            {{featureDescription}}

            Return a JSON object in EXACTLY this structure:
            {
              "groups": [
                {
                  "groupName": "Functional Tests",
                  "description": "Core functionality tests covering the main happy path",
                  "testCases": [
                    {
                      "id": "TC-001",
                      "title": "Short descriptive title",
                      "preconditions": "What must be true before running this test",
                      "steps": [
                        "Step 1: ...",
                        "Step 2: ...",
                        "Step 3: ..."
                      ],
                      "expectedResult": "What should happen if the test passes",
                      "priority": "High",
                      "testType": "Functional"
                    }
                  ]
                }
              ]
            }

            REQUIRED groups to include (add more if relevant):
            - Functional Tests (happy path, core features)
            - Negative Tests (invalid inputs, wrong data, unauthorized access)
            - Edge Cases (boundary values, empty inputs, max lengths)
            - UI/UX Tests (layout, responsiveness, accessibility basics)

            Generate at least 3-5 test cases per group. Be thorough.
            """;
    }

    // ─── Response Parser ──────────────────────────────────────────────────────
    private static List<TestCaseGroup> ParseOpenAIResponse(string rawJson)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<OpenAITestCaseRoot>(rawJson, options);

            if (parsed?.Groups == null || parsed.Groups.Count == 0)
                throw new InvalidDataException("OpenAI returned an empty or invalid response.");

            return parsed.Groups;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Failed to parse OpenAI response as JSON: {ex.Message}", ex);
        }
    }

    // ─── Internal deserialization helper ─────────────────────────────────────
    private class OpenAITestCaseRoot
    {
        public List<TestCaseGroup> Groups { get; set; } = new();
    }
}