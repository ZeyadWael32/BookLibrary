using BookLibrary.API;
using BookLibrary.API.Data;
using BookLibrary.API.Exceptions;
using BookLibrary.API.Repository;
using BookLibrary.API.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingConfig>();
});

// Controllers
builder.Services.AddControllers();

// Keycloak JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];

        // Local development only: Keycloak uses HTTP.
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Keycloak:Authority"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Keycloak:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Keycloak access token."
    });

    // Do not add a global security requirement:
    // GET endpoints are public.
});

// Exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Exception handling middleware
app.UseExceptionHandler();

// Swagger and Scalar (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.MapScalarApiReference(options => options
        .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", auth =>
        {
            auth.Token = "";
        }));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();