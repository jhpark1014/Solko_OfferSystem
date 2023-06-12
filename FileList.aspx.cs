using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class FileList : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_FileInTree ORDER BY ProjectName, depth_fullname, FileFullName");
        GridView1.DataSource = dbReader;
        GridView1.DataBind();
        dbReader.Close();
        dbConn.Close();
    }
    protected void btnUpload_Click(object sender, EventArgs e)
    {

    }

    protected void ListView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
    protected void SqlDataSource1_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
    {

    }
    protected void DeleteMasterFile(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        int id = Int32.Parse(arg[0]);
        if (id > 0)
        {
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlDataReader dbReader = DB.QueryReader(dbConn, "EXEC DeleteFile " + id);
            while (dbReader.Read())
            {
                string uploadedName = ConfigurationManager.AppSettings["uploadLocation"].ToString() + @"\" + dbReader["SaveName"].ToString();
                FileInfo fileDelete = new FileInfo(uploadedName);

                if (fileDelete.Exists)
                {
                    fileDelete.Delete();
                }
            }
            dbReader.Close();
            dbConn.Close();
            Response.Redirect("/FileList");
        }
    }
    protected void DownloadFile(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        int id = Int32.Parse(arg[0]);
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_FileInTree WHERE MasterID=" + id);
        while (dbReader.Read())
        {
            string uploadedName = ConfigurationManager.AppSettings["uploadLocation"].ToString() + dbReader["SaveName"].ToString();
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/plain";
            response.AddHeader("Content-Disposition", "attachment; filename=" + dbReader["FileFullName"].ToString() + ";");
            response.TransmitFile(uploadedName);
            response.Flush();
            response.End();
        }
    }

}