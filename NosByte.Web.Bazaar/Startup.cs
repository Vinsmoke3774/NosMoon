using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NosByte.Web.Bazaar.Managers;
using OpenNos.DAL.DAO;
using OpenNos.DAL.Interface;

namespace NosByte.Web.Bazaar
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<UserManager>();
            services.AddSingleton<FrozenCrownManager>();

            // Register DAOS
            services.AddSingleton<IItemInstanceDAO, ItemInstanceDAO>();
            services.AddSingleton<ICharacterDAO, CharacterDAO>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            log4net.Config.XmlConfigurator.Configure();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
