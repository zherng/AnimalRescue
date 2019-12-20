using AnimalRescue.API.Core;
using AnimalRescue.API.Core.Configuration;
using AnimalRescue.API.Core.Middlewares;
using AnimalRescue.BusinessLogic;
using AnimalRescue.Infrastructure.Configuration;

using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;

namespace AnimalRescue.API
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
            RuntimeConfiguration runtimeConfiguration = Configuration.GetTypedSection<RuntimeConfiguration>("Runtime"); 
            services.AddSingleton<IRuntimeConfiguration>(p => runtimeConfiguration);

            services.AddCors(options => 
            {
                options.AddDefaultPolicy(builder => 
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            services.AddControllers();
            
            services.AddConfigureSwagger();

            List<Profile> profiles = new List<Profile> { new ApiMappingProfile() };
            services.AddConfigureBusinessLogic(Configuration, profiles);
            var mappingConfig = new MapperConfiguration(mc => profiles.ForEach(x => mc.AddProfile(x)));
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddTransient<UnhandledExceptionMiddleware>(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<UnhandledExceptionMiddleware>(); 
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseConfigureSwagger();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
