using Asp.Versioning;
using System.Text;
using identity.api.Configuration;
using identity.api.Infrastructure;
using identity.api.Middlewares;
using identity.api.Repositories;
using identity.api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

    options.UseNpgsql(connectionString);
});

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ITokenGenerator, TokenGenerator>();
services.AddSingleton(TimeProvider.System);

services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer must be configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience must be configured.")
    .Validate(
        options => Encoding.UTF8.GetByteCount(options.SigningKey) >= JwtOptions.MinimumSigningKeyLength,
        $"Jwt:SigningKey must contain at least {JwtOptions.MinimumSigningKeyLength} bytes.")
    .Validate(options => options.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be greater than zero.")
    .ValidateOnStart();

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
    .AddJwtBearer();

services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
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
