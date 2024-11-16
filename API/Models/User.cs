namespace API.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string NameUser { get; set; } = null!;

    public string EmailUser { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRole { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
