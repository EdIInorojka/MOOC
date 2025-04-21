using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;
using MOOCSite.ViewModels;
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
                var viewModels = new List<CourseWithEnrollmentViewModel>();

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    foreach (var course in courses)
                    {
                        var checkResponse = await client.GetAsync($"api/Users/{userId}/courses/{course.Id}/isEnrolled");
                        var isEnrolled = checkResponse.IsSuccessStatusCode && await checkResponse.Content.ReadFromJsonAsync<bool>();
                        viewModels.Add(new CourseWithEnrollmentViewModel { Course = course, IsEnrolled = isEnrolled });
                    }
                }
                else
                {
                    viewModels = courses.Select(c => new CourseWithEnrollmentViewModel { Course = c }).ToList();
                }

                return View(viewModels);
            }

            return View(new List<CourseWithEnrollmentViewModel>());
        }

        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Перенаправит на страницу входа
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Добавим логирование для отладки
            Console.WriteLine($"Requesting courses for user {userId}");

            try
            {
                var response = await client.GetAsync($"api/Users/{int.Parse(userId)}/courses");

                Console.WriteLine($"API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var courses = await response.Content.ReadFromJsonAsync<List<Course>>();
                    Console.WriteLine($"Received {courses?.Count ?? 0} courses");
                    return View(courses ?? new List<Course>());
                }

                Console.WriteLine($"Error: {response.ReasonPhrase}");
                ViewBag.Error = "Не удалось загрузить ваши курсы";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.Error = "Произошла ошибка при загрузке курсов";
            }

            return View(new List<Course>());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Сначала проверяем, записан ли уже пользователь
            var checkResponse = await client.GetAsync($"api/Users/{userId}/courses/{courseId}/isEnrolled");
            if (checkResponse.IsSuccessStatusCode && await checkResponse.Content.ReadFromJsonAsync<bool>())
            {
                TempData["Info"] = "Вы уже записаны на этот курс";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            var response = await client.PostAsync($"api/Users/{userId}/courses/{courseId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Вы успешно записаны на курс";
            }
            else
            {
                TempData["Error"] = "Ошибка при записи на курс";
            }

            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("MOOCApi");

            var response = await client.DeleteAsync($"api/Users/{userId}/courses/{courseId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Вы успешно отписались от курса";
            }
            else
            {
                TempData["Error"] = "Ошибка при отписке от курса";
            }

            return RedirectToAction("MyCourses");
        }
    }
}
