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
using System.Text;
using System.Collections.Generic;
using WinForms = System.Windows.Forms;
using WebControls = System.Web.UI.WebControls;
using TVPDemo;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

public partial class RequestList : System.Web.UI.Page
{
    //[STAThreadAttribute]
    private DataTable dt;
    static string _pageFilterKey = "Usage_FilterText";  // 페이지의 필터링데이터 키값
    static string _pageSortedKey = "Usage_SortedView";  // 페이지의 정렬데이터 키값

    // CRM using 
    Microsoft.Xrm.Sdk.IOrganizationService os = null;
    CrmServiceClient crmSvc = null;


    protected void Page_Load(object sender, EventArgs e)
    {
        string id_list = "";

        //if (Session["GridData"] != null)
        //{
        //    Debug.WriteLine("요기야");
        //    dt = (DataTable) GetValue_Session("GridData");
        //    GridView3.DataSource = dt;
        //    GridView3.DataBind();
        //}

        if (!Page.IsPostBack)
        {
            if (IsTransferedOtherPage("FROMMAIN", "T"))
            {
                Debug.WriteLine("요기야");
                //DataRow dr = dt.NewRow();

                //GridView GridView1 = (GridView)this.Page.PreviousPage.FindControl("GridView2");
                dt = (DataTable) Session["GridData"];
                Debug.WriteLine("**** Count: " + dt.Rows.Count);
                //for (int i = 0; i < dt.Columns.Count; i++)
                //{
                //    Debug.WriteLine(dt.Columns[i].ColumnName);
                //}

                //for (int r = 0; r < dt.Rows.Count; r++)
                //    for (int i = 0; i < dt.Columns.Count; i++)
                //    {
                //        if (dt.Columns[i].ColumnName == "제품명")
                //            Debug.WriteLine(dt.Rows[r][i]);
                //        if (dt.Columns[i].ColumnName == "사업장 위치")
                //            Debug.WriteLine(dt.Rows[r][i]);
                //    }

                companyName.Text = Session["companyName"].ToString();
                forwardDiscount.Text = Session["forwardDiscountRate"].ToString();
                backDiscount.Text = Session["backDiscountRate"].ToString();

                dt.Columns.Add("number");
                dt.Columns.Add("finalPrice");
                int no = 1;

                for (int i = 0; i < dt.Rows.Count / 3; i++)
                {
                    dt.Rows[i*3]["number"] = no;
                    no++;
                }

                //newRow["수량"] = qty;
                //newRow["customerRRP"] = price;
                //newRow["proposalRRP"] = totalPrice;

                //dt.Rows.Add(newRow);

                GridView3.DataSource = dt;
                GridView3.DataBind();
                //foreach (GridViewRow row in GridView3.Rows)
                //{
                //    WebControls.TextBox txtbox1 = (WebControls.TextBox)row.FindControl("changeForwardDiscount");
                //    WebControls.TextBox txtbox2 = (WebControls.TextBox)row.FindControl("changeBackDiscount");
                //    txtbox1.Text = forwardDiscount.Text;
                //    txtbox2.Text = backDiscount.Text;
                //}

                // rowspan
                for (int i = 0; i < GridView3.Rows.Count - 3; i++)
                {
                    GridViewRow rowss = GridView3.Rows[i];
                    //Debug.WriteLine("왜" + rowss.Cells[1].Text + GridView3.Rows[i + 1].Cells[1].Text + GridView3.Rows[i + 2].Cells[1].Text);
                    for (int j = 0; j < 7; j++)
                    {
                        rowss.Cells[j].RowSpan = 3;
                        GridView3.Rows[i + 2].Cells[j].Visible = false;
                        GridView3.Rows[i + 1].Cells[j].Visible = false;
                    }
                    i += 2;
                }

                //GridViewRow newrow = GridView3.
                //DataRow totalrow = dt.NewRow();
                //totalrow.Cell
            }
        }

        //dt = (DataTable)Session["GridData"];

        //System.Diagnostics.Debug.WriteLine(string.Format("Page_Load().. in OS_Calculate.aspx.. at {0}", DateTime.Now));

        //SqlCommand sqlCmd = CreateSqlCommand(id_list);
        //BindGridView1(id_list);
        //BindGridView_CRM(id_list);
        //GridView3.DataSource = dt;
        //GridView3.DataBind();

    }

    //protected void BindGridView(string id_list)
    //{
    //    string sqlALL = "SELECT * FROM ProductInfo ORDER BY CustomerName;";
    //    List<SqlParameter> paramList = new List<SqlParameter>();

    //    if (id_list.Length > 0)
    //    {
    //        string sql1 = "SELECT * FROM ProductInfo ";
    //        string sql2 = " WHERE ";
    //        string sql3 = " ORDER BY CustomerName;";

    //        // 조건절 생성
    //        StringBuilder sb = new StringBuilder();
    //        sb.AppendLine(" SELECT *  FROM ProductInfo  						 ");
    //        //sb.AppendLine("  WHERE ( @ID = '' OR ID in @ID) 	 ");
    //        sb.AppendLine("  ORDER BY CustomerName;								 ");
    //        sqlALL = sb.ToString();

    //        //paramList.Add(new SqlParameter("@ID", id_list));

    //        sqlALL = sql1 + sql2 + sql3;
    //    }

