using api.Services;
using api.Models.Ttv;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System;
using IdentityModel.Client;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            // SQL Server options.
            // Enable retry on failure.
            // Split LINQ queriers into multiple SQL queries.
            services.AddDbContext<TtvContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    options =>
                    {
                        options.EnableRetryOnFailure();
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                )
            );

            services.AddControllers();

            // Swagger documentation
            if (Environment.IsDevelopment())
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Mydata API",
                        Description = "An API for Mydata frontend.",
                        Version = "v1"
                    });
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
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

                    string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
            }

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["KEYCLOAK:REALM"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            // Authorization
            services.AddAuthorization(options =>
            {
                /*
                 * Policy RequireScopeApi1AndClaimOrcid requires
                 *    - Authenticated user
                 *    - scope "api1"
                 *    - claim "orcid"
                 */
                options.AddPolicy("RequireScopeApi1AndClaimOrcid", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Required claim "orcid"
                    policy.RequireClaim("orcid");
                    // Required scope "api1". The following allows presence of other scopes.
                    string[] scopes = new[] { "api1" };
                    policy.RequireAssertion(context => {
                        System.Security.Claims.Claim claim = context.User.FindFirst("scope");
                        if (claim == null) { return false; }
                        return claim.Value.Split(' ').Any(s =>
                           scopes.Contains(s, StringComparer.Ordinal)
                        );
                    });
                });
            });

            // CORS policies
            services.AddCors(options =>
            {
                // Development and testing
                options.AddPolicy("development", builder =>
                {
                    builder.WithOrigins(
                        "https://*.csc.fi",
                        "https://*.rahtiapp.fi",
                        "https://localhost:5003"
                    )
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });

                // Production - TODO remove localhost 
                options.AddPolicy("production", builder =>
                {
                    builder.WithOrigins(
                        "https://*.csc.fi",
                        "https://*.rahtiapp.fi",
                        "https://localhost:5003"
                    )
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            /*
             * HTTP client: ORCID API
             */
            services.AddHttpClient("ORCID", httpClient =>
            {
                httpClient.BaseAddress = new Uri(Configuration["ORCID:API"]);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            /*
             * HTTP client: Keycloak Admin user token management (client credentials flow).
             * https://identitymodel.readthedocs.io/en/latest/aspnetcore/worker.html
             */
            services.AddClientAccessTokenManagement(options =>
            {
                options.Clients.Add("keycloakAdminTokenClient", new ClientCredentialsTokenRequest
                {
                    Address = Configuration["KEYCLOAK:TOKENENDPOINT"],
                    ClientId = Configuration["KEYCLOAK:ADMIN:CLIENTID"],
                    ClientSecret = Configuration["KEYCLOAK:ADMIN:CLIENTSECRET"]
                });
            });

            /*
             * HTTP client: Keycloak Admin API
             * https://www.keycloak.org/docs-api/15.0/rest-api/index.html
             * Access token management is provided by "keycloakAdminTokenClient".
             */
            services.AddClientAccessTokenHttpClient(clientName: "keycloakClient", tokenClientName: "keycloakAdminTokenClient", configureClient: client =>
            {
                client.BaseAddress = new Uri(Configuration["KEYCLOAK:ADMIN:REALMUSERSENDPOINT"]);
            });

            /*
             * HTTP client: Keycloak user's external IDP token retrieval.
             * https://wjw465150.gitbooks.io/keycloak-documentation/content/server_admin/topics/identity-broker/tokens.html
             * For retrieving user's external IDP (ORCID) access tokens from Keycloak.
             */
            services.AddHttpClient("keycloakUserOrcidTokens", httpClient =>
            {
                httpClient.BaseAddress = new Uri(Configuration["KEYCLOAK:ORCIDTOKENENDPOINT"]);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });


            services.AddResponseCompression();
            services.AddScoped<OrcidApiService>();
            services.AddScoped<OrcidImportService>();
            services.AddScoped<OrcidJsonParserService>();
            services.AddScoped<OrganizationHandlerService>();
            services.AddScoped<UserProfileService>();
            services.AddSingleton<ElasticsearchService>();
            services.AddSingleton<UtilityService>();
            services.AddSingleton<LanguageService>();
            services.AddSingleton<DataSourceHelperService>();
            services.AddScoped<DemoDataService>();
            services.AddScoped<TtvSqlService>();
            services.AddScoped<TokenService>();
            services.AddScoped<KeycloakAdminApiService>();
            services.AddScoped<DuplicateHandlerService>();
            services.AddScoped<StartupHelperService>();
            services.AddMemoryCache();

            services.AddHostedService<BackgroundElasticsearchUpdateService>();
            services.AddSingleton<BackgroundElasticsearchPersonUpdateQueue>();
            services.AddTransient<BackgroundProfiledata>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, StartupHelperService startupHelperService, DataSourceHelperService dataSourceHelperService)
        {
            // Initialize
            DimRegisteredDataSource dimRegisteredDataSource_ORCID = startupHelperService.GetDimRegisteredDataSourceId_OnStartup_ORCID();
            DimRegisteredDataSource dimRegisteredDataSource_TTV = startupHelperService.GetDimRegisteredDataSourceId_OnStartup_TTV();

            dataSourceHelperService.DimRegisteredDataSourceId_ORCID = dimRegisteredDataSource_ORCID.Id;
            dataSourceHelperService.DimRegisteredDataSourceName_ORCID = dimRegisteredDataSource_ORCID.Name;
            dataSourceHelperService.DimOrganizationId_ORCID = dimRegisteredDataSource_ORCID.DimOrganization.Id;
            dataSourceHelperService.DimOrganizationNameFi_ORCID = dimRegisteredDataSource_ORCID.DimOrganization.NameFi;
            dataSourceHelperService.DimOrganizationNameEn_ORCID = dimRegisteredDataSource_ORCID.DimOrganization.NameEn;
            dataSourceHelperService.DimOrganizationNameSv_ORCID = dimRegisteredDataSource_ORCID.DimOrganization.NameSv;

            dataSourceHelperService.DimRegisteredDataSourceId_TTV = dimRegisteredDataSource_TTV.Id;
            dataSourceHelperService.DimRegisteredDataSourceName_TTV = dimRegisteredDataSource_TTV.Name;
            dataSourceHelperService.DimOrganizationId_TTV = dimRegisteredDataSource_TTV.DimOrganization.Id;
            dataSourceHelperService.DimOrganizationNameFi_TTV = dimRegisteredDataSource_TTV.DimOrganization.NameFi;
            dataSourceHelperService.DimOrganizationNameEn_TTV = dimRegisteredDataSource_TTV.DimOrganization.NameEn;
            dataSourceHelperService.DimOrganizationNameSv_TTV = dimRegisteredDataSource_TTV.DimOrganization.NameSv;

            // Response compression.
            app.UseResponseCompression();

            // Use Forwarded Headers Middleware to enable client ip address detection behind load balancer.
            // Must run this before other middleware.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Add registered data sources, organizations etc. needed in demo.
            // Most of demo data is added to each user, who creates a profile.
            // demoDataService.InitDemo();

            // Development environment settings
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseResponseCompression();
            app.UseRouting();

            // CORS policy depends on the environment
            if (Environment.IsDevelopment())
            {
                app.UseCors("development");
            }
            if (Environment.IsProduction())
            {
                app.UseCors("production");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
