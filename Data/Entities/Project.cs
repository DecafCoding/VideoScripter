using System.ComponentModel.DataAnnotations;

using VideoScripter.Data.Common;

namespace VideoScripter.Data.Entities;

public class Project : BaseEntity
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Topic { get; set; }
    [Required]
    public string UserId { get; set; }
    // Navigation properties
    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
    public virtual ICollection<Script> Scripts { get; set; } = new List<Script>();
}
