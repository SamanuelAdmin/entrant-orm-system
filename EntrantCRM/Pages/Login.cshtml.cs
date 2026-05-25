using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;

namespace EntrantCRM.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string login, string password, string link)
    {
        HttpContext.Session.SetString("login", login);
        HttpContext.Session.SetString("password", password);
        HttpContext.Session.SetString("link", link);

        Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder ria = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder(); // Створюємо підключення

        string serviceName = link.Replace("/", "").Replace("\\", "").Trim();
        string tnsConnectionString = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=serveroracle)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={link})))";
        // string finalConnectionString = $"User Id=Scott;Password=tiger1;Data Source={tnsConnectionString};";


        ria.UserID = login; 
        ria.Password = password;  
        ria.DataSource = tnsConnectionString;
        ria.PersistSecurityInfo = true;

        Oracle.ManagedDataAccess.Client.OracleConnection con = new Oracle.ManagedDataAccess.Client.OracleConnection();
        con.ConnectionString = ria.ConnectionString;


        try 
        {
            con.Open(); 
            return RedirectToPage("/Index");
        } catch {
          _logger.LogError("Cannot connect to database!");
          return new BadRequestResult();
        }
    }
}
