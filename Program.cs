using System.Text;
using foodies_api.Models.Mappings;
using foodies_api.Constants;
using foodies_api.Data;
using foodies_api.Endpoints;
using foodies_api.Interfaces.Repositories;
using foodies_api.Interfaces.Services;
using foodies_api.Repositories;
using foodies_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var AllowLocalDevelopment = "AllowLocalDevelopment";
// temporarily hiding password in user-secrets
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = configuration["DbPassword"];
conn += dbPassword;

var CorsPolicies = new Action<CorsPolicyBuilder>(policy =>
{
    policy.WithOrigins("http://localhost:3000",
                        "http://localhost:3001")
          .AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod();
});


builder.Services.AddCors(options => options.AddPolicy(name: AllowLocalDevelopment, CorsPolicies));

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddAutoMapper(typeof(PostUserProfile), typeof(GetBusinessProfile), typeof(PostUserLikeBusinessProfile));

builder.Services.AddScoped<IUsersLikeBusinessesRepository, UsersLikeBusinessesRepository>();
builder.Services.AddScoped<IUsersLikeBusinessesService, UsersLikeBusinessesService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IFoodiesYelpService, FoodiesYelpService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Configure Http Client to Foodies Yelp Service
builder.Services.AddHttpClient("FoodiesYelpService", client => client.BaseAddress = new Uri(configuration.GetValue<string>("BaseAddress")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });
    
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Identity.AdminUserPolicyName, p =>
        p.RequireClaim(Identity.AdminUserClaimName, "true"));
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{   // Opens at http://localhost:(whatever port numer)/index.html
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
    options.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
});

// Order of app.func matters. Authentication before Authorization. Both Auths before Controllers/Endpointss
app.UseAuthentication();
app.UseAuthorization();

// app.UseHttpsRedirection();

app.ConfigurationUserEndpoints();
app.ConfigurationAuthEndpoints();
app.ConfigurationUserLikeBusinessEndpoints();

app.UseCors(AllowLocalDevelopment);

app.Run();
