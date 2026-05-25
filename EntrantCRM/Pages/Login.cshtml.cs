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

        string serviceName = link.Replace("/", "").Replace("\\", "").Trim();
        string tnsConnectionString = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORA)))";
        string finalConnectionString = $"User Id=Scott;Password=tiger1;Data Source={tnsConnectionString};";


        ria.UserID = login;  // Присвоюємо введенний ID користувача з textBox1
        if (login.ToLower() == "sys")
        {
            finalConnectionString += "DBA Privilege=SYSDBA;";
        }

        ria.Password = password;   // Присвоюємо введенний пароль з textBox2
        ria.PersistSecurityInfo = true;  // Дозволяємо зберігати інформацію про безпеку

        Oracle.ManagedDataAccess.Client.OracleConnection con = new Oracle.ManagedDataAccess.Client.OracleConnection(finalConnectionString);
        con.ConnectionString = ria.ConnectionString; // Встановлюємо рядок підключення
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
