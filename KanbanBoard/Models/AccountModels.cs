namespace KanbanBoard.Models;

public record LoginModel(string? Email, string? Password);

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(task => task.Email).NotNull();
        RuleFor(task => task.Password).NotNull();
    }
}
public record RegisterModel(string? Email, string? Password, string? Name);

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(task => task.Email).NotNull().EmailAddress().Length(0, 255);
        RuleFor(task => task.Password).NotNull().Length(1, 30);
        RuleFor(task => task.Name).NotNull().Length(1, 30);
    }
}