    //    DB DB = new DB();
    //    SqlConnection dbConn = DB.DbOpen();
    //    SqlCommand sqlCmd = new SqlCommand(sqlALL, dbConn);
    //    //sqlCmd.Parameters.Add(paramList);
    //    SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
    //    DataTable tempDT = new DataTable();
    //    da.Fill(tempDT);
    //    dt = tempDT;
    //    dbConn.Close();

    //    dt.Columns.Add("Backdating");
    //    foreach (DataRow row in dt.Rows)
    //    {
    //        Object end = row["EndDate"];
    //        row["Backdating"] = "";
    //    }

    //    dt.Columns.Add("Forward");
    //    foreach (DataRow row in dt.Rows)
    //    {
    //        row["Forward"] = "12";
    //    }
    //}

    //protected void BindGridView1(string id_list)
    //{
    //    DB DB = new DB();
    //    //SqlConnection dbConn = DemoHelper.setup_connection();
    //    SqlConnection dbConn = DB.DbOpen();
    //    SqlCommand sqlCmd = dbConn.CreateCommand();

    //    if (id_list.Length != 0)
    //    {
    //        sqlCmd.CommandType = CommandType.StoredProcedure;
    //        sqlCmd.CommandText = "dbo.get_selected_data";
    //        sqlCmd.Parameters.Add("@myidlist", SqlDbType.Structured);
    //        sqlCmd.Parameters["@myidlist"].Direction = ParameterDirection.Input;
    //        sqlCmd.Parameters["@myidlist"].TypeName = "dbo.customized";

    //        sqlCmd.Parameters["@myidlist"].Value = new CSV_splitter(id_list, ' ');
    //        System.Diagnostics.Debug.WriteLine("id_list: "+id_list);
    //        System.Diagnostics.Debug.WriteLine("여기");
    //        System.Diagnostics.Debug.WriteLine("parameter 보기: " + sqlCmd.Parameters["@myidlist"].Value);
    //    }
    //    else
    //    {
    //        string sqlALL = "SELECT * FROM ProductInfo ORDER BY CustomerName;";
    //        sqlCmd = new SqlCommand(sqlALL, dbConn);
    //        //sqlCmd.Parameters["@myidlist"].Value = null;
    //        System.Diagnostics.Debug.WriteLine("id_list: " + id_list);
    //        System.Diagnostics.Debug.WriteLine("null");
    //    }

    //    SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
    //    //DataSet ds = new DataSet();
    //    //da.Fill(ds);
    //    //dt = ds;
    //    //DemoHelper.PrintDataSet(ds);
    //    DataTable tempDT = new DataTable();
    //    da.Fill(tempDT);
    //    dt = tempDT;
    //    dbConn.Close();

    //    dt.Columns.Add("BackDate");
    //    foreach (DataRow row in dt.Rows)
    //    {
    //        //String endDate = ((string) row["ExpiredDate"]) + "/31";
    //        //row["Backdating"] = CalcBackDate(DateTime.ParseExact(endDate, "yyyy/MM/dd", null));
    //        //row["Backdating"] = "";

    //        String[] datesplit = (row["ExpiredDate"].ToString()).Split('-');

    //        try
    //        {
    //            string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
    //            DateTime endDate = DateTime.ParseExact(date, "yyyy/M/dd", null);
    //            row["BackDate"] = CalcBackDate(endDate);
    //        }
    //        catch
    //        {
    //            System.Diagnostics.Debug.WriteLine("backdate exception: " + row["ExpiredDate"].ToString());
    //            System.Diagnostics.Debug.WriteLine("backdate exception: " + row["CustomerName"].ToString());
    //            row["BackDate"] = "";
    //        }

    //    }

    //    dt.Columns.Add("Forward");
    //    foreach (DataRow row in dt.Rows)
    //    {

    //        String[] datesplit = (row["ExpiredDate"].ToString()).Split('-');

    //        try
    //        {
    //            string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
    //            DateTime endDate = DateTime.ParseExact(date, "yyyy/M/dd", null);
    //            row["Forward"] = CalcForwardDate(endDate);
    //        }
    //        catch
    //        {
    //            //System.Diagnostics.Debug.WriteLine("exception: " + row["ExpiredDate"].ToString());
    //            //System.Diagnostics.Debug.WriteLine("exception: " + row["CustomerName"].ToString());
    //            row["Forward"] = "";
    //        }

    //    }

    //    dt.Columns.Add("기간요약");
    //    foreach (DataRow row in dt.Rows)
    //    {
    //        String[] datesplit = (row["ExpiredDate"].ToString()).Split(' ');
    //        string forwarddata = "Forward (" + DateTime.Now.Year + "." + DateTime.Now.Month + ".1 ~ " + datesplit[0] + " = " + row["Forward"];
    //        string backdata = "BackDate (" + datesplit[0] + " ~ " + DateTime.Now.Year + "." + DateTime.Now.Month + "." +
    //                        DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) +  " = " + row["BackDate"];

    //        string final = forwarddata + "<br/>" + backdata;

    //        row["기간요약"] = final;
    //    }

    //    dt.Columns.Add("단가", typeof(int));
    //    foreach (GridViewRow row in GridView2.Rows)
    //    {
    //        WebControls.Label val = (WebControls.Label) row.FindControl("ProductType");
    //        row.Cells[8].Text = string.Format("{0}", 0);
    //        //row.Cells[8].Text = 0;
    //    }
    //}

