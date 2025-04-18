using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using UPPCLLibrary;

namespace SaralESuvidha
{
    public class Startup
    {
        public static string ConnectionString { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            StaticData.loginSource = Configuration.GetValue<string>("AppConfigData:LoginSource");
            StaticData.rofferMobileUrl = Configuration.GetValue<string>("AppConfigData:RofferMobile");
            StaticData.rofferDthCustInfoUrl = Configuration.GetValue<string>("AppConfigData:DthcustInfo");
            StaticData.LoadSystemSetting();
            StaticDatabaseData.LoadAllConfigData();
            UPPCLManager.DbConnection = ConnectionString;
            UPPCLManager.Initialize();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton(Configuration);
            
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1200);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin() // or .WithOrigins("https://example.com") for specific domains
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });


            services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddHttpContextAccessor();
            //services.AddRazorPages();
            
            services.Configure<IISOptions>(options => 
            {
                options.ForwardClientCertificate = false;
            });
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
            }

            app.UseCors("AllowAll");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=ElectricityRecharge}/{id?}");
            });

            
        }
    }
}
