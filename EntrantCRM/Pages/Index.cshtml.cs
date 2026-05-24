using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntrantCRM.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var username = HttpContext.Session.GetString("login");

        if (username != null) { 
            var password = HttpContext.Session.GetString("password");
            var link = HttpContext.Session.GetString("link");
            
            _logger.LogInformation(username, password, link);
        }
    }
}
