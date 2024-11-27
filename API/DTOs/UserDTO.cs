namespace API.DTOs
{
    public class UserDTO
    {
        public int IdUser { get; set; }

        public string NameUser { get; set; } = null!;

        public string EmailUser { get; set; } = null!;

        public string? OldPassword { get; set; }

        public string NewPassword { get; set; } = null!;

        public int IdRole { get; set; }
    }
}
