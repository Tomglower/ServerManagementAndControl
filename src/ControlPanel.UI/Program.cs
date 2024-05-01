using ControlPanel.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ControlPanel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(option =>
{
    option.AddPolicy("MyPolicy", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddDbContext<AppDbContext>();
/*builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConString")));*/
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysecret.....")),
        ValidateAudience = false,
        ValidateIssuer = false
    };
});
builder.Services.AddTransient<AuthManager>();
builder.Services.AddTransient<ServerManager>();
builder.Services.AddTransient<PrometheusExporter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
var app = builder.Build();
var logger = app.Services.GetService<ILogger<AppDbContext>>();

try
{
    var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Миграции успешно применены");
}
catch (Exception ex)
{
   logger.LogError("Ошибка применения миграций: " + ex.Message);
    return;
}

app.UseCors("MyPolicy");

app.UseAuthentication();


app.UseRouting();

app.UseAuthorization();

app.MapControllers();


app.Run();

