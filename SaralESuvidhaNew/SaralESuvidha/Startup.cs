using System;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UPPCLLibrary;
using Microsoft.AspNetCore.Http;
using SaralESuvidha.Filters;
using SaralESuvidha.Services;
using Quartz;
using SaralESuvidha.QuartzJobs;

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
            StaticData.webhookSharedSecret = Configuration.GetValue<string>("AppConfigData:SharedSecret");
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
                options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin iframes
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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


            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new GlobalHighlightFilter());
            })
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

            services.Configure<SabPaisaOptions>(Configuration.GetSection("SabPaisa"));
            services.AddScoped<SabPaisaService>();
            services.AddHttpClient();
            services.AddQuartz(q =>
            {

                // Register the job
                var jobKey = new JobKey("CheckAndUpdateRazorpayStatusjob");
                var jobKey1 = new JobKey("SabpaisaStatusCheckJob");
                q.AddJob<CheckAndUpdateRazorpayStatusjob>(opts => opts.WithIdentity(jobKey));
                q.AddJob<SabpaisaStatusCheckJob>(opts => opts.WithIdentity(jobKey1));

                // Create a trigger for the job (every 10 seconds)
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("CheckAndUpdateRazorpayStatusjob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(2)
                        .RepeatForever()));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey1)
                    .WithIdentity("SabpaisaStatusCheckJob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(2)
                        .RepeatForever()));
            });
            services.AddQuartzHostedService(
                q => q.WaitForJobsToComplete = true);
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

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("X-Frame-Options");
                context.Response.Headers.Add(
                    "Content-Security-Policy",
                    "frame-ancestors 'self' chrome-extension://* https://ewallet.uppclonline.com https://ewallet-test.uppclonline.com");
                await next.Invoke();
            });

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
