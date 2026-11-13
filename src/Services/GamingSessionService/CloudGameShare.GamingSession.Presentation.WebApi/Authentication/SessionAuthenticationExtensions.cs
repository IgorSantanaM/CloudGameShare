using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CloudGameShare.GamingSession.Presentation.WebApi.Authentication;

internal static class SessionAuthenticationExtensions
{
    public static void AddSessionAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var developmentUser = environment.IsDevelopment() ? configuration["Authentication:DevelopmentUserId"] : null;
        if (!string.IsNullOrWhiteSpace(developmentUser))
        {
            services.AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                    DevelopmentAuthenticationHandler.SchemeName, _ => { });
        }
        else
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.MapInboundClaims = false;
            });
        }

        services.AddAuthorizationBuilder().AddPolicy("SessionOwner", policy =>
            policy.RequireAuthenticatedUser().RequireAssertion(context =>
                !string.IsNullOrWhiteSpace(context.User.FindFirst("sub")?.Value)));
    }
}