    /*
    protected void BindGridView_CRM(string id_list)
    {
        QueryDataFilter2("new_customer_products", new ColumnSet(true), new List<string>() { "new_l_account", "new_l_products", "new_p_status", "new_l_product_category" }, id_list);
    }
    */

    #region "CRM API"

    protected void QueryDataFilter2(string entityName, ColumnSet columnSet, List<string> relAttrNames, string id_list)
    {
        //DataSet ds = new DataSet();

        if (connected(crmSvc))
        {
            //DataTable dt = ds.Tables.Add();
            if (dt == null)
                dt = new DataTable();

            dt.Rows.Clear();
            dt.Columns.Clear();

            string sname = entityName;

            // query entity data
            List<string> idList = new List<string>(id_list.Split(CCommon.splitSpace, StringSplitOptions.RemoveEmptyEntries));

            //if (!string.IsNullOrEmpty(startFilter))
            //{
            //    startFilter = startFilter + "/01";
            //    DateTime stDate = DateTime.ParseExact(startFilter, "yyyy/MM/dd", null);
            //    //System.Diagnostics.Debug.WriteLine("startdate" + stDate);
            //}
            //if (!string.IsNullOrEmpty(finFilter))
            //{
            //    finFilter = finFilter + "/31";
            //    DateTime finDate = DateTime.ParseExact(finFilter, "yyyy/MM/dd", null);
            //    //System.Diagnostics.Debug.WriteLine("finishdate" + finDate);
            //}


            QueryExpression query = new QueryExpression(entityName);
            query.ColumnSet = columnSet;

            // filter link condition style 1.
            //LinkEntity customerRef = query.AddLink("account", "new_l_account", "accountid");
            //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "필옵틱스");

            // filter by id(primary key)
            if ( idList.Count > 0 )
            {
                query.Criteria.AddCondition("new_customer_productsid", ConditionOperator.In, idList.ToArray());
            }

            // filter condition setup example
            //query.Criteria = new FilterExpression();
            //query.Criteria.AddCondition(new ConditionExpression("new_l_account", ConditionOperator.Equal, "c13d6c87-73c3-eb11-bacc-00224816b187"));

            int pageNumber = 1;
            string pageCookie = string.Empty;
            EntityCollection ec;
            List<Entity> resultList = new List<Entity>();

            do
            {
                Debug.WriteLine(string.Format("page = {0}", pageNumber));

                if (pageNumber != 1)
                {
                    query.PageInfo.PageNumber = pageNumber;
                    query.PageInfo.PagingCookie = pageCookie;
                }
                ec = crmSvc.RetrieveMultiple(query);
                if (ec.MoreRecords)
                {
                    pageNumber++;
                    pageCookie = ec.PagingCookie;
                }

                resultList.AddRange(ec.Entities);
            } while (ec.MoreRecords);


            // column list build
            List<string> keyNames = new List<string>();
            foreach (var item in ec.Entities)
            {
                foreach (KeyValuePair<String, Object> attribute in item.Attributes)
                    keyNames.Add(attribute.Key);

                break;
            }

            // sort column by name
            keyNames.Sort();
            foreach (string aName in keyNames)
            {
                dt.Columns.Add(aName);
                Debug.WriteLine(string.Format("column name = {0}", aName));
            }

            // row list to datatable
            foreach (var item in resultList)
            {
                DataRow dr = dt.NewRow();

                string saveData = "";
                foreach (string aName in keyNames)
                {
                    saveData = "";

                    if (item.Attributes.Contains(aName))
                    {
                        if (relAttrNames.Contains(aName))
                        {
                            // get reference to value
                            object val = GetAttributeValue1(item.Attributes[aName]);
                            if (val == null) saveData = "";
                            else saveData = val.ToString();
                        }
                        else
                        {
                            // get reference to id
                            object val = GetAttributeValue2(item.Attributes[aName]);
                            if (val == null) saveData = "";
                            else saveData = val.ToString();
                        }
                    }

                    dr[aName] = saveData;
                }

                dt.Rows.Add(dr);
            }

            // 조정중..
            {
            
            dt.Columns.Add("BackDate");
            foreach (DataRow row in dt.Rows)
            {
                //String endDate = ((string) row["new_dt_expired"]) + "/31";
                //row["Backdating"] = CalcBackDate(DateTime.ParseExact(endDate, "yyyy/MM/dd", null));
                //row["Backdating"] = "";

                String[] datesplit = (row["new_dt_expired"].ToString()).Split('-');

                try
                {
                    string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
                    DateTime endDate = DateTime.ParseExact(date, "yyyy/M/dd", null);
                    row["BackDate"] = CalcBackDate(endDate);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("backdate exception: " + row[4].ToString());
                    //System.Diagnostics.Debug.WriteLine("backdate exception: " + row["CustomerName"].ToString());
                    row["BackDate"] = "";
                }

            }
            
            dt.Columns.Add("Forward");
            foreach (DataRow row in dt.Rows)
            {

                String[] datesplit = (row["new_dt_expired"].ToString()).Split('-');

                try
                {
                    string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
                    DateTime endDate = DateTime.ParseExact(date, "yyyy/M/dd", null);
                    row["Forward"] = CalcForwardDate(endDate);
                }
                catch
                {
                    //System.Diagnostics.Debug.WriteLine("exception: " + row["ExpiredDate"].ToString());
                    //System.Diagnostics.Debug.WriteLine("exception: " + row["CustomerName"].ToString());
                    row["Forward"] = "";
                }

            }

            dt.Columns.Add("기간요약");
            foreach (DataRow row in dt.Rows)
            {
                String[] datesplit = (row["new_dt_expired"].ToString()).Split(' ');
                string forwarddata = "Forward (" + DateTime.Now.Year + "." + DateTime.Now.Month + ".1 ~ " + datesplit[0] + " = " + row["Forward"];
                string backdata = "BackDate (" + datesplit[0] + " ~ " + DateTime.Now.Year + "." + DateTime.Now.Month + "." +
                                DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + " = " + row["BackDate"];

                string final = forwarddata + "<br/>" + backdata;

                row["기간요약"] = final;
            }


            dt.Columns.Add("customerRRP", typeof(int));
            //foreach (GridViewRow row in GridView2.Rows)
            //{
            //    WebControls.Label val = (WebControls.Label)row.FindControl("ProductType");
            //    //row.Cells[8].Text = string.Format("{0}", 10000);
            //    row.Cells[8].Text = "10000";
            //    //row.Cells[8].Text = 0;
            //}
            foreach (DataRow row in dt.Rows)
            {
                row["customerRRP"] = 10000;
                // Convert.ToInt32(row["customerRRP"])
            }


                dt.Columns.Add("proposalRRP", typeof(int));
            foreach (DataRow row in dt.Rows)
            {
                    row["proposalRRP"] = Convert.ToInt32(row["customerRRP"]) * Convert.ToInt32(row["new_i_qty"]);
                    // Convert.ToInt32(row["단가"])
            }


            dt.Columns.Add("견적가격", typeof(int));
            foreach (DataRow row in dt.Rows)
            {
                row["견적가격"] = Convert.ToInt32(row["proposalRRP"]) * (1 - Convert.ToDouble(forwardDiscount.Text)) / 12 * Convert.ToInt32(row["Forward"]);
            }

            }
            
        }

        //return ds;
    }

