using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ErrorHandlerAsyncWtf.Controllers
{
    public class ErrorsController : Controller
    {
        [Route("errors")]
        [Route("errors/{code}")]
        public ActionResult Index(int? code)
        {
            return Content($"Hello, {code}!");
        }
    }
}