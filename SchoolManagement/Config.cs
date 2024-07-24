using ApiResource = IdentityServer4.EntityFramework.Entities.ApiResource;
using ApiScope = IdentityServer4.EntityFramework.Entities.ApiScope;
using Client = IdentityServer4.EntityFramework.Entities.Client;
using IdentityResource = IdentityServer4.EntityFramework.Entities.IdentityResource;

namespace SchoolManagement;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {

        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
        };
}