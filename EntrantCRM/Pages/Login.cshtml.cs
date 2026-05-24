using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntrantCRM.Pages;

public class LoginModel : PageModel
{

    public void OnGet()
    {

    }

    public IActionResult OnPost(string login, string password, string link)
    {
        HttpContext.Session.SetString("login", login);
        HttpContext.Session.SetString("password", password);
        HttpContext.Session.SetString("link", link);

        // TODO: Check the creds before adding

        return RedirectToPage("/Index");
    }
}
