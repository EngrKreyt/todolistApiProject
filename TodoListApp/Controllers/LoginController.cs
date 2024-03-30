using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TodoListApp.Models;

namespace TodoListApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7100/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/authentication/login", content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                Response.Cookies.Append("AuthToken", token); // Store the token in a cookie named "AuthToken"

                // Save the token to use for subsequent requests or for authentication
                // Redirect to a different view upon successful login
                return RedirectToAction("Index", "Todo"); // Example redirect to Home/Index
            }
            else
            {
                // Handle failed login
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View("Login");
            }
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/authentication/register",content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                Response.Cookies.Append("AuthToken", token); // Store the token in a cookie named "AuthToken"

                // Save the token to use for subsequent requests or for authentication
                // Redirect to a different view upon successful login
                return View("Login");
            }
            else
            {
                // Handle failed login
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View("Login");
            }
        }

    }
}