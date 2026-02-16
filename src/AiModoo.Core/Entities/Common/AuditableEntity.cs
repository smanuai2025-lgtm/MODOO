namespace AiModoo.Core.Entities.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
