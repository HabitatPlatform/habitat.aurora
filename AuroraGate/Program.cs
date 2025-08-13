using AuroraGate.Components;
using AuroraGate.Data;
using AuroraGate.Domain;
using AuroraGate.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(cfg.ConnectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<MongoClient>();
    return client.GetDatabase(cfg.Database);
});


builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IMongoCollection<User>>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return db.GetCollection<User>(cfg.UsersCollection);
});
builder.Services.AddSingleton<IMongoCollection<Role>>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return db.GetCollection<Role>(cfg.RolesCollection);
});
builder.Services.AddSingleton<IMongoCollection<RefreshToken>>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return db.GetCollection<RefreshToken>(cfg.RefreshTokensCollection);
});


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();




var app = builder.Build();





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
