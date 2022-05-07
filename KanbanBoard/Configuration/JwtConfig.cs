namespace KanbanBoard.Configuration;

public class JwtConfig
{
    public const string SectionName = "JwtConfig";
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;

    public SymmetricSecurityKey SecretKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
}