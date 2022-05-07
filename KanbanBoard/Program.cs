global using System.Text;
global using System.Data;
global using System.Text.Json;
global using System.Diagnostics;
global using System.Security.Claims;
global using System.IdentityModel.Tokens.Jwt;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.SpaServices;
global using Microsoft.AspNetCore.Mvc.Authorization;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.EntityFrameworkCore;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Text.Json.Serialization;
global using VueCliMiddleware;
global using Npgsql;
global using FluentValidation;
global using FluentValidation.AspNetCore;
global using EFCore.BulkExtensions;
global using Mapster;
global using KanbanBoard;
global using KanbanBoard.Utils;
global using KanbanBoard.Configuration;
global using KanbanBoard.Models;
global using KanbanBoard.Database;
global using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

string connection = builder.Configuration.GetConnectionString("PgsqlConnection");
services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connection));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");
ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => SnakeCaseNamingPolicy.ToSnakeCase(member.Name);

services.AddControllersWithViews(options =>
    {
        //var policy = new AuthorizationPolicyBuilder().Build();
        //options.Filters.Add(new AuthorizeFilter(policy));
        options.InputFormatters.Insert(0, new RawRequestBodyFormatter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(SnakeCaseNamingPolicy.Instance));
        options.AllowInputFormatterExceptionMessages = false;
        //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    })
    .AddFluentValidation(fv =>
    {
        fv.DisableDataAnnotationsValidation = true;
        fv.RegisterValidatorsFromAssemblyContaining<Program>(lifetime: ServiceLifetime.Singleton);
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        //options.InvalidModelStateResponseFactory = context => { };
    });

services.Configure<JwtConfig>(configuration.GetSection(JwtConfig.SectionName));

var jwtConfig = configuration.GetSection(JwtConfig.SectionName).Get<JwtConfig>();

var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
    ValidIssuer = jwtConfig.Issuer,
    ValidateIssuer = true,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false,
    ClockSkew = TimeSpan.Zero,
};

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // TODO: ~~~

services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParams;
    });

services.AddScoped<RequestData>();

services.AddSpaStaticFiles(opt => opt.RootPath = "ClientApp/dist");

services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsBuilder =>
        {
            corsBuilder
                .AllowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(hostName => true);
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    RequestData requestData = context.RequestServices.GetRequiredService<RequestData>();
    if (!context.Request.Cookies.TryGetValue("DeviceUid", out string? deviceSUid)
        || !Guid.TryParse(deviceSUid, out Guid deviceUid))
    {
        requestData.DeviceUid = Guid.NewGuid();
        context.Response.Cookies.Append("DeviceUid", requestData.DeviceUid.ToString(),
            new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.FromUnixTimeSeconds(int.MaxValue) });
    }
    else
        requestData.DeviceUid = deviceUid;

    if (context.User.Identity?.IsAuthenticated is true)
    {
        requestData.UserId = int.Parse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        requestData.UserEmail = context.User.FindFirst(JwtRegisteredClaimNames.Email)!.Value;
    }

    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    endpoints.MapToVueCliProxy(
        "{*path}",
        new SpaOptions { SourcePath = "ClientApp" },
        npmScript: (System.Diagnostics.Debugger.IsAttached) ? "serve" : null,
        regex: "Compiled successfully",
        forceKill: true
        );
});

app.Run();

/*public class VerifyAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        
    }
}*/