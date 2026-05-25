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


    public Oracle.ManagedDataAccess.Client.OracleConnection GetConnection(string login, string password, string link) {
        string tnsConnectionString = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=serveroracle)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={link})))";

        Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder ria = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder();
        ria.UserID = login; 
        ria.Password = password;  
        ria.DataSource = tnsConnectionString;
        ria.PersistSecurityInfo = true;

        Oracle.ManagedDataAccess.Client.OracleConnection con = new Oracle.ManagedDataAccess.Client.OracleConnection();
        con.ConnectionString = ria.ConnectionString;
        
        con.Open(); 
        return con;
    }


    public IActionResult OnGet()
    {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        try 
        {
            Oracle.ManagedDataAccess.Client.OracleConnection con = GetConnection(login, password, link)
            con.Open(); 
        } catch {
          _logger.LogError("Cannot connect to database!");
          return RedirectToPage("/Login");
        }
        

        string sql = "SELECT * FROM comp_educ_inst_view "; 

        using (var cmd = new OracleCommand(sql, con))
        {
            con.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    names.Add(reader.GetString(0));
                }
            }
        }
        
        con.Close(); 


        return Page();
        
    }
}
