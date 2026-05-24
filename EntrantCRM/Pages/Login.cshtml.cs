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

        // TODO: Check the creds before adding
        Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder ria = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder(); // Створюємо підключення
        var builder = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder();
        
        string serviceName = link.Replace("/", "").Replace("\\", "").Trim();
        string tnsConnectionString = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=)))";
        
        builder.UserID = login;
        builder.Password = password;
        builder.DataSource = tnsConnectionString; 
        builder.PersistSecurityInfo = true;
        builder.Pooling = false;       // Отключаем пул, чтобы соединение создавалось честно
        builder.ConnectionTimeout = 5;
        
        if (builder.UserID.ToLower() == "sys")
        {
            builder.DBAPrivilege = "SYSDBA";
        }

        string completedConnString = builder.ConnectionString;
        _logger.LogInformation(completedConnString);

        using (var con = new Oracle.ManagedDataAccess.Client.OracleConnection("USER ID=sys;PASSWORD=sys;DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xe)))"))
        {
          //try  // Спроба під'єднання
          //{
              con.Open();  // Відкриваємо з'єднання з базою даних
              return RedirectToPage("/Index");
          //} catch {
          //  _logger.LogError("Cannot connect to database!");
          //  return new BadRequestResult();
          //}
        }
    }
}
