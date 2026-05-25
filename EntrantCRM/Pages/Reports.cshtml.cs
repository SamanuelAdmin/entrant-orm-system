using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntrantCRM.Pages;

public class ReportsModel : PageModel
{
    private readonly ILogger<ReportsModel> _logger;

    public ReportsModel(ILogger<ReportsModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var username = HttpContext.Session.GetString("login");
        if (username == null) { 
            return RedirectToPage("/Login");
        }

        

        return Page();
        
    }
}
