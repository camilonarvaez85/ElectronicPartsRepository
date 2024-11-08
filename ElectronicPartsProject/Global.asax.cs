using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace ElectronicPartsProject
{
    
    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {

            //if (!Request.IsAuthenticated)
            //{
            //    HttpContext.Current.GetOwinContext().Authentication.Challenge(
            //        new AuthenticationProperties { RedirectUri = "/" },
            //        OpenIdConnectAuthenticationDefaults.AuthenticationType);
            //}

            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new GlobalAuthFilter());  // Agregar el filtro global
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_BeginRequest()
        //{
        //    if (!Request.IsAuthenticated)
        //    {
        //        HttpContext.Current.GetOwinContext().Authentication.Challenge(
        //            new AuthenticationProperties { RedirectUri = "/" },
        //            OpenIdConnectAuthenticationDefaults.AuthenticationType);
        //    }
        //}


    }
}
