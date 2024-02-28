using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace AuthBlazor
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        ];

        public static IEnumerable<ApiResource> ApiResourses =>
        [
                new ApiResource( "catalog", "Catalog Api")
            ];

        public static IEnumerable<ApiScope> ApiScopes =>
            [
            new ApiScope(name: "catalog", displayName: "Catalog Api")
            ];

        public static IEnumerable<Client> GetClients(IConfiguration configuration) =>
            [
            new Client
                {
                    ClientId = "catalog",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    ClientName = "Catalog Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{configuration["CatalogApi"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["CatalogApi"]}/swagger/" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "catalog"
                    }
                }
            ];
    }
}
