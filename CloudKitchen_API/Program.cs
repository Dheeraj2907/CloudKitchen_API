using AutoMapper;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services;
using CloudKitchen_API.Services.Hubs;
using CloudKitchen_API.Services.Implementation;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperProfile());
}, NullLoggerFactory.Instance);
IMapper autoMapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(autoMapper);

builder.Services.AddDbContext<KitchenDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Allow the SignalR client to pass the JWT via query string
        // (browsers cannot set Authorization headers on WebSocket connections)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/order"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddTransient<IAuthService, AuthService>()
    .AddTransient<IMenuService, MenuService>()
    .AddScoped<IOrderService, OrderService>()
    .AddTransient<IAuthService, AuthService>()
    .AddScoped<IOrderService, OrderService>()
    .AddScoped<IMenuService, MenuService>()
    .AddScoped<ICartService, CartService>()
    .AddScoped<IUserService, UserService>()
    .AddScoped<IAddressService, AddressService>()
    .AddScoped<ICouponService, CouponService>()
    .AddScoped<IReviewService, ReviewService>()
    .AddScoped<IRiderService, RiderService>()
    .AddScoped<INotificationService, NotificationService>()
    .AddScoped<IAdminMenuService, AdminMenuService>()
    .AddScoped<IOrderNotifier, OrderNotifier>()
    .AddScoped<INotificationQueryService, NotificationQueryService>()
    .AddScoped<IImageStorageService, LocalImageStorageService>(); 

builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/order");

app.Run();