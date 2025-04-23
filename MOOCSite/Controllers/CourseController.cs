using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;
using System.Security.Claims;

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
            var response = await client.GetAsync($"api/Courses/{id}?includeDisciplines=true");

            if (response.IsSuccessStatusCode)
            {
                var course = await response.Content.ReadFromJsonAsync<Course>();

                // Загружаем дисциплины, если они не были включены в ответ
                if (course.Disciplines == null || !course.Disciplines.Any())
                {
                    var disciplinesResponse = await client.GetAsync($"api/Courses/{id}/disciplines");
                    if (disciplinesResponse.IsSuccessStatusCode)
                    {
                        course.Disciplines = await disciplinesResponse.Content.ReadFromJsonAsync<List<Discipline>>();
                    }
                }

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var checkResponse = await client.GetAsync($"api/Users/{userId}/courses/{id}/isEnrolled");
                    if (checkResponse.IsSuccessStatusCode)
                    {
                        ViewBag.IsEnrolled = await checkResponse.Content.ReadFromJsonAsync<bool>();
                    }
                }

                return View(course);
            }

            return NotFound();
        }
    }
}
