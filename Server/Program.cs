using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Server.BGTasks;
using System.Composition;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<dbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("dbServerContext") ?? throw new InvalidOperationException("Connection string 'STServerContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

			builder.Services.AddHostedService<EvidenceChecker>();

			var app = builder.Build();
			Console.WriteLine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
            
        }
    }
}