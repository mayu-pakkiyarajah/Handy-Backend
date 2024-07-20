using CloudinaryDotNet;
using HandyHero.Common;
using HandyHero.Data;
using HandyHero.Services.Infrastructure;
using HandyHero.Services.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static HandyHero.Services.Repository.FieldWorkerRepository;
//using WebSocketManager;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
//using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using HandyHero.Services;
using HandyHero.Hub;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Retrieve connection string
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");

// Add DbContext to the service container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtIsUser = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIsUser,
            ValidAudience = jwtIsUser,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddSingleton(new Cloudinary(new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
)));

// Add Email Configuration
builder.Services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IAdmin, AdminRepository>();
builder.Services.AddScoped<ICustomer, CustomerRepository>();
builder.Services.AddScoped<IFieldWorker, FieldWorkerRepository>();
builder.Services.AddScoped<INotification, NotificationRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddControllers().AddNewtonsoftJson();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Your Angular frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Allow credentials
        });
});

builder.Services.AddScoped<ChatService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Added authentication middleware
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();