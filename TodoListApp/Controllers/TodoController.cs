using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using TodoListApp.Models;
using Newtonsoft.Json.Linq;
using System.Text;
namespace TodoListApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TodoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            // Ensure token is not null or empty before proceeding
            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page or handle unauthorized access
                return RedirectToAction("Login", "Authentication");
            }
            string tokenJson = token;
            string bearertoken = JObject.Parse(tokenJson)["token"].ToString();
            var todos = await GetTodosAsync(bearertoken);

            return View(todos);
        }

        private async Task<List<Todo>> GetTodosAsync(string token)
        {
            var todos = new List<Todo>();

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7100/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync("api/Todo/GetTodos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    todos = JsonConvert.DeserializeObject<List<Todo>>(content);
                }
                else
                {
                    // Handle unsuccessful response
                }
            }

            return todos;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTodo(Todo todo)
        {

            var token = Request.Cookies["AuthToken"];
            // Ensure token is not null or empty before proceeding
            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page or handle unauthorized access
                return RedirectToAction("Login", "Authentication");
            }string tokenJson = token;
            string bearertoken = JObject.Parse(tokenJson)["token"].ToString();

            var content = new StringContent(JsonConvert.SerializeObject(todo), Encoding.UTF8, "application/json");

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7100/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearertoken);

                var response = await httpClient.PostAsync("api/Todo/CreateTodo", content);

                if (response.IsSuccessStatusCode)
                {
                    // Your logic for successful response
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle unsuccessful response
                    return StatusCode((int)response.StatusCode);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTodo(Todo todo)
        {
            var token = Request.Cookies["AuthToken"];
            // Ensure token is not null or empty before proceeding
            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page or handle unauthorized access
                return RedirectToAction("Login", "Authentication");
            }
            string tokenJson = token;
            string bearertoken = JObject.Parse(tokenJson)["token"].ToString();

            var content = new StringContent(JsonConvert.SerializeObject(todo), Encoding.UTF8, "application/json");

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7100/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearertoken);

                // Concatenate the TodoId to the endpoint URL
                var updateUrl = $"api/Todo/UpdateTodo/{todo.TodoId}";

                var response = await httpClient.PutAsync(updateUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Your logic for successful response
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle unsuccessful response
                    return StatusCode((int)response.StatusCode);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTodo(int todoId)
        {
            var token = Request.Cookies["AuthToken"];
            // Ensure token is not null or empty before proceeding
            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login page or handle unauthorized access
                return RedirectToAction("Login", "Authentication");
            }
            string tokenJson = token;
            string bearertoken = JObject.Parse(tokenJson)["token"].ToString();

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7100/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearertoken);

                // Concatenate the TodoId to the endpoint URL
                var deleteUrl = $"api/Todo/DeleteTodo/{todoId}";

                var response = await httpClient.DeleteAsync(deleteUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Your logic for successful response
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle unsuccessful response
                    return StatusCode((int)response.StatusCode);
                }
            }
        }
    }
}
