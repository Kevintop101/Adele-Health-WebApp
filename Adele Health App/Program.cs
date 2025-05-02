using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Adele_Health_App.Areas.Identity.Data;
using Adele_Health_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


//No need to touch this, open up pages and go to index.cshtml
var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("AdeleHealthAppAuthConnection") ?? throw new InvalidOperationException("Connection string 'AdeleHealthAppAuthConnection' not found.");

var appDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection - logging' not found.");


builder.Services.AddDbContext<AdeleHealthAppAuth>(options => options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(appDbConnectionString));
builder.Services.AddSession();


builder.Services.AddDefaultIdentity<AdeleHealthAppUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<AdeleHealthAppAuth>();




builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();


app.Run();