    protected bool connected(CrmServiceClient svc)
    {
        if (svc == null)
        {
            string ConnectionString = "" +
                                  "AuthType = OAuth; " +
                                    "Username = " + CConst.crmUID + "; " +
                                    "Password = " + CConst.crmPWD + "; " +
                                    "Url = https://solko.crm5.dynamics.com;" +
                                    "AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;" +
                                    "RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;" +
                                    "LoginPrompt=Never";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            crmSvc = new CrmServiceClient(ConnectionString);
            if (crmSvc.IsReady)
            {
                //var myContact = new Entity("contact");
                //myContact.Attributes["lastname"] = "홍";
                //myContact.Attributes["firstname"] = "종성";
                //Guid newid = crmSvc.Create(myContact);
            }

            svc = crmSvc;
        }


        return svc != null && svc.IsReady;
    }

    private object GetAttributeValue1(object entityValue)
    {
        object output = "";
        switch (entityValue.ToString())
        {
            case "Microsoft.Xrm.Sdk.EntityReference":
                output = ((EntityReference)entityValue).Name;
                break;
            case "Microsoft.Xrm.Sdk.OptionSetValue":
                output = ((OptionSetValue)entityValue).Value.ToString();
                break;
            case "Microsoft.Xrm.Sdk.Money":
                output = ((Money)entityValue).Value.ToString();
                break;
            case "Microsoft.Xrm.Sdk.AliasedValue":
                output = GetAttributeValue1(((Microsoft.Xrm.Sdk.AliasedValue)entityValue).Value);
                break;
            default:
                output = entityValue.ToString();
                break;
        }
        return output;
    }

    private object GetAttributeValue2(object entityValue)
    {
        object output = "";
        switch (entityValue.ToString())
        {
            case "Microsoft.Xrm.Sdk.EntityReference":
                output = ((EntityReference)entityValue).Id.ToString();
                break;
            case "Microsoft.Xrm.Sdk.OptionSetValue":
                output = ((OptionSetValue)entityValue).Value.ToString();
                break;
            case "Microsoft.Xrm.Sdk.Money":
                output = ((Money)entityValue).Value.ToString();
                break;
            case "Microsoft.Xrm.Sdk.AliasedValue":
                output = GetAttributeValue2(((Microsoft.Xrm.Sdk.AliasedValue)entityValue).Value);
                break;
            default:
                output = entityValue.ToString();
                break;
        }
        return output;
    }


    #endregion


    protected int CalcBackDate(DateTime expDate)
    {
        if (expDate < DateTime.Now)
        {
            return (int)((DateTime.Now.Year - expDate.Year) * 12) + DateTime.Now.Month - expDate.Month;
        }
        else
        {
            return 0;
        }
    }

    protected int CalcForwardDate(DateTime expDate)
    {
        if (expDate > DateTime.Now)
        {
            return (int) ((expDate.Year - DateTime.Now.Year) * 12) + expDate.Month - DateTime.Now.Month;
        }
        else
        {
            return 12;
        }
        
    }

