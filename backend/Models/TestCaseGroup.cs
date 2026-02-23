namespace Backend.Models;

public class TestCaseGroup
{
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TestCase> TestCases { get; set; } = new();
}