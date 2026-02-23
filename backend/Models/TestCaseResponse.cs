namespace Backend.Models;

public class TestCaseResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<TestCaseGroup> Groups { get; set; } = new();
    public int TotalTestCases { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}