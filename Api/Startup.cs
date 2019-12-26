using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System;
using UrbanSisters.Dal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UrbanSisters.Api.Hubs;
using Microsoft.OpenApi.Models;
using AutoMapper;
using Azure.Storage.Blobs;
using UrbanSisters.Model;

namespace UrbanSisters.Api
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
            services.AddDbContext<UrbanSisterContext>((options) =>
            {
                options.UseSqlServer(Configuration["SqlConnectionString"]);
            });
            
            services.AddSingleton(new MapperConfiguration(mc => 
            {
                mc.CreateMap<Dto.UserInscription, User>();
                mc.CreateMap<User, Dto.User>().ForMember(dest => dest.IsRelookeuse, opt => opt.MapFrom(src => src.Relookeuse != null));
            }).CreateMapper());
            
            BlobServiceClient blobServiceClient = new BlobServiceClient(Configuration["BlobStorageConnectionString"]);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("profilepicture");
            services.AddSingleton(blobContainerClient);

            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["SecretSignatureKey"]));
            
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = "UrbanSistersServeurDeJetons";
                options.Audience = "http://localhost:5000";
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "UrbanSistersServeurDeJetons",

                ValidateAudience = true,
                ValidAudience = "http://localhost:5000",

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Audience = "http://localhost:5000";
                options.ClaimsIssuer = "UrbanSistersServeurDeJetons";
                options.TokenValidationParameters = tokenValidationParameters;
                options.SaveToken = true;
            });

            services.AddControllers();
            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UrbanSisters Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanSisters Api V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");
            });
        }
    }
}
