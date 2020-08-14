using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImageOptimizer.Data;
using ImageOptimizer.Models;
using ImageOptimizer.Services;
using ImageOptimizer.Models.SubscriptionViewModels;
using ImageOptimizer.Models.ManageViewModels;

namespace ImageOptimizer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {           
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(config => { config.User.RequireUniqueEmail = true; })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddMvc();
            
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddTransient<ApplicationDbContextSeedData>();
            services.AddTransient<UserManagementService>();
            services.AddTransient<SubscriptionPlanManagementService>();
            services.AddTransient<BraintreeManagementService>();
            services.AddTransient<FormattingService>();

            // Add OptionModel.
            services.Configure<AppSettings>(options => Configuration.Bind(options));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContextSeedData seeder)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {                
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }
            
            app.UseStaticFiles();

            // Configure AutoMapper
            Mapper.Initialize(config =>
            {
                config.CreateMap<Address, BillingInformationViewModel>().ReverseMap();
                config.CreateMap<Address, EditBillingInformationViewModel>().ReverseMap();
            });

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            //app.UseFacebookAuthentication(options =>
            //{
            //    options.AppId = Configuration["Authentication:Facebook:AppId"];
            //    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            try
            {
                await seeder.EnsureSeedData();
            }
            finally
            {
                seeder?.Dispose();
            }
        }
    }
}
