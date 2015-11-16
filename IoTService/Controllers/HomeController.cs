using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IoTService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View();

        [Route("led")]
        public ActionResult MulticolorLed() => View();

        [Route("humiture")]
        public ActionResult Humiture() => View();

        [Route("rover")]
        public ActionResult Rover() => View();

        [Route("Jukebox")]
        public ActionResult Jukebox() => View();
    }
}
