using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using WebApiAuthSample.Models;

namespace WebApiAuthSample.Controllers
{
    public class HomeController : Controller
    {
        private static Token _myToken;

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
                !String.Equals(password, passwordConfirm))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    "Registration form is invalid");
            }

            var registerModel = new Dictionary<string, string>
            {
                {"Email", email},
                {"Password", password},
                {"ConfirmPassword", password}
            };

            var response = await CallApiTask("api/Account/Register", registerModel);

            if (response.IsSuccessStatusCode)
                return new HttpStatusCodeResult(response.StatusCode);

            var errors = await response.Content.ReadAsAsync<ResponseErrors>();
            return new HttpStatusCodeResult(response.StatusCode, errors.ToString());
        }

        [HttpPost]
        public async Task<JsonResult> GetAccessToken(string tokenEmail, string tokenPassword)
        {
            if (String.IsNullOrEmpty(tokenEmail) || String.IsNullOrEmpty(tokenPassword))
            {
                throw new ArgumentNullException();
            }

            var tokenModel = new Dictionary<string, string>
            {
                {"grant_type", "password"},
                {"username", tokenEmail},
                {"password", tokenPassword},
            };

            var response = await CallApiTask("api/authtoken", tokenModel);

            if (!response.IsSuccessStatusCode)
            {
                var errors = await response.Content.ReadAsStringAsync();
                throw new Exception(errors);
            }

            _myToken = response.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() }).Result;

            return Json(_myToken);
        }

        public ActionResult Logout()
        {
            _myToken = null;

            return RedirectToAction("Index");
        }

        public async Task<ICollection<string>> GetValues()
        {
            if (_myToken != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiUri);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _myToken.AccessToken);
                    var authorizedResponse = client.GetAsync("api/values").Result;

                    if (!authorizedResponse.IsSuccessStatusCode) return null;

                    var res = await authorizedResponse.Content.ReadAsAsync<List<string>>();
                    return res;
                }
            }
            return null;
        }

        private static async Task<HttpResponseMessage> CallApiTask(string apiEndPoint, Dictionary<string, string> model = null)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUri);
                client.DefaultRequestHeaders.Accept.Clear();

                if (_myToken != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _myToken.AccessToken);
                }
                else
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                return await client.PostAsync(apiEndPoint, model != null ? new FormUrlEncodedContent(model) : null);
            }
        }
    }
}
