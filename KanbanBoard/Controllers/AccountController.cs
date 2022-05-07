namespace KanbanBoard.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> logger;
    private JwtConfig jwtConfig;

    public AccountController(ILogger<AccountController> logger, IOptions<JwtConfig> jwtConfig)
    {
        this.logger = logger;
        this.jwtConfig = jwtConfig.Value;
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        User? user = context.Users.FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);
        if (user == null)
            return Problem(title: "Invalid login or password", statusCode: 400);

        string jwt = CreateJwtToken(user);

        RefreshToken? oldRefreshToken = context.RefreshTokens.Where(t => t.DeviceUid == requestData.DeviceUid).FirstOrDefault();
        if (oldRefreshToken is { }) context.RefreshTokens.Remove(oldRefreshToken);

        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Expires = (DateTime.Now + TimeSpan.FromDays(30)).SetKindUtc(),
            DeviceUid = requestData.DeviceUid
        };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var response = new
        {
            AccessToken = jwt,
            User = new { user.Id, user.Name, user.Email }
        };

        Response.Cookies.Append("RefreshToken", refreshToken.Id.ToString(),
            new CookieOptions
            {
                HttpOnly = true, 
                // Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(30))
            });

        return Ok(response);
    }
    
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (await context.Users.AnyAsync(x => x.Email == model.Email))
            return Problem(title: "The user with this email already exists", statusCode: 400);

        User user = new() { Email = model.Email!, Password = model.Password!, Name = model.Name! };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        string jwt = CreateJwtToken(user);

        RefreshToken? oldRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.DeviceUid == requestData.DeviceUid);
        if (oldRefreshToken is { }) context.RefreshTokens.Remove(oldRefreshToken);
        
        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Expires = (DateTime.Now + TimeSpan.FromDays(30)).SetKindUtc(),
            DeviceUid = requestData.DeviceUid
        };
        
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var response = new { AccessToken = jwt };

        Response.Cookies.Append("RefreshToken", refreshToken.Id.ToString(),
            new CookieOptions
            {
                HttpOnly = true, 
                // Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(30))
            });

        return Ok(response);
    }

    [HttpPost("/refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out string? sToken)
            || !Guid.TryParse(sToken, out Guid token))
            return Problem(title: "There is no RefreshToken cookie", statusCode: 400);
        
        RefreshToken? refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Id == token);
        if (refreshToken == null || refreshToken.Expires <= DateTime.Now)
            return Problem(title: "Invalid or expired token", statusCode: 400);
        if (refreshToken.DeviceUid != requestData.DeviceUid)
            return Problem(title: "Invalid token", detail: "The token was created on another client", statusCode: 400);

        User user = await context.Users.FirstAsync(u => u.Id == refreshToken.UserId);
        string jwt = CreateJwtToken(user);

        RefreshToken newRefreshToken = new()
        {
            UserId = user.Id,
            Expires = (DateTime.Now + TimeSpan.FromDays(30)).SetKindUtc(),
            DeviceUid = requestData.DeviceUid
        };
        context.RefreshTokens.Remove(refreshToken);
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        var response = new { AccessToken = jwt };

        Response.Cookies.Append("RefreshToken", newRefreshToken.Id.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                // Secure = true, //TODO: 
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(30))
            });

        return Ok(response);
    }
    
    [HttpPost("/logout")]
    public async Task<IActionResult> Login(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out string? sToken)
            || !Guid.TryParse(sToken, out Guid token))
            return Problem(title: "There is no RefreshToken cookie", statusCode: 400);
        
        RefreshToken? refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Id == token);
        if (refreshToken == null)
            return Problem(title: "Invalid or expired token", statusCode: 400);
        if (refreshToken.DeviceUid != requestData.DeviceUid)
            return Problem(title: "Invalid token", detail: "The token was created on another client", statusCode: 400);
        
        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync();

        Response.Cookies.Delete("RefreshToken");

        return Ok();
    }

    [HttpGet("/i"), Authorize]
    public IActionResult Info()
    {
        return Ok(new
        {
            Login = User.FindFirst(JwtRegisteredClaimNames.Email)!.Value,
            Role = String.Join(", ", User.FindAll(ClaimTypes.Role))
        });
    }

    string CreateJwtToken(User user)
    {
        var now = DateTime.UtcNow;

        var jwt = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            notBefore: now,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                //new Claim("roles", user.Role)
            },
            expires: now.Add(TimeSpan.FromSeconds(120)),
            signingCredentials: new SigningCredentials(jwtConfig.SecretKey,
                SecurityAlgorithms.HmacSha256));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }
}