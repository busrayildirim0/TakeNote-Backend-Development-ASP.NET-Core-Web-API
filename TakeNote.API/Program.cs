using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TakeNote.API.Hubs;
using TakeNote.API.Middlewares; // Middleware namespace
using TakeNote.API.Services;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Core.Settings; // Settings namespace
using TakeNote.DataAccess;
using TakeNote.DataAccess.Repositories;
using TakeNote.Service.Interfaces;
using TakeNote.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURATION (Options Pattern)
// JwtSettings sınıfını appsettings.json ile eşleştiriyoruz.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 2. DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TakeNoteDB"));

// 3. IDENTITY
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.SetIsOriginAllowed(_ => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

// 5. DEPENDENCY INJECTION
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ITaskItemService, TaskItemService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// 6. JWT Authentication
// Ayarları Options Pattern ile alıyoruz ama burada startup anında okumak için bind ediyoruz.
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>();
var secretKey = Encoding.UTF8.GetBytes(jwtSettings!.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR Token Handling
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/collaborationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 7. LOGGING (Varsayılan loglama zaten geliyor ama console loglarını netleştirelim)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Swagger Config
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TakeNote API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// --- HTTP REQUEST PIPELINE ---

// [ÖNEMLİ] Global Exception Middleware - En başa yakın olmalı
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<CollaborationHub>("/collaborationHub");
app.MapControllers();

// Seeding default users (Alice, Bob, Charlie) and workspaces/notes
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var dbContext = services.GetRequiredService<AppDbContext>();
        
        // 1. Seed default roles
        var roles = new[] { "Admin", "Editor", "Viewer" };
        foreach (var roleName in roles)
        {
            if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName }).GetAwaiter().GetResult();
            }
        }
        
        // 2. Seed default users
        var aliceUser = userManager.FindByEmailAsync("alice@takenote.com").GetAwaiter().GetResult();
        var bobUser = userManager.FindByEmailAsync("bob@takenote.com").GetAwaiter().GetResult();
        var charlieUser = userManager.FindByEmailAsync("charlie@takenote.com").GetAwaiter().GetResult();
        
        if (aliceUser == null)
        {
            aliceUser = new User { UserName = "alice", Email = "alice@takenote.com", EmailConfirmed = true };
            userManager.CreateAsync(aliceUser, "Password123!").GetAwaiter().GetResult();
        }
        if (bobUser == null)
        {
            bobUser = new User { UserName = "bob", Email = "bob@takenote.com", EmailConfirmed = true };
            userManager.CreateAsync(bobUser, "Password123!").GetAwaiter().GetResult();
        }
        if (charlieUser == null)
        {
            charlieUser = new User { UserName = "charlie", Email = "charlie@takenote.com", EmailConfirmed = true };
            userManager.CreateAsync(charlieUser, "Password123!").GetAwaiter().GetResult();
        }
        
        // 3. Seed some workspaces, members, notes and tasks if db is empty
        if (!dbContext.Workspaces.Any())
        {
            var ws1 = new Workspace
            {
                Title = "Project TakeNote Team",
                Description = "Primary workspace for building the TakeNote Frontend and Realtime collaboration.",
                IsPrivate = false,
                OwnerId = aliceUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Workspaces.Add(ws1);
            dbContext.SaveChanges(); // get Id
            
            var memberAlice = new WorkspaceMember { UserId = aliceUser.Id, WorkspaceId = ws1.Id, Role = "Owner", JoinedAt = DateTime.UtcNow };
            var memberBob = new WorkspaceMember { UserId = bobUser.Id, WorkspaceId = ws1.Id, Role = "Editor", JoinedAt = DateTime.UtcNow };
            var memberCharlie = new WorkspaceMember { UserId = charlieUser.Id, WorkspaceId = ws1.Id, Role = "Viewer", JoinedAt = DateTime.UtcNow };
            dbContext.WorkspaceMembers.AddRange(memberAlice, memberBob, memberCharlie);
            
            var note1 = new Note
            {
                Title = "Frontend Styling & Custom Guidelines",
                Content = "Here are the core rules:\n1. Keep it glassmorphic!\n2. Dark mode defaults with indigo colors.\n3. Make sure auto-saves happen smoothly.",
                IsPinned = true,
                WorkspaceId = ws1.Id,
                CreatedById = aliceUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = new List<string> { "design", "css", "guideline" }
            };
            dbContext.Notes.Add(note1);
            dbContext.SaveChanges();
            
            var task1 = new TaskItem { NoteId = note1.Id, Description = "Design the style.css variables", IsCompleted = true, DueDate = DateTime.UtcNow.AddDays(1) };
            var task2 = new TaskItem { NoteId = note1.Id, Description = "Implement SignalR connection handlers in app.js", IsCompleted = false, DueDate = DateTime.UtcNow.AddDays(3), AssignedToId = bobUser.Id };
            var task3 = new TaskItem { NoteId = note1.Id, Description = "Perform cross-browser check", IsCompleted = false, DueDate = DateTime.UtcNow.AddDays(5), AssignedToId = charlieUser.Id };
            dbContext.TaskItems.AddRange(task1, task2, task3);
            
            var personalNote = new Note
            {
                Title = "Alice's Secret Ideas",
                Content = "Research offline-first service worker sync for notes storage next week.",
                IsPinned = false,
                WorkspaceId = null,
                CreatedById = aliceUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = new List<string> { "research", "secret" }
            };
            dbContext.Notes.Add(personalNote);
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding data: {ex.Message}");
    }
}

app.Run();