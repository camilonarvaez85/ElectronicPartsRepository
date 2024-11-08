using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

public class GlobalAuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (!HttpContext.Current.User.Identity.IsAuthenticated)
        {
            // Iniciar el proceso de autenticación
            HttpContext.Current.GetOwinContext().Authentication.Challenge(
                //TODO: dev or prod
                //new AuthenticationProperties { RedirectUri = "https://localhost:44323" },
                new AuthenticationProperties { RedirectUri = "https://web.osela.com/ElectroParts" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);

            filterContext.Result = new HttpUnauthorizedResult(); // Interrumpir la ejecución y redirigir
        }

        base.OnActionExecuting(filterContext);
    }
}
