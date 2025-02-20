using HMES.API.Middleware;
using HMES.Business.MapperProfiles;
using HMES.Business.Services.UserServices;
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rawConnectionString = builder.Configuration.GetSection("Database:ConnectionString").Value;

if(rawConnectionString == null)
{
    throw new Exception("Connection string is not found");
}

var connectionString = rawConnectionString
    .Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "")
    .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "")
    .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "")
    .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "");

builder.Services.AddDbContext<HmesContext>(options =>
    options.UseSqlServer(connectionString));

//======================================= AUTHENTICATION ==========================================
builder.Services.AddAuthentication("HMESAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthorizeMiddleware>("HMESAuthentication", null);

//========================================== MIDDLEWARE ===========================================
builder.Services.AddSingleton<GlobalExceptionMiddleware>();

//========================================== MAPPER ===============================================
builder.Services.AddAutoMapper(typeof(MapperProfileConfiguration).Assembly);

//========================================== REPOSITORY ===========================================
builder.Services.AddScoped<IUserRepositories, UserRepositories>();

//=========================================== SERVICE =============================================
builder.Services.AddScoped<IUserServices, UserServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
