using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOOCSite.Models;
using MOOCSite.ViewModels;
using System;
using System.Net.Http.Headers;

namespace MOOCSite.Controllers
{
    [Route("admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:44345/";
        }

        #region Dashboard
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            // Жесткая проверка на "admin"
            if (User.Identity?.Name?.ToLower() != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }
        #endregion

        #region Courses CRUD
        [Route("courses")]
        public async Task<IActionResult> Courses()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync("api/Courses?includeUniversity=true&includeLecturers=true");

            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<List<Course>>();
                return View(courses ?? new List<Course>());
            }

            ViewBag.Error = "Ошибка при загрузке курсов";
            return View(new List<Course>());
        }

        [HttpGet("courses/create")]
        public async Task<IActionResult> CreateCourse()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Загрузка данных для выпадающих списков
            var universitiesTask = client.GetAsync("api/Universities");
            var disciplinesTask = client.GetAsync("api/Disciplines");
            var lecturersTask = client.GetAsync("api/Lecturers");

            await Task.WhenAll(universitiesTask, disciplinesTask, lecturersTask);

            var model = new CourseCreateViewModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1))
            };

            if (universitiesTask.Result.IsSuccessStatusCode)
                model.Universities = await universitiesTask.Result.Content.ReadFromJsonAsync<List<University>>();

            if (disciplinesTask.Result.IsSuccessStatusCode)
                model.Disciplines = await disciplinesTask.Result.Content.ReadFromJsonAsync<List<Discipline>>();

            if (lecturersTask.Result.IsSuccessStatusCode)
                model.Lecturers = await lecturersTask.Result.Content.ReadFromJsonAsync<List<Lecturer>>();

            return View(model);
        }

        [HttpPost("courses/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Перезагружаем списки при ошибке валидации
                await LoadSelectLists(model);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Создаем объект курса для отправки
            var course = new Course
            {
                Title = model.Title,
                Link = model.Link,
                Description = model.Description,
                IsSelfPassed = model.IsSelfPassed,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Price = model.Price,
                Language = model.Language,
                Certificated = model.Certificated,
                IsCertificatePaid = model.IsCertificatePaid,
                Credits = model.Credits,
                Reviews = model.Reviews,
                UniversityId = model.UniversityId
            };

            // Отправка курса
            var response = await client.PostAsJsonAsync("api/Courses", course);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Ошибка при создании курса");
                await LoadSelectLists(model);
                return View(model);
            }

            var createdCourse = await response.Content.ReadFromJsonAsync<Course>();

            // Добавляем дисциплины
            if (model.SelectedDisciplineIds.Any())
            {
                foreach (var disciplineId in model.SelectedDisciplineIds)
                {
                    await client.PostAsync($"api/Courses/{createdCourse.Id}/disciplines/{disciplineId}", null);
                }
            }

            // Добавляем лекторов
            if (model.SelectedLecturerIds.Any())
            {
                foreach (var lecturerId in model.SelectedLecturerIds)
                {
                    await client.PostAsync($"api/Courses/{createdCourse.Id}/lecturers/{lecturerId}", null);
                }
            }

            TempData["Success"] = "Курс успешно создан";
            return RedirectToAction("Courses");
        }

        private async Task LoadSelectLists(CourseCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");

            var universitiesTask = client.GetAsync("api/Universities");
            var disciplinesTask = client.GetAsync("api/Disciplines");
            var lecturersTask = client.GetAsync("api/Lecturers");

            await Task.WhenAll(universitiesTask, disciplinesTask, lecturersTask);

            if (universitiesTask.Result.IsSuccessStatusCode)
                model.Universities = await universitiesTask.Result.Content.ReadFromJsonAsync<List<University>>();

            if (disciplinesTask.Result.IsSuccessStatusCode)
                model.Disciplines = await disciplinesTask.Result.Content.ReadFromJsonAsync<List<Discipline>>();

            if (lecturersTask.Result.IsSuccessStatusCode)
                model.Lecturers = await lecturersTask.Result.Content.ReadFromJsonAsync<List<Lecturer>>();
        }

        [Route("courses/edit/{id}")]
        public async Task<IActionResult> EditCourse(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync($"api/Courses/{id}?includeUniversity=true&includeLecturers=true&includeDisciplines=true");

            if (response.IsSuccessStatusCode)
            {
                var course = await response.Content.ReadFromJsonAsync<Course>();
                await LoadSelectLists();
                return View(course);
            }

            return NotFound();
        }

        [HttpPost("courses/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course, int[] lecturerIds, int[] disciplineIds)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(course);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Обновляем курс
            var response = await client.PutAsJsonAsync($"api/Courses/{id}", course);

            if (response.IsSuccessStatusCode)
            {
                // Обновляем лекторов курса
                await UpdateCourseRelations(id, "lecturers", lecturerIds ?? Array.Empty<int>(), client);

                // Обновляем дисциплины курса
                await UpdateCourseRelations(id, "disciplines", disciplineIds ?? Array.Empty<int>(), client);

                TempData["Success"] = "Курс успешно обновлен";
                return RedirectToAction("Courses");
            }

            await LoadSelectLists();
            ViewBag.Error = "Ошибка при обновлении курса";
            return View(course);
        }

        private async Task UpdateCourseRelations(int courseId, string relationType, int[] newIds, HttpClient client)
        {
            // Получаем текущие связи
            var currentResponse = await client.GetAsync($"api/Courses/{courseId}/{relationType}");
            if (currentResponse.IsSuccessStatusCode)
            {
                var currentItems = await currentResponse.Content.ReadFromJsonAsync<List<dynamic>>();
                var currentIds = currentItems?.Select(i => (int)i.id).ToList() ?? new List<int>();

                // Удаляем связи, которых нет в новом списке
                foreach (var id in currentIds.Except(newIds))
                {
                    await client.DeleteAsync($"api/Courses/{courseId}/{relationType}/{id}");
                }

                // Добавляем новые связи
                foreach (var id in newIds.Except(currentIds))
                {
                    await client.PostAsync($"api/Courses/{courseId}/{relationType}/{id}", null);
                }
            }
        }

        [HttpPost("courses/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.DeleteAsync($"api/Courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Курс успешно удален";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении курса";
            }

            return RedirectToAction("Courses");
        }
        #endregion

        #region Universities CRUD
        [Route("universities")]
        public async Task<IActionResult> Universities()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync("api/Universities");

            if (response.IsSuccessStatusCode)
            {
                var universities = await response.Content.ReadFromJsonAsync<List<University>>();
                return View(universities ?? new List<University>());
            }

            ViewBag.Error = "Ошибка при загрузке университетов";
            return View(new List<University>());
        }

        [Route("universities/create")]
        [HttpGet]
        public IActionResult CreateUniversity()
        {
            return View(new University()); // Явная инициализация модели
        }

        [HttpPost("universities/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUniversity(University university)
        {
            if (!ModelState.IsValid)
            {
                return View(university);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.PostAsJsonAsync("api/Universities", university);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Университет успешно создан";
                return RedirectToAction("Universities"); // Исправленный редирект
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Ошибка при создании университета: {errorContent}");
            return View(university);
        }


        [Route("universities/edit/{id}")]
        public async Task<IActionResult> EditUniversity(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync($"api/Universities/{id}");

            if (response.IsSuccessStatusCode)
            {
                var university = await response.Content.ReadFromJsonAsync<University>();
                return View(university);
            }

            return NotFound();
        }

        [HttpPost("universities/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUniversity(int id, University university)
        {
            if (id != university.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(university);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.PutAsJsonAsync($"api/Universities/{id}", university);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Университет успешно обновлен";
                return RedirectToAction("Universities");
            }

            ViewBag.Error = "Ошибка при обновлении университета";
            return View(university);
        }

        [HttpPost("universities/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.DeleteAsync($"api/Universities/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Университет успешно удален";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении университета";
            }

            return RedirectToAction("Universities");
        }
        #endregion

        #region Lecturers CRUD
        [Route("lecturers")]
        public async Task<IActionResult> Lecturers()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync("api/Lecturers");

            if (response.IsSuccessStatusCode)
            {
                var lecturers = await response.Content.ReadFromJsonAsync<List<Lecturer>>();
                return View(lecturers ?? new List<Lecturer>());
            }

            ViewBag.Error = "Ошибка при загрузке лекторов";
            return View(new List<Lecturer>());
        }

        [Route("lecturers/create")]
        [HttpGet]
        public IActionResult CreateLecturer()
        {
            return View(new Lecturer()); // Явная инициализация
        }

        [HttpPost("lecturers/create")]
        [ValidateAntiForgeryToken] // Добавлен атрибут
        public async Task<IActionResult> CreateLecturer(Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                return View(lecturer);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.PostAsJsonAsync("api/Lecturers", lecturer);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Лектор успешно создан";
                return RedirectToAction("Lecturers"); // Исправленный редирект
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Ошибка при создании лектора: {errorContent}");
            return View(lecturer);
        }

        [Route("lecturers/edit/{id}")]
        public async Task<IActionResult> EditLecturer(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.GetAsync($"api/Lecturers/{id}");

            if (response.IsSuccessStatusCode)
            {
                var lecturer = await response.Content.ReadFromJsonAsync<Lecturer>();
                return View(lecturer);
            }

            return NotFound();
        }

        [HttpPost("lecturers/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int id, Lecturer lecturer)
        {
            if (id != lecturer.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(lecturer);
            }

            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.PutAsJsonAsync($"api/Lecturers/{id}", lecturer);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Лектор успешно обновлен";
                return RedirectToAction("Lecturers");
            }

            ViewBag.Error = "Ошибка при обновлении лектора";
            return View(lecturer);
        }

        [HttpPost("lecturers/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLecturer(int id)
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");
            var response = await client.DeleteAsync($"api/Lecturers/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Лектор успешно удален";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении лектора";
            }

            return RedirectToAction("Lecturers");
        }
        #endregion

        #region Helper Methods
        private async Task LoadSelectLists()
        {
            var client = _httpClientFactory.CreateClient("MOOCApi");

            // Загружаем университеты
            var universitiesResponse = await client.GetAsync("api/Universities");
            if (universitiesResponse.IsSuccessStatusCode)
            {
                ViewBag.Universities = await universitiesResponse.Content.ReadFromJsonAsync<List<University>>() ?? new List<University>();
            }
            else
            {
                ViewBag.Universities = new List<University>();
            }

            // Загружаем лекторов
            var lecturersResponse = await client.GetAsync("api/Lecturers");
            if (lecturersResponse.IsSuccessStatusCode)
            {
                ViewBag.Lecturers = await lecturersResponse.Content.ReadFromJsonAsync<List<Lecturer>>() ?? new List<Lecturer>();
            }
            else
            {
                ViewBag.Lecturers = new List<Lecturer>();
            }

            // Загружаем дисциплины
            var disciplinesResponse = await client.GetAsync("api/Disciplines");
            if (disciplinesResponse.IsSuccessStatusCode)
            {
                ViewBag.Disciplines = await disciplinesResponse.Content.ReadFromJsonAsync<List<Discipline>>() ?? new List<Discipline>();
            }
            else
            {
                ViewBag.Disciplines = new List<Discipline>();
            }
        }
        #endregion
    }
}