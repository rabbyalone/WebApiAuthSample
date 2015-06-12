using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace WebApiAuthSample.Filters
{
    public class RequireWebApiHttpsAttribute : AuthorizationFilterAttribute
    {
        private const int Port = 43001;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS Required"
                };
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }
    }


    public class AbcAttribute : RequireHttpsAttribute
    {
        private readonly int _httpsPort;

        public AbcAttribute()
        {
#if DEBUG
            //set DEBUG port
            _httpsPort = 44301;
#else
            //set Run port
            _httpsPort = 443;
#endif
        }

        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                base.HandleNonHttpsRequest(filterContext);
                //throw new InvalidOperationException("ToggleHttpHttpsAttribute can only be used on GET requests.");
            }

            var request = filterContext.HttpContext;

            filterContext.Result = GetRedirectResult(request.Request);
        }

       private RedirectResult GetRedirectResult(HttpRequestBase request)
       {
           if (request.Url != null)
           {
               var uri = new UriBuilder(request.Url) {Scheme = Uri.UriSchemeHttps, Port = _httpsPort};

               return new RedirectResult(uri.ToString(), true);
           }

           throw new InvalidOperationException("ToggleHttpHttpsAttribute can only be used on GET requests.");
       }
    }

}