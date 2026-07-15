using System.ComponentModel.DataAnnotations;

namespace MangaWorkflow.Services.HuyNQ.DTOs.Chapter;

public class ChapterCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "ChapterMetaHuynqId must be a positive number.")]
    public int? ChapterMetaHuynqId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 255 characters.")]
    public string Title { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ChapterNumber must be a positive number.")]
    public int? ChapterNumber { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "PageCount cannot be negative.")]
    public int? PageCount { get; set; }

    public bool? Approved { get; set; }

    public DateTime? ReleaseDate { get; set; }

    [Range(1, 5, ErrorMessage = "PriorityLevel must be between 1 and 5.")]
    public int? PriorityLevel { get; set; }

    public string EditorComment { get; set; }

    [StringLength(255, ErrorMessage = "LayoutVersion cannot exceed 255 characters.")]
    public string LayoutVersion { get; set; }

    public string ReviewNotes { get; set; }
}
