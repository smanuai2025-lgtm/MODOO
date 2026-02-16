using Microsoft.AspNetCore.Identity;

namespace AiModoo.Core.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsSystemRole { get; set; }
}
