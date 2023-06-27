using Apex.Robot.Vision.API.Models;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace Apex.Robot.Vision
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
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.Configure<ImageSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<ModelSettings>(Configuration.GetSection("ModelSettings"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ImageSettings>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ModelSettings>>().Value);

            //services.AddPredictionEnginePool<ImageNetData, ImageNetPrediction>()
            //    .FromFile(modelName: "imagePrediction", filePath: Configuration.GetValue<string>("ModelSettings:RetrainedModelFilePath"), watchForChanges: false);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Apex.Robot.Vision API", Version = "v1" });
            });

            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Apex.Robot.Vision API v1"));

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseFileServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
