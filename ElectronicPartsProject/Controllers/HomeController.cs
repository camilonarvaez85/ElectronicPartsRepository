using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicPartsProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            

            return RedirectToAction("Index", "Part"); 
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void SignOut()
        {
            //TODO: Uncomment Sign Out url
            //dev
            //var callBackUrl = "https://localhost:44313/";
            //local
            //var callBackUrl = "https://localhost/PortalApps/";
            //Prod
            var callBackUrl = "https://web.osela.com/PortalAppsTest/";

            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callBackUrl // Establecer la URL de redirección después de cerrar la sesión
                },
                CookieAuthenticationDefaults.AuthenticationType,
                OpenIdConnectAuthenticationDefaults.AuthenticationType
            );
        }
    }
}