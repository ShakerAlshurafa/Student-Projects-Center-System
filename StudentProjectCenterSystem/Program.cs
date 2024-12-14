using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenter.Infrastructure.Repositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using StudentProjectsCenterSystem.mapping_profile;
using StudentProjectsCenterSystem.Services;
using System.Text;

namespace StudentProjectCenterSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.CacheProfiles.Add("defaultCache", new CacheProfile()
                {
                    Duration = 30,
                    Location = ResponseCacheLocation.Any
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped(typeof(IProjectRepository), typeof(ProjectRepository));
            builder.Services.AddScoped(typeof(IAuthRepository), typeof(AuthRepository));
            builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
            builder.Services.AddScoped(typeof(ITokenServices), typeof(TokenService));
            builder.Services.AddScoped(typeof(IProjectDetailsSectionsRepository), typeof(ProjectDetailsSectionsRepository));
            builder.Services.AddScoped(typeof(IProjectDetailsRepository), typeof(ProjectDetailsRepository));
            builder.Services.AddScoped(typeof(IWorkgroupRepository), typeof(WorkgroupRepository));
            builder.Services.AddScoped(typeof(ITaskRepository), typeof(TaskRepository));
            builder.Services.AddScoped(typeof(ITermGroupRepository), typeof(TermGroupRepository));
            builder.Services.AddScoped(typeof(ITermRepository), typeof(TermRepository));

            builder.Services.AddScoped<AzureFileUploader>();

            builder.Services.AddTransient<IEmailService, EmailService>();

            // Tokens used for password reset, email confirmation, or account activation.
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(
                    builder.Configuration.GetValue<int>("TokenSettings:IdentityTokenLifespanHours")
                );
            });

            var key = builder.Configuration.GetValue<string>("ApiSetting:SecretKey");


            builder.Services.AddIdentity<LocalUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(x => x.Value?.Errors.Count() > 0)
                                                                .SelectMany(x => x.Value.Errors)
                                                                .Select(e => e.ErrorMessage)
                                                                .ToList();
                    var validationResponse = new ApiValidationResponse(errors);
                    return new BadRequestObjectResult(validationResponse);
                };
            });

            // Add CORS service with global policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()    // Allow requests from any origin
                          .AllowAnyHeader()    // Allow any header
                          .AllowAnyMethod();   // Allow any HTTP method (GET, POST, etc.)
                });
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Description = "Enter JWT Bearer token"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateLifetime = true
                };
            });

            var app = builder.Build();

            // Use the CORS policy globally
            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            //  {
            app.UseSwagger();
            app.UseSwaggerUI();
            // }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