    protected SqlCommand CreateSqlCommand(string id_list)
    {
        DB DB = new DB();
        using (SqlConnection dbConn = DemoHelper.setup_connection())
        using (SqlCommand sqlCmd = dbConn.CreateCommand())
        {

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandText = "dbo.getcustomer";

            sqlCmd.Parameters.Add("@list", SqlDbType.Structured);
            sqlCmd.Parameters["@list"].Direction = ParameterDirection.Input;
            sqlCmd.Parameters["@list"].TypeName = "dbo.idlist";

            sqlCmd.Parameters["@list"].Value = new CSV_splitter(id_list, ' ');

            return sqlCmd;
        }
    }

    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        GridView3.PageIndex = e.NewPageIndex;
        if (Session[_pageSortedKey] != null)
        {
            GridView3.DataSource = Session[_pageSortedKey];
            GridView3.DataBind();
        }
        else
        {
            //String ids = "";
            //String[] id_list;

            //if (Request.QueryString["ID"] != null)
            //    ids = Request.QueryString["ID"];
            //id_list = ids.Split(' ');

            //SqlCommand sqlCmd = CreateSqlCommand(id_list);
            //BindGridView1(id_list);
            //BindGridView_CRM(id_list);
            GridView3.DataSource = dt;
            GridView3.DataBind();
        }
    }

    protected string[] getLicDates(int licType, string StartDate = "Today")
    {
        string[] licDate = { "", "" };
        DateTime dtS;
        if (StartDate == "Today")
        {
            dtS = DateTime.Today;
        }
        else
        {
            dtS = DateTime.Parse(StartDate);
        }

        licDate[0] = dtS.ToString("yyyy-MM-dd");
        DateTime dtE;
        switch (licType)
        {
            case 1:
                dtE = dtS.AddYears(1);
                licDate[1] = dtE.ToString("yyyy-MM-dd");
                break;
            case 5:
                dtE = dtS.AddMonths(1);
                licDate[1] = dtE.ToString("yyyy-MM-dd");
                break;
            case 6:
                dtE = dtS.AddMonths(1);
                licDate[1] = dtE.ToString("yyyy-MM-dd");
                break;
        }
        return licDate;
    }

    protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    protected void searchbtn_Click(object sender, EventArgs e)
    {
        //DateTime dte = new DateTime();

        //string yr = YearDropDown.SelectedValue;
        //string mon = MonthDropDown.SelectedValue;

        //string sSearch = txtSearch.Text;
        //sSearch = sSearch.Trim();

        ////if (string.IsNullOrEmpty(yr))

        //if (string.IsNullOrEmpty(sSearch))
        //    Session[_pageFilterKey] = "";
        //else
        //    Session[_pageFilterKey] = sSearch;

        ////Session[_pageSortedKey] = null;
        ////_sortDirection = "";
        

        ////BindGridView();
        //GridView2.DataSource = dt;
        //GridView2.PageIndex = 0; //since PageIndex starts from 0 by default.
        //GridView2.DataBind();
        ////Response.Redirect(Request.RawUrl);
    }


    #region "필요함수"
    protected object GetValue_Session(string keyName)
    {
        foreach (string k in Session.Keys)
        {
            if (string.Equals(keyName, k, StringComparison.OrdinalIgnoreCase))
            {
                return Session[k];
            }
        }

        return "";
    }

    protected string GetValue_GetMethod(string keyName)
    {
        System.Collections.Specialized.NameValueCollection plist = Request.QueryString;

        foreach (string k in plist)
        {
            string[] values = plist.GetValues(k);
            if (string.Equals(keyName, k, StringComparison.OrdinalIgnoreCase))
            {
                return values[0];
            }
        }

        return "";
    }

    protected object GetValue_PostMethod(string controlName)
    {
        string foundName = FindControlByName(controlName);

        if (string.IsNullOrEmpty(foundName))
            return null;

        return Request.Form[foundName];
    }

    protected bool IsTransferedOtherPage(string keyName, string compareValue)
    {
        string readValue = GetValue_GetMethod(keyName);
        if (string.IsNullOrEmpty(readValue))
            return false;

        if (compareValue.Equals(readValue, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    protected string FindControlByName(string keyName)
    {
        // "ctl00$MainContent$TextBox1" 형태의 데이터이므로, 찾는 콘트롤 명칭앞에 $를 둔다 
        string findString = "$" + keyName;

        foreach (string keyname in Request.Form.Keys)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(" form key={0}", keyname));
            if (keyname.EndsWith(findString))
                return keyname;
        }
        return "";
    }

    #endregion

    protected void applybtn_Click(object sender, EventArgs e)
    {
        foreach (GridViewRow row in GridView3.Rows)
        {
            WebControls.TextBox fwdDis = (WebControls.TextBox) row.FindControl("changeForwardDiscount");
            WebControls.TextBox backDis = (WebControls.TextBox)row.FindControl("changeBackDiscount");
            fwdDis.Text = forwardDiscount.Text;
            backDis.Text = backDiscount.Text;
            double fwdPrice = Convert.ToInt32(row.Cells[9].Text) * (1 - Convert.ToDouble(forwardDiscount.Text) * 0.01) / 12 * Convert.ToInt32(row.Cells[6].Text);
            double backPrice = Convert.ToInt32(row.Cells[9].Text) * (1 - Convert.ToInt32(backDiscount.Text) * 0.01) / 12 * Convert.ToInt32(row.Cells[5].Text);

            row.Cells[11].Text = "Forward Price: " + fwdPrice.ToString() + "<br>" + "BackDate Price: " + backPrice.ToString() + "Total: " + (fwdPrice + backPrice);
            //Debug.WriteLine("짱나" + row.Cells[0].Text);
        }
    }

    protected void estimatebtn_Click(object sender, EventArgs e)
    {

    }

    #region "안써"
    protected void GrantReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        //txtComment.Text = GridView1.Rows(Int32.Parse(arg[0])).col            ;
        GridViewRow row = GridView3.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        txtCompany.Text = row.Cells[1].Text;
        txtBuso.Text = row.Cells[2].Text;
        txtName.Text = row.Cells[3].Text;
        txtMail.Text = row.Cells[4].Text;
        txtOffice.Text = row.Cells[5].Text;
        txtPhone.Text = row.Cells[6].Text;
        txtswKey.Text = row.Cells[7].Text;
        txtprgName.Text = row.Cells[8].Text;
        txtprgVer.Text = row.Cells[9].Text;

        //txtComment.Text = row.Cells[16].Text;


        //txtComment.Text = row.Cells[6].Text;
        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalAdd').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalAdd.Checked = true;
        //Page_Load(this, new EventArgs());
    }

    protected void sendMail(string mail)

    {
        //SMTP서버 설정 읽어옴 > Web.config에 정의됨
        SmtpSection smtpConfig = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

        //발신자 및 수신자 메일 설정
        string senderName = "(주)솔코(SOLKO)";
        //string senderID = "monitor@bitlogic.kr"; (2019.07.19) smtp 서버 변경
        //string senderID = "tech@solidkorea.co.kr";
        //string senderID = "solko.xpmworks@gmail.com";
        string senderID = "xpmworks@naver.com";
        string receiveID = mail;
        //string receiveID = "jhpark@sinu.tech";

        string msgTitle = "xPMWorks License가 승인되었습니다. ";
        string msgContent = "<div style=\"font-size: 10pt;\" >" +
            "<p>xPMWorks License의 승인이 완료되었습니다.</p>" +
            "<p>xPMWorks의 '라이선스 관리'에서 '라이선스 확인'을 누르시면 새로운 라이선스로 갱신됩니다.</p>" +
            "<p>감사합니다.</p>" +
            "</div>";

        string msgContentEng = "<br/><br/><div style=\"font-size: 10pt;\" >" +
            "<p>The xPMWorks License has been approved.</p>" +
            "<p>If you click 'Download License' in 'License Management' of xPMWorks, it is renewed with a new license.</p>" +
            "<p>Thank you.</p>" +
            "</div>";

        //메일 컨텐츠 설정 (발송자, 수신자, 메일제목, 메일내용 등..)
        MailMessage message = new MailMessage();
        message.From = new MailAddress(senderID, senderName); //new MailAddress(발송자메일, 발송자명) 설정 시 : 받은메일함에 메일 주소가아닌 보낸이 이름이 표시된다. (발송자명은 옵션)
        message.To.Add(new MailAddress(receiveID));
        message.Subject = msgTitle;
        message.Body = msgContent + msgContentEng;
        message.SubjectEncoding = System.Text.Encoding.UTF8;  //메일 제목의 Encoding을 UTF8로 설정
        message.BodyEncoding = System.Text.Encoding.UTF8;     //메일 내용의 Encoding을 UTF8로 설정
        message.IsBodyHtml = true;                            //메일 본문을 HTML형식을 지원하도록 설정

        //첨부파일 설정
        //////Attachment at = new Attachment(@"c:\test_image.jpg", MediaTypeNames.Image.Jpeg);
        //////at.ContentId = "ContentIDO";  //Attachment의 ContentID를 통해 메일 내용의 Img태그에서 접근 가능함
        //////message.Attachments.Add(at);

        //SMTP 설정
        SmtpClient smtpClient = new SmtpClient(smtpConfig.Network.Host, smtpConfig.Network.Port);
        smtpClient.UseDefaultCredentials = false;
        //SMTP서버로부터 인증을 받기위한 Credentials 생성
        NetworkCredential networkCred = new NetworkCredential(smtpConfig.Network.UserName, smtpConfig.Network.Password);
        smtpClient.Credentials = networkCred;
        //SSL접속을 할 수 있도록 EnableSsl을 True로 설정 (구글은 필수, 미설정 시 메일 발송이 되지않음.)
        //smtpClient.EnableSsl = false;
        smtpClient.EnableSsl = true;  // 2021.02.08 부터 사용
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

        //Send 메서드를 이용하여 메일을 발송한다.
        smtpClient.Send(message);
    }


    protected void sendMail_Reject(string mail)
    {
        //SMTP서버 설정 읽어옴 > Web.config에 정의됨
        SmtpSection smtpConfig = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

        //발신자 및 수신자 메일 설정
        string senderName = "(주)솔코(SOLKO)";
        //string senderID = "monitor@bitlogic.kr"; (2019.07.19) smtp 서버 변경
        //string senderID = "tech@solidkorea.co.kr";
        //string senderID = "solko.xpmworks@gmail.com";
        string senderID = "xpmworks@naver.com";
        string receiveID = mail;
        //string receiveID = "jhpark@sinu.tech";

        string msgTitle = "xPMWorks License 구매 또는 SOLIDWORKS 유지보수 계약이 필요합니다(Requires xPMWorks License purchase or SOLIDWORKS subscription contract)";
        string msgContent = "<div style=\"font-size: 10pt;\" >" +
            "<p>xPMWorks License의 발행을 위하여 유효한 기간에 포함된 유지보수 계약된 SOLIDWORKS 시리얼 번호가 필요합니다.</p>" +
            "<p>제공해 주신 SOLIDWORKS 시리얼 번호는 (주)솔코가 확인할 수 있는 시리얼 번호가 아니거나, 유효한 기간을 벗어난 상태이므로 xPMWorks License를 발급할 수 없습니다.</p>" +
            "<p>xPMWorks License 구매 또는 SOLIDWORKS 유지보수 계약의 문의는 아래 연락처로 문의하여 주시기 바랍니다.</p>" +
            "<p>문의처 : (주)솔코</p>" +
            "<p>홈페이지 : http://www.solidkorea.co.kr</p>" +
            "<p>전화번호 : 031) 8069-8300</p>" +
            "<p>감사합니다.</p>" +
            "</div>";

        string msgContentEng = "<br/><br/><div style=\"font-size: 10pt;\" >" +
            "<p>For the issuance of the xPMWorks License, a subscription contracted SOLIDWORKS serial number included in the valid period is required.</p>" +
            "<p>The SOLIDWORKS serial number you provided is not a serial number that SOLKO Co., Ltd. can check, or it is out of the validity period, so you cannot issue an xPMWorks License.</p>" +
            "<p>If you have any questions about xPMWorks License purchase or SOLIDWORKS subscription contract, please contact us at the contact information below.</p>" +
            "<p>Inquiries : SOLKO Co., Ltd.</p>" +
            "<p>Homepage : http://www.solidkorea.co.kr</p>" +
            "<p>Phone : +82 31-8069-8300</p>" +
            "<p>Thank you.</p>" +
            "</div>";

        //메일 컨텐츠 설정 (발송자, 수신자, 메일제목, 메일내용 등..)
        MailMessage message = new MailMessage();
        message.From = new MailAddress(senderID, senderName); //new MailAddress(발송자메일, 발송자명) 설정 시 : 받은메일함에 메일 주소가아닌 보낸이 이름이 표시된다. (발송자명은 옵션)
        message.To.Add(new MailAddress(receiveID));
        message.Subject = msgTitle;
        message.Body = msgContent + msgContentEng;
        message.SubjectEncoding = System.Text.Encoding.UTF8;  //메일 제목의 Encoding을 UTF8로 설정
        message.BodyEncoding = System.Text.Encoding.UTF8;     //메일 내용의 Encoding을 UTF8로 설정
        message.IsBodyHtml = true;                            //메일 본문을 HTML형식을 지원하도록 설정

        //첨부파일 설정
        //////Attachment at = new Attachment(@"c:\test_image.jpg", MediaTypeNames.Image.Jpeg);
        //////at.ContentId = "ContentIDO";  //Attachment의 ContentID를 통해 메일 내용의 Img태그에서 접근 가능함
        //////message.Attachments.Add(at);

        //SMTP 설정
        SmtpClient smtpClient = new SmtpClient(smtpConfig.Network.Host, smtpConfig.Network.Port);
        smtpClient.UseDefaultCredentials = false;
        //SMTP서버로부터 인증을 받기위한 Credentials 생성
        NetworkCredential networkCred = new NetworkCredential(smtpConfig.Network.UserName, smtpConfig.Network.Password);
        smtpClient.Credentials = networkCred;
        //SSL접속을 할 수 있도록 EnableSsl을 True로 설정 (구글은 필수, 미설정 시 메일 발송이 되지않음.)
        smtpClient.EnableSsl = true;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

        //Send 메서드를 이용하여 메일을 발송한다.
        smtpClient.Send(message);
    }


    protected void DeleteReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        GridViewRow row = GridView3.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalDel').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalDel.Checked = true;
    }

    protected void RejectReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        GridViewRow row = GridView3.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        txtMailReject.Text = row.Cells[4].Text;

        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalRej').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalRej.Checked = true;
    }

    protected void btnDel_Click(object sender, EventArgs e)
    {
        //int id = Int32.Parse(arg[0]);
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        DB.QueryInsert(dbConn, "UPDATE Request SET Deleted=1 WHERE ID=" + txtRowID.Text);
        dbConn.Close();
        hidemodalDel.Checked = false;
        Page_Load(this, new EventArgs());
    }

    protected void btnRej_Click(object sender, EventArgs e)
    {
        //int id = Int32.Parse(arg[0]);
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        DB.QueryInsert(dbConn, "UPDATE Request SET Deleted=1, Comment=Trim(concat(Comment, ' NoSUB')) WHERE ID=" + txtRowID.Text);
        dbConn.Close();

        if ((txtMailReject.Text + "").Length > 3)
        {
            sendMail_Reject(txtMailReject.Text);
        }

        hidemodalRej.Checked = false;
        Page_Load(this, new EventArgs());
    }
    protected void msgb(string MSG)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + MSG + "');", true);

    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {

        try
        {
            DateTime dStart = DateTime.Parse(txtLicStart.Text);
            DateTime dEnd = DateTime.Parse(txtLicEnd.Text);

        }
        catch
        {
            msgb("입력된 날짜가 정상적인 범위를 벗어났습니다(해당 월의 마지막 일자를 확인하여 주세요)");
            return;
        }



        int licType = 1;
        if (RadioButton1.Checked)
        {
            licType = 1;
        }
        if (RadioButton2.Checked)
        {
            licType = 5;
        }
        if (RadioButton3.Checked)
        {
            licType = 6;
        }

        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        string queryString = "UPDATE Request SET  Granted=1,GrantDate=Getdate() WHERE ID=" + txtRowID.Text + ";\n";
        queryString += "UPDATE License SET Deleted=1 WHERE SwKey=" + DB.convSql(txtswKey.Text, false) + ";\n";
        queryString += "INSERT INTO License (";
        queryString += "SwKey,";
        queryString += "Company,";
        queryString += "Buso,";
        queryString += "Position,";
        queryString += "Name,";
        queryString += "Mail,";
        queryString += "Phone1,";
        queryString += "Phone2,";
        queryString += "Comment,";
        queryString += "Type,";
        queryString += "StartDate,";
        queryString += "EndDate,";
        queryString += "ProgramName,";
        queryString += "ProgramVersion";
        queryString += ")";
        queryString += "VALUES (";
        queryString += DB.convSql(txtswKey.Text, true);
        queryString += DB.convSql(txtCompany.Text, true);
        queryString += DB.convSql(txtBuso.Text, true);
        queryString += DB.convSql("", true);
        queryString += DB.convSql(txtName.Text, true);
        queryString += DB.convSql(txtMail.Text, true);
        queryString += DB.convSql(txtOffice.Text, true);
        queryString += DB.convSql(txtPhone.Text, true);
        queryString += DB.convSql("", true);
        queryString += licType + ",";
        queryString += DB.convSql(txtLicStart.Text, true);
        queryString += DB.convSql(txtLicEnd.Text, true);
        queryString += DB.convSql(txtprgName.Text, true);
        queryString += DB.convSql(txtprgVer.Text, false);
        queryString += ");";



        DB.QueryInsert(dbConn, queryString);
        if ((txtMail.Text + "").Length > 3)
        {
            sendMail(txtMail.Text);
        }
        dbConn.Close();
        hidemodalAdd.Checked = false;
        Page_Load(this, new EventArgs());
    }
    protected void changRadio(object sender, EventArgs e)
    {
        int licType = 0;

        if (RadioButton1.Checked) { licType = 1; }
        if (RadioButton2.Checked) { licType = 5; }
        if (RadioButton3.Checked) { licType = 6; }

        string[] licDate = getLicDates(licType);
        txtLicStart.Text = licDate[0];
        txtLicEnd.Text = licDate[1];

        //switch (licType)
        //{
        //    case 1:
        //        txtLicMax.Text = "";
        //        txtLicMax.Visible = false;
        //        break;
        //    case 2:
        //        txtLicMax.Text = "1000";
        //        txtLicMax.Visible = true;
        //        break;
        //    case 3:
        //        txtLicMax.Text = "";
        //        txtLicMax.Visible = false;
        //        break;

        //}



    }
    protected void changeStartDate(object sender, EventArgs e)
    {

    }

    protected void dpStart_SelectionChanged(object sender, EventArgs e)
    {

        Calendar dp = (Calendar)sender;
        DateTime dt = dp.SelectedDate;
        txtLicStart.Text = dt.ToString("yyyy-MM-dd");
    }

    protected void dpEnd_SelectionChanged(object sender, EventArgs e)
    {
        Calendar dp = (Calendar)sender;
        DateTime dt = dp.SelectedDate;
        txtLicEnd.Text = dt.ToString("yyyy-MM-dd");

    }

    #endregion

    protected void discountRate_TextChanged(object sender, EventArgs e)
    {
        // 현재값 읽어서.. 할인율 재 계산 후, list price 업데이트
        //  1. 현재값 포맷 체크 : 숫자형식 
        //  2. 계산식 처리 
        //  3. list price cell 값 변경
        try
        {
            WebControls.TextBox t = (WebControls.TextBox)sender;
            GridViewRow row = t.NamingContainer as GridViewRow;
            int rowindex = row.RowIndex;

            //Debug.WriteLine(string.Format("new value = {0}", t.Text));
            //WebControls.TextBox txtbox = (WebControls.TextBox)GridView3.SelectedRow.Cells[10].FindControl("changeDiscount");
            GridView3.Rows[rowindex].Cells[11].Text =
                Convert.ToString(Convert.ToInt32(GridView3.Rows[rowindex].Cells[9].Text) * (1 - Convert.ToInt32(t.Text) * 0.01) / 12 * Convert.ToInt32(row.Cells[6].Text));
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('숫자를 입력해 주세요')", true);
        }
        //Console.WriteLine(string.Format("col={0}, row={1}", 
    }

    //protected void GridView3_DataBound(object sender, EventArgs e)
    //{
    //    //GridViewRow totalrow = new GridViewRow(GridView3.Rows.Count+1, 0, DataControlRowType.DataRow, DataControlRowState.Insert);
    //    DataRow totalRow = dt.NewRow();
    //    for (int i = 0; i < 10; i++)
    //        totalRow[i] = "합계";

    //    dt.Rows.Add(totalRow);
    //    Debug.WriteLine("됐어?");
    //    //totalRow.Cells[0].Text = "합계";
    //    //totalRow.Cells[0].ColumnSpan = 6;
    //    //totalRow.Cells[6].Text = "9";
    //    //totalRow.Cells[6].ColumnSpan = 5;
    //}
}

