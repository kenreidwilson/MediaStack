using MediaStack_API.Infrastructure;
using MediaStack_API.Middleware;
using MediaStack_API.Services.Thumbnailer;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using MediaStack_API.Services.CLI_Background_Services;
using MediaStackCore.Services.HasherService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API
{
    public class Startup
    {
        #region Data members

        private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Constructors

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        #endregion

        #region Methods

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(this.MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.AddControllers();

            services.AddTransient<DbContext, MediaStackContext>();
            services.AddTransient<IUnitOfWorkService, UnitOfWorkService>();
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IHasher, SH1Hasher>();
            services.AddSingleton<IFileSystemController, FileSystemController>();
            services.AddSingleton<IThumbnailer, Thumbnailer>();

            services.AddAutoMapper(typeof(DefaultAutoMapperProfile));

            services.AddHostedService<CLIBackgroundServiceHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAPILogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireCors(this.MyAllowSpecificOrigins); });
        }

        #endregion
    }
}