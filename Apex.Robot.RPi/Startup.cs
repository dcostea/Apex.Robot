using Apex.Robot.RPi.Hubs;
using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Apex.Robot.RPi.Services;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Apex.Robot.RPi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApiSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiSettings>>().Value);
            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<ISensorsService, SensorsService>();
            services.AddSingleton<IMotorsService, MotorsService>();
            services.AddSingleton<IPredictionsService, PredictionsService>();

            services.AddPredictionEnginePool<ModelInput, ModelOutput>()
               .FromFile(modelName: "sensorsModel", filePath: Configuration.GetValue<string>("AppSettings:ModelFilePath"), watchForChanges: true);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Apex.Robot.RPi", Version = "v1" });
            });

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Apex.Robot.RPi v1"));

            app.UseFileServer();

            app.UseRouting();

            var cameraHub = Configuration.GetSection("AppSettings").GetValue<string>("CameraHub");
            var sensorsHub = Configuration.GetSection("AppSettings").GetValue<string>("SensorsHub");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CameraHub>(cameraHub);
                endpoints.MapHub<SensorsHub>(sensorsHub);
                endpoints.MapControllers();
            });
        }
    }
}
