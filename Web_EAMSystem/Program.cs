using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web_EAMSystem.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        // 操作逾時功能
        // 為「方便測試」，先設定成 1 分鐘！(測試完改回 30)
        option.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        //  新增 2：開啟「滑動過期」機制
        option.SlidingExpiration = true;
        //當沒登入時，不要去找 Login 頁面
        option.Events.OnRedirectToLogin = context =>
        {
            // 將他導向我們未來要做的「拒絕存取」警告頁面
            context.Response.Redirect("/Auth/AccessDenied");
            return Task.CompletedTask;
        };
        // 當權限不足時的導向
        option.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect("/Auth/AccessDenied");
            return Task.CompletedTask;
        };

    });

// 全域禁止未登入者連入 (FallbackPolicy)
builder.Services.AddAuthorization(options =>
    {
        // 設定全域預設策略：所有進入系統的請求，預設都必須是「已登入」狀態
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EAM_DBConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 啟用身分驗證 (必須在 UseAuthorization 的上面！)
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
