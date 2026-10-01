using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Shopping.Web.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null)
        {
            var redirect = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Page("/Index");

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirect },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

    }
}
