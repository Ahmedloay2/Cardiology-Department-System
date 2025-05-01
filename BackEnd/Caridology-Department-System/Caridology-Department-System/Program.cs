using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Caridology_Department_System.Models;
using Caridology_Department_System;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// --- DbContext ---
builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

// --- Session ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Cardiology.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:44312",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5501",
                "https://cardio-w-tever.vercel.app",
                "https://cardiology-department-system.runasp.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --- File Upload Size Limit ---
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();
    c.UseAllOfToExtendReferenceSchemas();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cardiology Department API",
        Version = "v1",
        Description = "API for Cardiology Department System"
    });

    // Swagger security left in place for future use
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Cookies",
        Description = "Cookie-based authentication"
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cardiology Department API V1");
    c.RoutePrefix = string.Empty;
    c.ConfigObject.AdditionalItems["requestSnippetsEnabled"] = true;
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();  // Middleware kept
app.UseAuthorization();   // Middleware kept
app.UseSession();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
