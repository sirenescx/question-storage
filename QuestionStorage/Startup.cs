using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using QuestionStorage.Models;

namespace QuestionStorage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            
            services.AddRazorPages();
            
            services.AddDbContext<StorageContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("QuestionsStorageContextConnection")));
            
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Display/ListCourses");
                });
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(  
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "question",
                    pattern: "{controller=Questions}/{action=Index}/{courseId}/{questionId?}");

                endpoints.MapControllerRoute(
                    name: "display",
                    pattern: "{controller=Display}/{action=ListQuestions}/{courseId?}");
                
                endpoints.MapControllerRoute(
                    name: "quiz",
                    pattern: "{controller=Quizzes}/{action=Index}/{courseId}/{quizId?}");
                
                endpoints.MapControllerRoute(
                    name: "tags",
                    pattern: "{controller=Quizzes}/{action=Index}/{courseId}/{tagId?}");
            });
        }
    }
}