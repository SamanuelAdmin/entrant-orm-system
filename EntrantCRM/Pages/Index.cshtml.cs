using RazorEngineCore;
using System.Text;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;


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


    public string GetTableNameByView(string connectionString, string viewName) {
        string findTableSql = @"
            SELECT referenced_name 
            FROM user_dependencies 
            WHERE name = UPPER(:viewName) 
            AND referenced_type = 'TABLE'
            AND rownum = 1"; // берем первую таблицу, если во View их несколько

        string tableName;

        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            using (OracleCommand cmd = new OracleCommand(findTableSql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("viewName", viewName));
                
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    tableName = result.ToString();
                }
                else
                {
                    tableName = viewName; 
                }
            }
        }

        return tableName;
    }


    public void ExecSQL(string connectionString, string sql) {
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                
                // defend from SQL injections
                // foreach (var kvp in valuesDictionary)
                // {
                //     cmd.Parameters.Add(new OracleParameter(kvp.Key, kvp.Value));
                // }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }



    public IActionResult OnGet(string viewname,  string title)
    {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        string connectionString = GetConnectionString(login, password, link);

        if (viewname== null) {
            viewname = "entrant_view"; // default view
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

    

    public IActionResult OnGetReport(string viewname)
    {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        string connectionString = GetConnectionString(login, password, link);

        if (viewname== null) {
            return new BadRequestResult();
        }
        

        string tableName;
        try 
        {
            tableName = GetTableNameByView(connectionString, viewname);
            this.table = GetViewData(connectionString, viewname);
        } catch {
          _logger.LogError("Database error");
          return new BadRequestResult();
        }

        string templateSource = System.IO.File.ReadAllText("wwwroot/ReportTemplate.html");

        IRazorEngine razorEngine = new RazorEngine();
        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<DataTable>> template = 
            razorEngine.Compile<RazorEngineTemplateBase<DataTable>>(templateSource);

        this.table.TableName = tableName;
        string htmlResult = template.Run(instance =>
        {
            instance.Model = this.table;
        });

        System.IO.File.WriteAllText(Path.Combine("wwwroot", "Reports", $"{viewname}.html"), htmlResult, Encoding.UTF8);


        return Page();
    }


    public IActionResult OnPost() {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");
        
        string connectionString = GetConnectionString(login, password, link);

        // adding new string
        var columnsToInsert = new List<string>();
        var parameterNames = new List<string>();
        var valuesDictionary = new Dictionary<string, string>();

        foreach (var key in Request.Form.Keys)
        {
            if (key == "__RequestVerificationToken" || key == "viewname") continue;

            columnsToInsert.Add(key);
            parameterNames.Add($"'{Request.Form[key]}'");
            valuesDictionary.Add(key, Request.Form[key]);
        }

        if (columnsToInsert.Count == 0)
        {
            _logger.LogInformation("There is nothing to add!");
            return Page();
        }

        string tableName;
        try {
            tableName = GetTableNameByView(connectionString, Request.Form["viewname"]);
        } catch (Exception ex) {
            _logger.LogInformation($"Cannot get table name by view name. {ex.Message}");
            return new BadRequestResult();
        }

        string sql = $"INSERT INTO {tableName} ({string.Join(", ", columnsToInsert)}) VALUES ({string.Join(", ", parameterNames)})";
        _logger.LogInformation(sql);

        try
        {
            ExecSQL(connectionString, sql);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error when adding new line to {tableName}: " + ex.Message);
            return new BadRequestResult();
        }

        return RedirectToPage();
    }


    public IActionResult OnPostDelete()
    {
        var login = HttpContext.Session.GetString("login");
        if (login == null) { 
            return RedirectToPage("/Login");
        }

        var password = HttpContext.Session.GetString("password");
        var link = HttpContext.Session.GetString("link");

        string connectionString = GetConnectionString(login, password, link);

        string tableName;
        try {
            tableName = GetTableNameByView(connectionString, Request.Form["viewname"]);
        } catch (Exception ex) {
            _logger.LogInformation($"Cannot get table name by view name. {ex.Message}");
            return new BadRequestResult();
        }
        string rowId = Request.Form["rowid"];

        string sql = $"DELETE FROM \"{tableName}\" WHERE PK = {rowId}";
        _logger.LogInformation(sql); 


        try
        {
            ExecSQL(connectionString, sql);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error when removind {rowId} from {tableName}: " + ex.Message);
            return new BadRequestResult();
        }
        

        return RedirectToPage("/Index");
    }
}
