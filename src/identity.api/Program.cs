using Asp.Versioning;
using System.Text;
using identity.api.Configuration;
using identity.api.Infrastructure;
using identity.api.Middlewares;
using identity.api.Repositories;
using identity.api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();

services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

services.AddTransient<RequestResponseMiddleware>();
services.AddTransient<ExceptionMiddleware>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ITokenGenerator, TokenGenerator>();
services.AddSingleton(TimeProvider.System);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) || string.IsNullOrWhiteSpace(jwtOptions.Audience))
    throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < JwtOptions.MinimumSigningKeyLength)
    throw new InvalidOperationException($"Jwt:SigningKey must contain at least {JwtOptions.MinimumSigningKeyLength} bytes.");

if (jwtOptions.ExpirationMinutes <= 0)
    throw new InvalidOperationException("Jwt:ExpirationMinutes must be greater than zero.");

services.Configure<JwtOptions>(jwtSection);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestResponseMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DataInitializer.InitializeAsync(app);

app.Run();

public partial class Program
{
}
