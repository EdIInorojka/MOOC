using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOOCSite.ViewModels;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using MOOCSite.Models;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var client = _httpClientFactory.CreateClient("MOOCApi");
        var response = await client.GetAsync($"api/Users/{User.FindFirstValue(ClaimTypes.NameIdentifier)}");

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<User>();
            return View(user);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient("MOOCApi");
        var response = await client.PostAsJsonAsync("api/Users/Authenticate", model);

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<User>();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
        return View(model);
    }
    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient("MOOCApi");
        var response = await client.PostAsJsonAsync("api/Users", model);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Login");
        }

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, error);
        return View(model);
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var client = _httpClientFactory.CreateClient("MOOCApi");
        var response = await client.GetAsync($"api/Users/{User.FindFirstValue(ClaimTypes.NameIdentifier)}");

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<User>();
            var model = new EditProfileViewModel
            {
                Id = user.Id,
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        return RedirectToAction("Profile");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("MOOCApi");
        var response = await client.PutAsJsonAsync($"api/Users/{model.Id}", model);

        if (response.IsSuccessStatusCode)
        {
            // Обновляем имя пользователя в куках, если изменился логин
            if (User.Identity.Name != model.Login)
            {
                var identity = User.Identity as ClaimsIdentity;
                var claim = identity.FindFirst(ClaimTypes.Name);
                identity.RemoveClaim(claim);
                identity.AddClaim(new Claim(ClaimTypes.Name, model.Login));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            }

            TempData["Success"] = "Профиль успешно обновлен";
            return RedirectToAction("Profile");
        }

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, error);
        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var client = _httpClientFactory.CreateClient("MOOCApi");

        var response = await client.PostAsJsonAsync($"api/Users/{userId}/password", new
        {
            model.CurrentPassword,
            model.NewPassword
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Пароль успешно изменён";
            return RedirectToAction("Profile");
        }

        ModelState.AddModelError("", "Ошибка при смене пароля");
        return View(model);
    }


}