using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IceSMS.API.Data;
using IceSMS.API.Interfaces;
using StackExchange.Redis;
using IceSMS.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// PostgreSQL yapılandırması
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis yapılandırması
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddScoped<ISessionService, RedisSessionService>();

// Email servisi
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// SMS servisi
//builder.Services.AddScoped<ISmsService, DummySmsService>();

// Google Auth servisi
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// Profil servisi
builder.Services.AddScoped<IProfileService, ProfileService>();

// Auth servisi
builder.Services.AddScoped<IAuthService, AuthService>();

// User servisi
builder.Services.AddScoped<IUserService, UserService>();

// Role servisi
builder.Services.AddScoped<IRoleService, RoleService>();

// Service registrations
builder.Services.AddScoped<IVendorsService, VendorsService>();
builder.Services.AddScoped<IVendorsConnectionParametersService, VendorsConnectionParametersService>();
builder.Services.AddScoped<IVendorsActionParametersService, VendorsActionParametersService>();
builder.Services.AddScoped<ISmsTitleService, SmsTitleService>();

// JWT yapılandırması
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured")))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IceSMS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// CORS ayarları
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // React uygulamasının adresi
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/v1/docs/{documentname}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {        c.SwaggerEndpoint("/api/v1/docs/v1/swagger.json", "IceSMS API v1");
        c.RoutePrefix = "api/v1/docs";
    });


app.UseHttpsRedirection();

// CORS middleware
app.UseCors("AllowReactApp");

// Tenant middleware
app.Use(async (context, next) =>
{
    var domain = context.Request.Host.Host.ToLower();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Domain.ToLower() == domain);
    if (tenant != null)
    {
        context.Items["TenantId"] = tenant.Id;
    }
    
    await next();
});

// Statik dosyalar için wwwroot klasörünü kullan
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// React router için tüm bilinmeyen istekleri index.html'e yönlendir
app.MapFallbackToFile("index.html");

app.Run();

