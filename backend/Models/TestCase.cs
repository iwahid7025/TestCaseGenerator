namespace Backend.Models;

public class TestCase
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Preconditions { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public string ExpectedResult { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    public string TestType { get; set; } = string.Empty; // Functional, Negative, Edge Case etc.
}