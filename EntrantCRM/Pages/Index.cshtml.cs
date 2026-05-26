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

    public DataTable table;
    public string title;


    public string GetConnectionString(string login, string password, string link) {
        string tnsConnectionString = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=serveroracle)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={link})))";

        Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder ria = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder();
        ria.UserID = login; 
        ria.Password = password;  
        ria.DataSource = tnsConnectionString;
        ria.PersistSecurityInfo = true;
 
        return ria.ConnectionString;
    }

    public DataTable GetViewData(string connectionString, string viewName)
    {
        string query = $"SELECT * FROM {viewName}"; 

//        Oracle.ManagedDataAccess.Client.OracleConnection con = new Oracle.ManagedDataAccess.Client.OracleConnection();
//        con.ConnectionString = ria.ConnectionString;
//        con.Open(); 

        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                {
                    DataTable dataTable = new DataTable();
                    conn.Open();
                    adapter.Fill(dataTable); 
                    
                    return dataTable;
                }
            }
        }
    }


    public IActionResult OnGet(string viewname, string title)
    {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        string connectionString = GetConnectionString(login, password, link)

        if (viewname== null) {
            viewname = "entrant_view" // default view
        }
        if (title == null) {
            this.title = viewname.Replace("_view", "");
        } else {
            this.title = title;
        }
        
        try 
        {
            this.table = GetViewData(connectionString, viewname);
        } catch {
          _logger.LogError("Database error");
          return new BadRequestResult();
        }

        return Page();
        
    }
}
