using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class TestCaseRequest
{
    [Required]
    [MinLength(20, ErrorMessage = "Description must be at least 20 characters.")]
    [MaxLength(5000, ErrorMessage = "Description must not exceed 5000 characters.")]
    public string FeatureDescription { get; set; } = string.Empty;

    public string? ApplicationName { get; set; }
}