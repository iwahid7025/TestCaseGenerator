using Backend.Models;

namespace Backend.Services;

public class TestCaseService
{
    private readonly OpenAIService _openAIService;
    private readonly ILogger<TestCaseService> _logger;

    public TestCaseService(OpenAIService openAIService, ILogger<TestCaseService> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<TestCaseResponse> GenerateAsync(TestCaseRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Generating test cases for: {AppName}",
                request.ApplicationName ?? "Unnamed App"
            );

            var groups = await _openAIService.GenerateTestCasesAsync(
                request.FeatureDescription,
                request.ApplicationName
            );

            var totalCount = groups.Sum(g => g.TestCases.Count);

            return new TestCaseResponse
            {
                Success = true,
                Groups = groups,
                TotalTestCases = totalCount,
                ApplicationName = request.ApplicationName ?? "Unnamed Application",
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test case generation failed.");
            return new TestCaseResponse
            {
                Success = false,
                ErrorMessage = "Failed to generate test cases. Please try again.",
                Groups = new List<TestCaseGroup>()
            };
        }
    }
}