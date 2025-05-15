using System.ComponentModel.DataAnnotations;

using VideoScripter.Data.Common;

namespace VideoScripter.Data.Entities;

public class Category : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    // Relationships
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
