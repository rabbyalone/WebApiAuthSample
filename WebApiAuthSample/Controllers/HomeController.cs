using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using WebApiAuthSample.Models;

namespace WebApiAuthSample.Controllers
{
    public class HomeController : Controller
    {
        private const string ApiUri = "https://localhost:44301/";

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public async Task<HttpStatusCodeResult> Register(string email, string password, string passwordConfirm)
        {
            if (String.IsNullOrEmpty(email) || 
                String.IsNullOrEmpty(password) || 
                !String.Equals(password,passwordConfirm))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, 
                    "Registration form is invalid");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUri);
                client.DefaultRequestHeaders.Accept.Clear();

                var registerModel = new Dictionary<string, string>
                {
                    {"Email", email},
                    {"Password", password},
                    {"ConfirmPassword", password}
                };

                var response = await client.PostAsJsonAsync("api/Account/Register", registerModel);

                if (response.IsSuccessStatusCode) 
                    return new HttpStatusCodeResult(response.StatusCode);

                var errors = await response.Content.ReadAsAsync<ResponseErrors>();
                return new HttpStatusCodeResult(response.StatusCode, errors.ToString());
            }
        }
    }
}
