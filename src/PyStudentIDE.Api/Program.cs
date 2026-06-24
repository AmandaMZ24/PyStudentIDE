using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PyStudentIDE.Application.Facade;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Application.Services;
using PyStudentIDE.Infrastructure.Crypto;
using PyStudentIDE.Infrastructure.Data;
using PyStudentIDE.Infrastructure.Data.Repositories;
using PyStudentIDE.Infrastructure.Decorators;
using PyStudentIDE.Infrastructure.Git;
using PyStudentIDE.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICryptoStrategy, Sha256Strategy>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new EventLogger(config["Logging:LogPath"] ?? "logs/events.log");
});
builder.Services.AddScoped<IGitAdapter, GitAdapter>();

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "DefaultSecretKey_2026_PyStudentIDE!";
var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Estudiante", policy => policy.RequireRole("ESTUDIANTE"));
    options.AddPolicy("Docente", policy => policy.RequireRole("DOCENTE"));
    options.AddPolicy("EstudianteOrDocente", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("ESTUDIANTE") || context.User.IsInRole("DOCENTE")));
});

builder.Services.AddScoped<IAuthService>(sp =>
{
    var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
    var inner = new AuthService(unitOfWork, jwtSecretKey);
    var logger = sp.GetRequiredService<EventLogger>();
    return new AuthServiceDecorator(inner, logger);
});

builder.Services.AddScoped<IAssignmentService>(sp =>
{
    var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
    var inner = new AssignmentService(unitOfWork);
    var logger = sp.GetRequiredService<EventLogger>();
    return new AssignmentServiceDecorator(inner, logger);
});

builder.Services.AddScoped<ITestEngine, TestEngineService>();
builder.Services.AddScoped<ILlaveCursoService, LlaveCursoService>();
builder.Services.AddScoped<IGitService, GitService>();
builder.Services.AddScoped<IRetroalimentacionService, RetroalimentacionService>();
builder.Services.AddScoped<PyStudentFacade>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    service = "PyStudentIDE.Api",
    timestamp = DateTimeOffset.UtcNow
}));
app.MapControllers();

app.Run();
