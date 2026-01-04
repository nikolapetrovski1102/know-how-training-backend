using Core.Application.Services;
using Infrastructure.Data.Services;
using Infrastructure.Services;
using Infrastructure.Services.Implementation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddHostedService<CacheWarmerService>();

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageAdminService, PageAdminService>();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddDbContext<Areas.Admin.Data.PageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages(options => options.RootDirectory = "/Areas/Admin/Pages");

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("ReactDev");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();