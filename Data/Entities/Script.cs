using System.ComponentModel.DataAnnotations;

using VideoScripter.Data.Common;

namespace VideoScripter.Data.Entities;

public class Script : BaseEntity
{
    [Required]
    public Guid ProjectId { get; set; }
    [Required]
    public string Title { get; set; }
    public string Content { get; set; }
    public int Version { get; set; }
    // Navigation property
    public virtual Project Project { get; set; }
}