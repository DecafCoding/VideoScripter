using System.ComponentModel.DataAnnotations;

using VideoScripter.Data.Common;

namespace VideoScripter.Data.Entities;

public class TranscriptTopic : BaseEntity
{
    [Required]
    public Guid VideoId { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Content { get; set; }
    public string TopicSummary { get; set; }
    public bool IsSelected { get; set; }
    // Navigation property
    public virtual Video Video { get; set; }
}
