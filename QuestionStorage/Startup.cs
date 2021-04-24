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
using QuestionStorage.Models.Options;
using QuestionStorage.Models.Users;
using UserOptions = Microsoft.AspNetCore.Identity.UserOptions;

namespace QuestionStorage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            
            services.AddRazorPages();

            services.AddDbContext<StorageContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("QuestionsStorageContextConnection"));
                options.EnableSensitiveDataLogging();
            });
            
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
            
            services.Configure<QuestionOptions>(Configuration.GetSection("QuestionSettings"));
            services.Configure<UserOptions>(Configuration.GetSection("UserSettings"));
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
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
                    name: "displayWithTags",
                    pattern: "{controller=Display}/{action=ListQuestions}/{courseId}/{tags?}");
                
                endpoints.MapControllerRoute(
                    name: "displayGenerated",
                    pattern: "{controller=Display}/{action=ListQuestions}/{courseId}/{questions?}");
                
                endpoints.MapControllerRoute(
                    name: "quiz",
                    pattern: "{controller=Quizzes}/{action=Index}/{courseId}/{quizId?}");
                
                endpoints.MapControllerRoute(
                    name: "tags",
                    pattern: "{controller=Quizzes}/{action=Index}/{courseId}/{tagId?}");
                
                endpoints.MapControllerRoute(
                    name: "restorePassword",
                    pattern: "{controller=Account}/{action=Index}/{token?}");
            });
        }
    }
}