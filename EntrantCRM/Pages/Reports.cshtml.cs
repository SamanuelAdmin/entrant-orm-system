using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntrantCRM.Pages;

public class ReportsModel : PageModel
{
    public class RecordEnt {
      public string Name {get; set;}
      public string Path {get; set;}
      public DateTime ChangeTime {get;set;}

      public RecordEnt(string name, string path, DateTime changetime) {
        this.Name = name;
        this.Path = path;
        this.ChangeTime = changetime;
      }
    }

    private readonly ILogger<ReportsModel> _logger;
    public List<RecordEnt> reports_list = new List<RecordEnt>();

    public ReportsModel(ILogger<ReportsModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet(string report_name)
    {
        var username = HttpContext.Session.GetString("login");
//        if (username == null) { 
//            return RedirectToPage("/Login");
//        }
//
        if (report_name != null) {
          return File($"/Reports/{Path.GetFileName(report_name)}", "text/html");
        }

        string[] report_names = Directory.GetFiles(Path.Combine("wwwroot", "Reports")); 

        foreach (string fullpath in report_names) {
          reports_list.Add(
            new RecordEnt(
                Path.GetFileName(fullpath),
                fullpath,
                System.IO.File.GetLastWriteTime(fullpath)
              )
          );
        }

        return Page();
        
    }
}
