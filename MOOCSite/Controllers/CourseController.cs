using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;

namespace MOOCSite.Controllers
{
    public class CourseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CourseController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync($"api/Courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var course = await response.Content.ReadFromJsonAsync<Course>();
                return View(course);
            }

            return NotFound();
        }
    }
}
