using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Implementations;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Register here ya shabab
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IGameQuestionRepository, GameQuestionRepository>();
builder.Services.AddScoped<IGamePlayerRepository, GamePlayerRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IPlayerAnswerRepository, PlayerAnswerRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<QuizGame.Hubs.GameHub>("/hubs/game");


using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();