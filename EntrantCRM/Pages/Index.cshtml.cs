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
    public string viewname;


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
        this.viewname = viewname; // info for actions

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


    public IActionResult OnPost(string tableName) {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        string connectionString = GetConnectionString(login, password, link)

        // adding new string
        var columnsToInsert = new List<string>();
        var parameterNames = new List<string>();
        var valuesDictionary = new Dictionary<string, string>();

        foreach (var key in Request.Form.Keys)
        {
            if (key == "__RequestVerificationToken" || key == "tableName") continue;

            columnsToInsert.Add(key);
            parameterNames.Add($":{key}");
            valuesDictionary.Add(key, Request.Form[key]);
        }

        if (columnsToInsert.Count == 0)
        {
            _logger.LogInformation("There is nothing to add!");
            return Page();
        }

        string sql = $"INSERT INTO {tableName} ({string.Join(", ", columnsToInsert)}) VALUES ({string.Join(", ", parameterNames)})";

        try
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    // defend from SQL injections
                    foreach (var kvp in valuesDictionary)
                    {
                        cmd.Parameters.Add(new OracleParameter(kvp.Key, kvp.Value));
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error when adding new line to {tableName}: " + ex.Message);
            return new BadRequestResult();
        }

        return RedirectToPage();
    }
}
