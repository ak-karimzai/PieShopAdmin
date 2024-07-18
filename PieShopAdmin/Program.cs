
using Microsoft.EntityFrameworkCore;
using PieShopAdmin.Models;
using PieShopAdmin.Models.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<PieShopDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PieShopDbContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    // for enabling store timestamp with UTC
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<PieShopDbContext>();
    DbInitializer.Seed(context);
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
