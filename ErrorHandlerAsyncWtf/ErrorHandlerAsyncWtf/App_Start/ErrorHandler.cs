using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace ErrorHandlerAsyncWtf
{
    public class ErrorHandler : IHttpModule
    {
        private HttpContext HttpContext { get { return HttpContext.Current; } }
        private CustomErrorsSection CustomErrors { get; set; }

        public void Init(HttpApplication application)
        {
            System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
            CustomErrors = (CustomErrorsSection)configuration.GetSection("system.web/customErrors");

            application.EndRequest += Application_EndRequest;
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {

            // only handle rewrite mode, ignore redirect configuration (if it ain't broke don't re-implement it)
            if (CustomErrors.RedirectMode == CustomErrorsRedirectMode.ResponseRewrite && HttpContext.IsCustomErrorEnabled)
            {

                int statusCode = HttpContext.Response.StatusCode;

                // if this request has thrown an exception then find the real status code
                Exception exception = HttpContext.Error;
                if (exception != null)
                {
                    // set default error status code for application exceptions
                    statusCode = (int)HttpStatusCode.InternalServerError;
                }

                HttpException httpException = exception as HttpException;
                if (httpException != null)
                {
                    statusCode = httpException.GetHttpCode();
                }

                if ((HttpStatusCode)statusCode != HttpStatusCode.OK)
                {

                    Dictionary<int, string> errorPaths = new Dictionary<int, string>();

                    foreach (CustomError error in CustomErrors.Errors)
                    {
                        errorPaths.Add(error.StatusCode, error.Redirect);
                    }

                    // find a custom error path for this status code
                    if (errorPaths.Keys.Contains(statusCode))
                    {
                        string url = errorPaths[statusCode];

                        // avoid circular redirects
                        if (!HttpContext.Request.Url.AbsolutePath.Equals(VirtualPathUtility.ToAbsolute(url)))
                        {

                            HttpContext.Response.Clear();
                            HttpContext.Response.TrySkipIisCustomErrors = true;

                            HttpContext.Server.ClearError();

                            // do the redirect here
                            if (HttpRuntime.UsingIntegratedPipeline)
                            {
                                HttpContext.Server.TransferRequest(url, true);
                            }
                            else
                            {
                                HttpContext.RewritePath(url, false);

                                IHttpHandler httpHandler = new MvcHttpHandler();
                                httpHandler.ProcessRequest(HttpContext);
                            }

                            // return the original status code to the client
                            // (this won't work in integrated pipleline mode)
                            HttpContext.Response.StatusCode = statusCode;

                        }
                    }

                }

            }

        }

        public void Dispose()
        {

        }


    }
}