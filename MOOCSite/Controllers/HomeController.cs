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

        public async Task<IActionResult> Index(
    string search = "",
    string sortBy = "title",
    string sortOrder = "asc",
    bool? isSelfPassed = null,
    bool? certificated = null,
    string language = null,
    int? minPrice = null,
    int? maxPrice = null,
    float? minRating = null,
    int? disciplineId = null)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Получаем список всех дисциплин
            var disciplinesResponse = await client.GetAsync("api/Disciplines");
            ViewBag.Disciplines = disciplinesResponse.IsSuccessStatusCode
                ? await disciplinesResponse.Content.ReadFromJsonAsync<List<Discipline>>()
                : new List<Discipline>();

            // Строим URL с параметрами
            var url = $"api/Courses/Filter?search={search}&sortBy={sortBy}&sortOrder={sortOrder}";

            if (isSelfPassed.HasValue) url += $"&isSelfPassed={isSelfPassed}";
            if (certificated.HasValue) url += $"&certificated={certificated}";
            if (!string.IsNullOrEmpty(language)) url += $"&language={language}";
            if (minPrice.HasValue) url += $"&minPrice={minPrice}";
            if (maxPrice.HasValue) url += $"&maxPrice={maxPrice}";
            if (minRating.HasValue) url += $"&minRating={minRating}";
            if (disciplineId.HasValue) url += $"&disciplineId={disciplineId}";

            // Получаем отфильтрованные курсы
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<List<Course>>();
                var viewModels = new List<CourseWithEnrollmentViewModel>();

                // Для каждого курса загружаем дисциплины (если они не были загружены)
                foreach (var course in courses)
                {
                    if (course.Disciplines == null || !course.Disciplines.Any())
                    {
                        var disciplinesResponseForCourse = await client.GetAsync($"api/Courses/{course.Id}/disciplines");
                        if (disciplinesResponseForCourse.IsSuccessStatusCode)
                        {
                            course.Disciplines = await disciplinesResponseForCourse.Content.ReadFromJsonAsync<List<Discipline>>();
                        }
                    }

                    // Проверяем запись пользователя на курс
                    bool isEnrolled = false;
                    if (User.Identity.IsAuthenticated)
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var checkResponse = await client.GetAsync($"api/Users/{userId}/courses/{course.Id}/isEnrolled");
                        if (checkResponse.IsSuccessStatusCode)
                        {
                            isEnrolled = await checkResponse.Content.ReadFromJsonAsync<bool>();
                        }
                    }

                    viewModels.Add(new CourseWithEnrollmentViewModel
                    {
                        Course = course,
                        IsEnrolled = isEnrolled
                    });
                }

                // Сохраняем параметры фильтрации
                ViewBag.CurrentFilter = new
                {
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    IsSelfPassed = isSelfPassed,
                    Certificated = certificated,
                    Language = language,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinRating = minRating,
                    DisciplineId = disciplineId
                };

                return View(viewModels);
            }

            return View(new List<CourseWithEnrollmentViewModel>());
        }
    }
}
