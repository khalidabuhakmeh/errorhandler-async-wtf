using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ErrorHandlerAsyncWtf.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("sync")]
        public ActionResult Sync()
        {
            throw new Exception();
        }

        [Route("async")]
        public ActionResult Async()
        {
            var yep = Blowup().Result;
            return Content("don't get here");
        }

        [Route("evil")]
        public ActionResult Evil(string name)
        {
            return View();
        }

        public async Task<string> Blowup()
        {
            throw new Exception();
        }

    }
}