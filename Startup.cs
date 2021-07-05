using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZawagProject.Data;
using ZawagProject.Helpers;
using Microsoft.AspNetCore.SignalR;
using ZawagProject.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Stripe;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ZawagProject
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
       

        public Startup(IConfiguration configuration,IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.


        [Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            //register dbContext
            services.AddDbContext<DataContext>(option =>
            {
                option.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).ConfigureWarnings(warnings=>warnings.Ignore(CoreEventId.IncludeIgnoredWarning));
            });
            //identityConfiguration
            IdentityBuilder builder=services.AddIdentityCore<User>(option=>{
                option.Password.RequireDigit=false;
                option.Password.RequiredLength=4;
                option.Password.RequireNonAlphanumeric=false;
                option.Password.RequireUppercase=false;
            });
            builder=new IdentityBuilder(builder.UserType,typeof(Role),builder.Services);
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();
               //Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    //match token of user with key 
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection
                    ("AppSettings:Token").Value)),
                    //in Development must be false
                    ValidateIssuer = false,
                    //No send Token to third party 
                    ValidateAudience = false
                };
            });
            //authorization policy
            services.AddAuthorization(op=>{
               op.AddPolicy("RequireAdminRole",policy=>policy.RequireRole("Admin"));
               op.AddPolicy("ModeratePhotoRole",policy=>policy.RequireRole("Admin","Moderator"));
               op.AddPolicy("VipOnly",policy=>policy.RequireRole("VIP"));
               


            });
            //remove traceId and title from Api And filter it with errors only
            //make authorize global in all projcet
            services.AddControllers(options=>{
                var policy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            } 
                /* if have problem in json*/
            ).AddJsonOptions(opt=>{
                opt.JsonSerializerOptions.WriteIndented=_environment.IsDevelopment();
            });
              
    
            //seeding data 
           services.AddTransient<TrialData>();
           //add documnets 
            services.AddSingleton(typeof(IConverter),new SynchronizedConverter(new PdfTools()));
            
            services.AddCors();
             //autoMapper service
             services.AddAutoMapper();
              //Mapper.Reset();
            services.AddSignalR();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            //stripe
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            //register Repository
            
            services.AddScoped<IAuthRepository, AuthRepository>();
            //ZawajRepository
            services.AddScoped<IZawajRepository,ZawajRepository>();


            //update lastAcrive of user
            services.AddScoped<LogUserActivity>();

         
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,TrialData trialData)
        {
            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe:SecretKey").Value);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }else{
                app.UseExceptionHandler(BuilderExcption=>{
                  BuilderExcption.Run(async context=>{
                   context.Response.StatusCode=(int)HttpStatusCode.InternalServerError;
                   var error=context.Features.Get<IExceptionHandlerFeature>();
                   if(error !=null){
                       context.Response.AddApplicationError(error.Error.Message);
                       await context.Response.WriteAsync(error.Error.Message);
                   }
                  });
                });
            }
              //trialData.TrialUsers();
            app.UseCors(x => x.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials);
            //chat configuration
            app.UseSignalR(routes=>{
             routes.MapHub<ChatHub>("/chat");
            });
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
         
            //when build angular to wwwroot 
            app.Use(async(context,next)=>{
                await next();
                if(context.Response.StatusCode==404){
                    context.Request.Path="/index.html";
                    await next();
                }
             
            });
              app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
