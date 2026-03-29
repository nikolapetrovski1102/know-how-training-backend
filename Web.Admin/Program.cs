using Core.Application.Services;
using Infrastructure.Data.Services;
using Infrastructure.Services;
using Infrastructure.Services.Implementation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddHostedService<CacheWarmerService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("*", policy =>
        policy.WithOrigins("*")
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageAdminService, PageAdminService>();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddDbContext<Areas.Admin.Data.PageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_485_760;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors("*");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
