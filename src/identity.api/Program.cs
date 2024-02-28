using Asp.Versioning;
using FluentValidation;
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

services.AddValidatorsFromAssemblyContaining<Program>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

services.AddScoped<IUserRepository, UserRepository>();

services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        b => b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
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
            ValidIssuer = "identity",
            ValidAudience = "client",
            IssuerSigningKey = new SymmetricSecurityKey("token_65fb67e4-5f3b-4711-843d-07d4a5e61c72"u8.ToArray())
        };
    });

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.UseCors("AllowOrigin");
app.UseMiddleware<RequestResponseMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

await DataInitializer.Initialize(app);

app.Run();

public partial class Program
{
}