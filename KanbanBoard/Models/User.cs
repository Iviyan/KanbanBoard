namespace KanbanBoard.Models;

[Table("users")]
public class User
{
    [Column("id")] public int Id { get; set; }
    [Column("email")] public string Email { get; set; } = null!;
    [Column("password")] public string Password { get; set; } = null!;
    [Column("name")] public string Name { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public virtual ICollection<Task> Tasks { get; set; } = new HashSet<Task>();
}

public record LoginModel(string? Email, string? Password);

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(user => user.Email).NotNull();
        RuleFor(user => user.Password).NotNull();
    }
}
public record RegisterModel(string? Email, string? Password, string? Name);

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(user => user.Email).NotNull().EmailAddress().Length(0, 255);
        RuleFor(user => user.Password).NotNull().Length(1, 30);
        RuleFor(user => user.Name).NotNull().Length(1, 30);
    }
}

public class UserPatchDto : PatchDtoBase
{
    private string name = null!;

    public string Name { get => name; set {name  = value; SetHasProperty(); } }
}

public class UserPatchValidator : AbstractValidator<UserPatchDto>
{
    public UserPatchValidator()
    {
        RuleFor(user => user.Name).NotNull().Length(1, 30);
    }
}

public record ChangePasswordRequest(string? NewPassword, string? OldPassword);

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(user => user.NewPassword).NotNull().Length(1, 30);
        RuleFor(user => user.OldPassword).NotNull();
    }
}