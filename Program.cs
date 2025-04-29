using Contacts.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Contact Management API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        }
    });
});

// Configure EF Core with SQLite
string dbPath = string.Empty;
if(builder.Environment.IsProduction())
{
     // Create a directory to store the SQLite database that persists across deployments
    string dbDirectory = Path.Join(Path.GetTempPath(), "ContactsAppData");
    Directory.CreateDirectory(dbDirectory);

    // Use a specific filename that's easy to identify
    dbPath = Path.Join(dbDirectory, "contacts_db_new.db");
    Console.WriteLine($"Using SQLite database at: {dbPath}");
}
else
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    dbPath = Path.Join(path, "contacts.db");
}

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlite($"Data Source={dbPath};Mode=ReadWriteCreate;Cache=Shared");
    
    // Add this to help with concurrency in Azure
    if (!builder.Environment.IsDevelopment()) {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(true);
    }
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "contactapi-e3e0b3epf6c4aqcm.canadacentral-01.azurewebsites.net",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "contactapi-e3e0b3epf6c4aqcm.canadacentral-01.azurewebsites.net",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultDevKeyForTesting12345678901234567890"))
        };
    });

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Management API v1");
});

app.MapGet("/", () => "Contact Management API is running! Try /api/health or /api/contacts");

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create database if it doesn't exist
// After app.MapControllers() but before app.Run()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        
        // Try to delete any existing corrupted database
        try {
            if (File.Exists(dbPath)) {
                Console.WriteLine($"Attempting to connect to existing database...");
                // Test if the database is valid by running a simple query
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (!canConnect) {
                    Console.WriteLine("Database file exists but is corrupted. Deleting...");
                    // Close connection before deleting
                    await dbContext.Database.CloseConnectionAsync();
                    File.Delete(dbPath);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error checking database: {ex.Message}, recreating...");
            try {
                // Close connection before deleting
                await dbContext.Database.CloseConnectionAsync();
                if (File.Exists(dbPath)) {
                    File.Delete(dbPath);
                }
            } catch {
                // Ignore errors trying to delete - we'll recreate anyway
            }
        }
        
        // Create and seed the database
        Console.WriteLine("Creating database if it doesn't exist...");
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database setup complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
        // Continue even with database errors - app can still run
    }
}

app.Run();