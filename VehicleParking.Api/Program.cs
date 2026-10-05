using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehicleParking.Api.Data;
using VehicleParking.Api.Models;
using VehicleParking.Api.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection register 
builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. JWT Key Authentication configure
var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

builder.Services.AddScoped<IParkingService, ParkingService>();
// 3. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 4. DATABASE SEEDING: Default Admin Account Automatically Create Karne ke Liye
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();

    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        var passwordHasher = new PasswordHasher<User>();

        var defaultAdmin = new User
        {
            FullName = "System Administrator",
            Email = "admin@parking.com",
            Password = string.Empty,
            Role = "Admin",
            CreatedAt = DateTime.Now
        };

        // Hash password correctly
        defaultAdmin.Password = passwordHasher.HashPassword(defaultAdmin, "admin123");

        context.Users.Add(defaultAdmin);
        context.SaveChanges();
    }
}

// 5. Middleware Pipeline
app.UseHttpsRedirection();  // http://..... => https://...

app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers();

app.MapGet("/", () => "Vehicle Parking API is Running!");

app.Run();