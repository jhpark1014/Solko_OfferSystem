using System;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;

using Microsoft.SharePoint;
using System.IO;
using OfficeOpenXml;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Collections.Generic;

public partial class OS_FileContent : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //makeTree();

        DataSet ds = Listup_PriceTable_SharePoint("OfferSystem", "SOLKO_SW_PriceBook.xlsx", 1);


        if (false)
        {

            Dictionary<string, string> dicName = new Dictionary<string, string>();
            {
                DataSet dsMap = Listup_PriceTable_SharePoint("OfferSystem", "SOLKO_SW_TRN_SVC_PriceBook__Sub_07FY22.xlsx", 2);
                for (int i = 0; i < CCommon.getDSCount(dsMap); i++)
                {
                    string nameCRM = CCommon.getDSValueAsString(dsMap, i, 0);
                    string nameXLS = CCommon.getDSValueAsString(dsMap, i, 1);
                    // string nameXLS2 = CCommon.getDSValueAsString(dsMap, i, "XLSNAME");
                    if (!dicName.ContainsKey(nameCRM))
                        dicName.Add(nameCRM, nameXLS);
                }
            }

            // print 
            foreach (KeyValuePair<string, string> adata in dicName)
            {
                Debug.WriteLine(string.Format("가격표 CRM이름={0}, XLS이름={1}", adata.Key, adata.Value));
            }


            // print 2 
            List<string> keyNames = new List<string>();
            foreach (KeyValuePair<string, string> adata in dicName)
                keyNames.Add(adata.Key);
            keyNames.Sort();
            for (int i = 0; i < keyNames.Count; i++)
            {
                string productName_CRM = keyNames[i];
                string productName_XLS = dicName[productName_CRM];
                Debug.WriteLine(string.Format("가격표 CRM이름={0}, XLS이름={1}", productName_CRM, productName_XLS));
            }
        }


        PriceGridView.DataSource = ds;
        PriceGridView.DataBind();

    }

    protected DataSet Listup_PriceTable_SharePoint(string siteName, string priceFilename, int readSheetIndex)
    {
        DataSet xlsDS = new DataSet();

        try
        {

            ClientContext ctx = new ClientContext("https://solidkorea.sharepoint.com/sites/" + siteName);
            SecureString passWord = new SecureString();
            foreach (char c in "Ghdwhdtjdakstp6997&&".ToCharArray()) passWord.AppendChar(c);
            ctx.Credentials = new SharePointOnlineCredentials("jshong@solidkorea.co.kr", passWord);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Web web = ctx.Web;
            Microsoft.SharePoint.Client.File sf = web.GetFileByServerRelativeUrl(string.Format("/sites/{0}/shared%20documents/{1}", siteName, priceFilename));
            ctx.Load(sf);
            ctx.ExecuteQuery();

            FileInformation fi = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, sf.ServerRelativeUrl);
            string filepath = Server.MapPath("~/temp/") + Guid.NewGuid().ToString() + ".xlsx";
            using (var fs = System.IO.File.Create(filepath))
            {
                fi.Stream.CopyTo(fs);
            }


            using (ExcelPackage package = new ExcelPackage(new FileInfo(filepath)))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets[readSheetIndex];   // 1번 시트만 읽음 

                int rows = sheet.Dimension.End.Row;
                int cols = sheet.Dimension.End.Column;
                DataTable dt = new DataTable(sheet.Name);

                for (int i = 1; i <= rows; i++)
                {
                    DataRow dr = null;

                    if (i > 1)
                    {
                        //dr = dt.Rows.Add();
                        dr = dt.NewRow();
                    }

                    for (int j = 1; j <= cols; j++)
                    {
                        if (i == 1)
                        {
                            string t = GetString(sheet.Cells[i, j].Value);
                            dt.Columns.Add(t);
                        }
                        else
                        {
                            string t = GetString(sheet.Cells[i, j].Value);
                            if (string.IsNullOrEmpty(t))
                                t = " ";

                            dr[j - 1] = t;
                        }
                    }

                    if (i > 1)
                        dt.Rows.Add(dr);

                }

                xlsDS.Tables.Add(dt);
            }

            return xlsDS;

            //MessageBoxShow(string.Format("excel table count = {0}", xlsDS.Tables.Count));
        }
        catch (Exception ex)
        {
            MessageBoxShow(ex.Message);
            return xlsDS;
        }
        finally 
        {
        }

    }

    protected static string GetString(object obj)
    {
        try
        {
            if (obj == null)
            {
                return "";
            }
            else
            {
                return obj.ToString();
            }
        }
        catch
        {
            return "";
        }
    }

    protected void MessageBoxShow(string msg)
    {

        //txtComment.Text = row.Cells[6].Text;
        ClientScriptManager sm = Page.ClientScript;
        string script = string.Format("<script language='javascript'>alert('{0}'); </script>", msg.Replace("\r\n", "\\n"));
        sm.RegisterStartupScript(this.GetType(), "sm", script);
    }

    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        PriceGridView.PageIndex = e.NewPageIndex;
        PriceGridView.DataBind();
    }

    protected void msgb(string MSG)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + MSG + "');", true);

    }
    protected void changeStartDate(object sender, EventArgs e)
    {

    }


    //#region " tree view " 

    //public void makeTree()
    //{
    //    if (TreeView1.Nodes.Count < 1)
    //    {

    //        DB DB = new DB();
    //        SqlConnection dbConn = DB.DbOpen();
    //        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM SK_LIB WHERE P_TYPE = '라이브러리' and P_NAME = 'LIB' order by DESCRIPTION ");
    //        while (dbReader.Read())
    //        {
    //            TreeNode rootNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("TYPE")) + " " + dbReader.GetString(dbReader.GetOrdinal("DESCRIPTION")), dbReader["CODE_NO"].ToString());
    //            rootNode.ChildNodes.Add(addTreeChildNode(dbReader.GetString(dbReader.GetOrdinal("TYPE")), dbReader.GetString(dbReader.GetOrdinal("NAME")), rootNode));
    //            TreeView1.Nodes.Add(rootNode);
    //        }
    //        DB.DbClose(dbConn);
    //    }
    //}

    ////public TreeNode addTreeRootNode(int porjectId, TreeNode nodeFolder)
    ////{
    ////    DB DB = new DB();
    ////    SqlConnection dbConn = DB.DbOpen();
    ////    SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_Folders where Root=1 and ProjectId = " + porjectId);
    ////    while (dbReader.Read())
    ////    {
    ////        TreeNode subNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("FolderName")), dbReader["CurrentFolderId"].ToString() + "," + dbReader["Id"].ToString());
    ////        subNode = addTreeChildNode(dbReader.GetInt32(dbReader.GetOrdinal("Id")), subNode);
    ////        nodeFolder.ChildNodes.Add(subNode);
    ////    }
    ////    DB.DbClose(dbConn);
    ////    return nodeFolder;
    ////}

    //public TreeNode addTreeChildNode(string parentType, string parentName, TreeNode nodeFolder)
    //{
    //    DB DB = new DB();
    //    SqlConnection dbConn = DB.DbOpen();
    //    SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM SK_LIB where P_TYPE = '" + parentType + "' and P_NAME = '" + parentName + "' ORDER BY ORD");
    //    while (dbReader.Read())
    //    {
    //        TreeNode subNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("TYPE")) + " " + dbReader.GetString(dbReader.GetOrdinal("DESCRIPTION")), dbReader["CODE_NO"].ToString());
    //        subNode = addTreeChildNode(dbReader.GetString(dbReader.GetOrdinal("TYPE")), dbReader.GetString(dbReader.GetOrdinal("NAME")), subNode);
    //        nodeFolder.ChildNodes.Add(subNode);
    //    }
    //    DB.DbClose(dbConn);
    //    return nodeFolder;
    //}
    //protected void TreeViewProject_SelectedNodeChanged(object sender, EventArgs e)
    //{
    //    //TreeChange();
    //}

    //#endregion

}

