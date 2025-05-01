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

            // Получаем курс с включенными связанными данными
            var response = await client.GetAsync($"api/Courses/{id}?includeDisciplines=true&includeUniversity=true&includeLecturers=true");

            if (response.IsSuccessStatusCode)
            {
                var course = await response.Content.ReadFromJsonAsync<Course>();

                // Получаем университет, если он не был включен в ответ
                if (course.University == null && course.UniversityId.HasValue)
                {
                    var universityResponse = await client.GetAsync($"api/Universities/{course.UniversityId}");
                    if (universityResponse.IsSuccessStatusCode)
                    {
                        course.University = await universityResponse.Content.ReadFromJsonAsync<University>();
                    }
                }

                // Получаем лекторов, если они не были включены в ответ
                if (course.Lecturers == null || !course.Lecturers.Any())
                {
                    var lecturersResponse = await client.GetAsync($"api/Courses/{id}/lecturers");
                    if (lecturersResponse.IsSuccessStatusCode)
                    {
                        course.Lecturers = await lecturersResponse.Content.ReadFromJsonAsync<List<Lecturer>>();
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