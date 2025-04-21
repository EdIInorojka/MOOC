using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;
using Newtonsoft.Json;
using System.Security.Claims;

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
            var response = await client.GetAsync("api/Courses");

            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<List<Course>>();
                return View(courses);
            }

            return View(new List<Course>());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("MOOCApi");

            var response = await client.PostAsync($"api/Users/{userId}/courses/{courseId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Вы успешно записаны на курс";
            }
            else
            {
                TempData["Error"] = "Ошибка при записи на курс";
            }

            return RedirectToAction("Index");
        }
    }
}
