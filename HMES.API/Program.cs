using Google.Cloud.Storage.V1;
using HMES.API.Middleware;
using HMES.Business.MapperProfiles;
using HMES.Business.Services.CartServices;
using HMES.Business.Services.CategoryServices;
using HMES.Business.Services.CloudServices;
using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.OTPServices;
using HMES.Business.Services.ProductServices;
using HMES.Business.Services.OrderServices;
using HMES.Business.Services.UserServices;
using HMES.Business.Services.UserTokenServices;
using HMES.Business.Ultilities.Email;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.CategoryRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.OTPRepositories;
using HMES.Data.Repositories.ProductRepositories;
using HMES.Data.Repositories.OrderDetailRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.ProductRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using HMES.Data.Repositories.UserAddressRepositories;
using HMES.Data.Repositories.UserRepositories;
using HMES.Data.Repositories.UserTokenRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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

//========================================== SWAGGER ==============================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "HMES.API",
        Description = "Hydroponic Monitoring Equipment System"
    });
    
    c.MapType<ProductStatusEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(ProductStatusEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });

    // 🟢 Cấu hình Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. " +
                      "\n\nEnter your token in the text input below. " +
                      "\n\nExample: '12345abcde'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    // 🟢 Cấu hình Cookie Authentication
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "Cookie",
        In = ParameterLocation.Header,
        Description = "Nhập Cookie vào đây (VD: sessionId=xyz123)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            new string[] {}
        }
    });
});


//======================================= AUTHENTICATION ==========================================
builder.Services.AddAuthentication("HMESAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthorizeMiddleware>("HMESAuthentication", null);

//=========================================== FIREBASE ============================================
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"meowwoofsocial.json");
builder.Services.AddSingleton<ICloudServices>(s => new CloudServices(StorageClient.Create()));

//========================================== MIDDLEWARE ===========================================
builder.Services.AddSingleton<GlobalExceptionMiddleware>();

//========================================== MAPPER ===============================================
builder.Services.AddAutoMapper(typeof(MapperProfileConfiguration).Assembly);

//========================================== REPOSITORY ===========================================
builder.Services.AddScoped<IUserRepositories, UserRepositories>();
builder.Services.AddScoped<IUserTokenRepositories, UserTokenRepositories>();
builder.Services.AddScoped<IDeviceRepositories, DeviceRepositories>();
builder.Services.AddScoped<ICategoryRepositories, CategoryRepositories>();
builder.Services.AddScoped<IProductRepositories, ProductRepositories>();
builder.Services.AddScoped<ICartRepositories, CartRepositories>();
builder.Services.AddScoped<ICartItemsRepositories, CartItemsRepositories>();
builder.Services.AddScoped<IOTPRepositories, OTPRepositories>();
builder.Services.AddScoped<IOrderRepositories, OrderRepositories>();
builder.Services.AddScoped<IOrderDetailRepositories, OrderDetailRepositories>();
builder.Services.AddScoped<ITransactionRepositories, TransactionRepositories>();
builder.Services.AddScoped<IUserAddressRepositories, UserAddressRepositories>();

//=========================================== SERVICE =============================================
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IUserTokenServices, UserTokenServices>();
builder.Services.AddScoped<IDeviceServices, DeviceServices>();
builder.Services.AddScoped<ICategoryServices,CategoryServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
