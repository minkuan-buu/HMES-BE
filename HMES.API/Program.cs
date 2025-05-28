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
using HMES.Business.Services.PlantServices;
using HMES.Business.Services.TargetValueServices;
using HMES.Business.Services.TicketServices;
using HMES.Business.Services.UserServices;
using HMES.Business.Services.UserTokenServices;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.CategoryRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.OTPRepositories;
using HMES.Data.Repositories.ProductRepositories;
using HMES.Data.Repositories.OrderDetailRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.TicketRepositories;
using HMES.Data.Repositories.TicketResponseRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using HMES.Data.Repositories.UserAddressRepositories;
using HMES.Data.Repositories.UserRepositories;
using HMES.Data.Repositories.UserTokenRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using HMES.Business.Services.UserAddressServices;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Business.Services.DeviceItemServices;
using HMES.Business.Services.NotificationServices;
using HMES.Data.Repositories.NotificationRepositories;
using HMES.Data.Repositories.PlantRepositories;
using HMES.Data.Repositories.TargetValueRepositories;
using HMES.Business.Utilities.Email;
using HMES.Data.Repositories.NutritionRDRepositories;
using HMES.Data.Repositories.NutritionReportRepositories;
using HMES.Business.Services.GHNService;
using HMES.Business.Services.PhaseServices;
using HMES.Data.Repositories.PhaseRepositories;
using HMES.Data.Repositories.PlantOfPhaseRepositories;
using HMES.Data.Repositories.TargetOfPhaseRepositories;
using System.Text.Json.Serialization;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rawConnectionString = builder.Configuration.GetSection("Database:ConnectionString").Value;

if (rawConnectionString == null)
{
    throw new Exception("Connection string is not found");
}

var connectionString = rawConnectionString
    .Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "")
    .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "")
    .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "")
    .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "")
    .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? "")
    .Replace("${HERE_MAP_API_KEY}", Environment.GetEnvironmentVariable("HERE_MAP_API_KEY") ?? "");

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

    c.MapType<TicketStatusEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(TicketStatusEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });

    c.MapType<TicketTypeEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(TicketTypeEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });
    c.MapType<PlantStatusEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(PlantStatusEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });
    c.MapType<ValueTypeEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(ValueTypeEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });
    c.MapType<RoleEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(RoleEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });
    c.MapType<AccountStatusEnums>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(AccountStatusEnums)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
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

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//======================================= AUTHENTICATION ==========================================
builder.Services.AddAuthentication("HMESAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthorizeMiddleware>("HMESAuthentication", null).AddScheme<AuthenticationSchemeOptions, IoTAuthorizeMiddleware>("HMESIoTAuthentication", null);

//=========================================== FIREBASE ============================================
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"hmes.json");
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
builder.Services.AddScoped<ITicketRepositories, TicketRepositories>();
builder.Services.AddScoped<ITicketResponseRepositories, TicketResponseRepositories>();
builder.Services.AddScoped<IDeviceItemsRepositories, DeviceItemsRepositories>();
builder.Services.AddScoped<IPlantRepositories, PlantRepositories>();
builder.Services.AddScoped<ITargetValueRepositories, TargetValueRepositories>();
builder.Services.AddScoped<ITargetOfPhaseRepository, TargetOfPhaseRepositories>();
builder.Services.AddScoped<INotificationRepositories, NotificationRepositories>();
builder.Services.AddScoped<INutritionRDRepositories, NutritionRDRepositories>();
builder.Services.AddScoped<INutritionReportRepositories, NutritionReportRepositories>();
builder.Services.AddScoped<IPhaseRepositories, PhaseRepositories>();
builder.Services.AddScoped<IPlantOfPhaseRepositories, PlantOfPhaseRepositories>();

//=========================================== SERVICE =============================================
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IUserTokenServices, UserTokenServices>();
builder.Services.AddScoped<IDeviceServices, DeviceServices>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<ICartServices, CartServices>();
builder.Services.AddScoped<IOTPServices, OTPServices>();
builder.Services.AddScoped<IOrderServices, OrderServices>();
builder.Services.AddScoped<IUserAddressServices, UserAddressServices>();
builder.Services.AddScoped<IEmail, Email>();
builder.Services.AddScoped<ITicketServices, TicketServices>();
builder.Services.AddScoped<IDeviceItemServices, DeviceItemServices>();
builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddHostedService<DeviceStatusChecker>();
builder.Services.AddHostedService<DoubleCheckExpiredPayment>();
builder.Services.AddScoped<IPlantServices, PlantServices>();
builder.Services.AddScoped<ITargetValueServices, TargetValueServices>();
builder.Services.AddScoped<INotificationServices, NotificationServices>();
builder.Services.AddScoped<IGHNService, GHNService>();
builder.Services.AddScoped<IPhaseServices, PhaseServices>();

//=========================================== CORS ================================================
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigin", policy =>
    {
        policy
            .WithOrigins(allowedOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("New-Access-Token");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowAllOrigin");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
