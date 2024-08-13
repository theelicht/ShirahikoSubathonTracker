using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiraSubathonTracker.DAL;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddDbContext<TrackerDatabaseContext>(contextBuilder =>
    {
        contextBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
        contextBuilder.EnableSensitiveDataLogging();
    }
);

builder.Services
    .AddAuthentication(opts =>
    {
        opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = configuration["JWT:ValidIssuer"],
            ValidAudience = configuration["JWT:ValidAudience"],
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Decode the JWT token to get the username or user ID
                var jwtToken = new JwtSecurityToken(token);
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (username == null) throw new SecurityTokenException("Invalid token");
                if (role is not "ApiAccess") throw new SecurityTokenException("Invalid token");

                // Retrieve the user's secret key from the database
                using var scope = builder.Services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TrackerDatabaseContext>();
                var user = context.Users.SingleOrDefault(u => u.Username == username);
                var existingToken = context.JwtTokens.SingleOrDefault(u => u.Username == username);

                if (user == null) throw new SecurityTokenException("Invalid token");
                if (existingToken is { IsBlocked: true }) throw new SecurityTokenException("Invalid token");

                // Return the user's secret key as the signing key
                return [new SymmetricSecurityKey(Encoding.ASCII.GetBytes(user.SecretKey))];
            }
        };
    });

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

AutoApplyMigrations(app);

app.Run();
return;

void AutoApplyMigrations(WebApplication webApplication)
{
    var scope = webApplication.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<TrackerDatabaseContext>();
    dbContext!.Database.Migrate();
}