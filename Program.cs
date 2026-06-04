using DATN.Services;
using DATN.Services.Implementations;
using DATN.Services.Interfaces;
using YourApp.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;
using DATN.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Redirect to login page when user is not authenticated
                    options.LoginPath = "/Account/Login";

                    // Redirect when user is authenticated but not authorized
                    options.AccessDeniedPath = "/Account/Login";

                    // Cookie lifetime
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                });

            // Add services for Razor Pages and MVC controllers with views
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Application services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Important: authentication must come before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Razor Pages first (Razor Pages prioritized in this workspace)
            app.MapRazorPages();

            // Controller routes (including areas)
            app.MapControllerRoute(
                name: "MyAreas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}