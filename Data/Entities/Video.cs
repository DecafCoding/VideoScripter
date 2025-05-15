using System.ComponentModel.DataAnnotations;

using VideoScripter.Data.Common;

namespace VideoScripter.Data.Entities;

public class Video : BaseEntity
{
    public Guid? ProjectId { get; set; }
    [Required]
    public string YTId { get; set; }
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    [Required]
    public Guid ChannelId { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int Duration { get; set; }
    public DateTime PublishedAt { get; set; }
    public string RawTranscript { get; set; }
    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual Channel Channel { get; set; }
    public virtual ICollection<TranscriptTopic> TranscriptTopics { get; set; } = new List<TranscriptTopic>();
}
