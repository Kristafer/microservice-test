using Catalog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog Api",
        Version = "v1",
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                TokenUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                Scopes = new Dictionary<string, string>()
                        {
                            { "catalog", "Catalog Api" }
                        }
            }
        }


    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// prevent from mapping "sub" claim to nameidentifier.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.Authority = identityUrl;
    options.RequireHttpsMetadata = false;
    options.Audience = "catalog";

    //options.TokenValidationParameters.ValidateAudience = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        
    };
});


//builder.Services.AddAuthentication(options =>
//            {
//                options.DefaultScheme = "cookies";
//                options.DefaultChallengeScheme = "oidc";
//            })
//                .AddCookie("cookies")
//                .AddOpenIdConnect("oidc", options =>
//                {
//                    options.Authority = identityUrl;
//                    options.ClientId = "catalog";
//                    options.MapInboundClaims = false;
//                    options.SaveTokens = true;
//                    if (builder.Environment.IsDevelopment())
//                    {
//                        This will allow the container to reach the discovery endpoint
//                        options.MetadataAddress = $"{identityUrl}/.well-known/openid-configuration";
//                        options.RequireHttpsMetadata = false;

//                        options.Events.OnRedirectToIdentityProvider = context =>
//                        {
//                            Intercept the redirection so the browser navigates to the right URL in your host
//                            context.ProtocolMessage.IssuerAddress = $"{identityUrl}/connect/authorize";
//                            return Task.CompletedTask;
//                        };
//                    }
//                });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "catalog");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
