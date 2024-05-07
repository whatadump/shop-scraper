namespace ShopScraper.Web.Client.Services;

using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class UserManager
{
    private IHttpContextAccessor _accessor;
    private UserManager<ApplicationUser> _manager;

    public UserManager(IHttpContextAccessor accessor, UserManager<ApplicationUser> manager)
    {
        _accessor = accessor;
        _manager = manager;
    }

    public async Task<ApplicationUser> GetAuthenticatedUserAsync()
    {
        return await _manager.GetUserAsync(_accessor.HttpContext.User);
    }
}