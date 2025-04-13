using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;
using Newtonsoft.Json;

namespace MOOCSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync("api/Users");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<User>>();
                return View(users);
            }

            return View(new List<User>());
        }
    }
}
