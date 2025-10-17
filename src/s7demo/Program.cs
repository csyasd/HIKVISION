using S7Demo.Models;
using S7Demo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 配置PLC连接参数
var plcConfig = new PlcConfig();
builder.Configuration.GetSection("PlcConfig").Bind(plcConfig);
builder.Services.AddSingleton(plcConfig);

// 注册S7 PLC服务
builder.Services.AddSingleton<S7PlcService>();

// 添加CORS支持（如果需要跨域访问）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 启用CORS
app.UseCors("AllowAll");

app.UseAuthorization();

// 配置路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 添加API路由
app.MapControllers();

app.Run();
