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
using System.Net.Http;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using System.IO;
using Microsoft.SharePoint.Client;
using System.Security;

public partial class RequestList : System.Web.UI.Page
{
    private DataTable dt;
    //private static readonly HttpClient = new HttpClient();
    static string _pageFilterKey = "Usage_FilterKey";  // 페이지의 필터링데이터 키값
    static string _pageSortedKey = "Usage_SortedView";  // 페이지의 정렬데이터 키값
    static string _pageFilterStDate = "Usage_StartDate";
    static string _pageFilterFinDate = "Usage_FinishDate";
    static string _pageFilterSerial = "Usage_SerialNum";


    //
    static int COL_PRODUCTID = 10;


    private static readonly HttpClient client = new HttpClient();

    // CRM using 
    Microsoft.Xrm.Sdk.IOrganizationService os = null;
    CrmServiceClient crmSvc = null;


    protected void Page_Load(object sender, EventArgs e)
    {
        InitSessionDataByUIControl();
        Debug.WriteLine("Initial Page Load");

        if (!Page.IsPostBack)
        {
            //BindGridView_MSSQL();
            BindGridView_CRM();
            GridView1.DataSource = dt;
            //GridView1.DataBind();
        }
    }

    protected void BindGridView_CRM()
    {
        QueryDataFilter("new_customer_products", new ColumnSet(true), new List<string>() { "new_l_account", "new_l_products", "new_p_status", "new_l_product_category" });
    }

    protected void ClearData()
    {
        if (dt == null)
        {
            dt = new DataTable();
        }
        else
        {
            dt.Rows.Clear();
        }
    }

    #region "CRM API"

    protected void QueryDataFilter(string entityName, ColumnSet columnSet, List<string> relAttrNames)
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
            //WriteLog("QueryAndSave_V2()", "CRM 데이터 요청");
            string tempFilter = (string)Session[_pageFilterKey];
            string startFilter = (string)Session[_pageFilterStDate];
            string finFilter = (string)Session[_pageFilterFinDate];
            string serialFilter = (string)Session[_pageFilterSerial];

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

            // 활성화 된 데이터만 조회
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            // filter link condition style 2
            if (!string.IsNullOrEmpty(tempFilter.Trim()))
            {
                LinkEntity customerRef = query.AddLink("account", "new_l_account", "accountid");
                //LinkEntity customerRefEng = query.AddLink("accountEng", "new_txt_account_eng", "accountidEng");

                FilterExpression childFilter = query.Criteria.AddFilter(LogicalOperator.Or);

                childFilter.AddCondition("account", "name", ConditionOperator.Like, "%" + tempFilter.Trim() + "%");
                childFilter.AddCondition("account", "new_txt_account_eng", ConditionOperator.Like, "%" + tempFilter.Trim() + "%");

                //query.Criteria.AddCondition("account", "name", ConditionOperator.Contains, customerName.Trim());
                //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "필옵틱스");
                //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Contains, customerName.Trim());
            }

            // filter 3 : 활성라이센스 일자 범위
            if (!string.IsNullOrEmpty(startFilter))
            {
                startFilter = startFilter + "/01";
                //DateTime stDate = DateTime.ParseExact(startFilter, "yyyy/MM/dd", null);
                DateTime stDate = DateTime.ParseExact(startFilter, "yyyy/M/dd", null);
                query.Criteria.AddCondition("new_dt_expired", ConditionOperator.GreaterEqual, stDate);   //  end >= startdate
            }

            if (!string.IsNullOrEmpty(finFilter))
            {
                finFilter = finFilter + "/01";
                //DateTime finDate = DateTime.ParseExact(finFilter, "yyyy/MM/dd", null);
                DateTime finDate = DateTime.ParseExact(finFilter, "yyyy/M/dd", null);
                finDate = finDate.AddMonths(+1);
                finDate = finDate.AddDays(-1);
                query.Criteria.AddCondition("new_dt_expired", ConditionOperator.LessEqual, finDate);   // end <= finishdate
            }

            // filter 4: Serial Number
            if (!string.IsNullOrEmpty(serialFilter.Trim()))
            {
                //LinkEntity serialNumRef = query.AddLink("account", "new_name", "accountid");
                //query.Criteria.AddCondition("account", "name", ConditionOperator.Like, serialFilter.Trim() + "%");
                query.Criteria.AddCondition("new_name", ConditionOperator.Like, serialFilter.Trim() + "%");
                //query.Criteria.AddCondition("account", "name", ConditionOperator.Contains, customerName.Trim());
                //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "필옵틱스");
                //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Contains, customerName.Trim());
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
                //Debug.WriteLine(string.Format("column name = {0}", aName));
            }

            // 테이블 컬럼 정의 후, 필요한 컬럼인데 생성되지 않으면 강제 생성
            if (!dt.Columns.Contains("new_p_type"))
            {
                dt.Columns.Add("new_p_type");
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
                            //object val = GetAttributeValue1(item.Attributes[aName]);
                            object val = GetAttributeValue3(item, aName);
                            if (val == null) saveData = "";
                            else saveData = val.ToString();
                        }
                        else
                        {
                            // get reference to id
                            //object val = GetAttributeValue2(item.Attributes[aName]);
                            object val = GetAttributeValue3(item, aName);
                            if (val == null) saveData = "";
                            else saveData = val.ToString();
                        }
                    }

                    dr[aName] = saveData;
                }

                // 만약, 타입(new_p_type)이 널 이면, ALC로 기본값 
                {
                    object sType = dr["new_p_type"];
                    if (IsNull(sType))
                    {
                        dr["new_p_type"] = "ALC";
                    }
                }

                // (2023.06.08 추가) 만약, 타입(new_p_type)이 널 이면, ALC로 기본값
                {
                    object sType = dr["new_p_type"];
                    if (IsNull(sType))
                    {
                        dr["new_p_type"] = "ALC";
                    }
                }

                // (2023.06.13 추가) 날짜 정보는 한국시간으로 변경(+9시간 처리)
                {

                    // expire date
                    if (dt.Columns.Contains("new_dt_expired"))
                    {
                        object sType = dr["new_dt_expired"];
                        if (!IsNull(sType))
                        {
                            string dateTemp = (string)dr["new_dt_expired"];
                            dateTemp = ToKoreanDate(dateTemp);
                            dr["new_dt_expired"] = dateTemp;
                        }
                    }

                    // end date
                    if (dt.Columns.Contains("new_dt_end"))
                    {
                        object sType = dr["new_dt_end"];
                        if (!IsNull(sType))
                        {
                            string dateTemp = (string)dr["new_dt_end"];
                            dateTemp = ToKoreanDate(dateTemp);
                            dr["new_dt_end"] = dateTemp;
                        }
                    }

                    // install date
                    if (dt.Columns.Contains("new_dt_install"))
                    {
                        object sType = dr["new_dt_install"];
                        if (!IsNull(sType))
                        {
                            string dateTemp = (string)dr["new_dt_install"];
                            dateTemp = ToKoreanDate(dateTemp);
                            dr["new_dt_install"] = dateTemp;
                        }
                    }
                }

                dt.Rows.Add(dr);
            }


            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    // End Date / Expired Date Column 형식 변경
                    //string instDate = row["new_dt_install"].ToString();
                    //string endDate = row["new_dt_end"].ToString();
                    //string expDate = row["new_dt_expired"].ToString();

                    ////string[] splitDate1 = instDate.Split(' ');
                    //string[] splitDate2 = endDate.Split(' ');
                    //string[] splitDate3 = expDate.Split(' ');

                    ////row["new_dt_install"] = splitDate1[0];
                    //row["new_dt_end"] = splitDate2[0];
                    //row["new_dt_expired"] = splitDate3[0];

                    // Serial Number Column 형식 변경
                    // 8+********+뒤8
                    string serialNum = row["new_name"].ToString();
                    string changedSerial = serialNum.Substring(0, 8) + "********" + serialNum.Substring(16, 8);
                    row["new_name"] = changedSerial;
                }
                catch
                {
                }
            }

        }

        //return ds;
    }

    protected bool IsNull(string val)
    {
        if (string.IsNullOrEmpty(val))
            return true;

        if (string.IsNullOrWhiteSpace(val))
            return true;

        if ("&nbsp;".Equals(val, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    protected bool IsNull(object val)
    {
        if (DBNull.Value.Equals(val))
            return true;

        //if (string.IsNullOrEmpty(val))
        //    return true;

        //if (string.IsNullOrWhiteSpace(val))
        //    return true;

        //if ("&nbsp;".Equals(val, StringComparison.OrdinalIgnoreCase))
        //    return true;

        return false;
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

    //private object GetAttributeValue1(object entityValue)
    //{
    //    object output = "";
    //    switch (entityValue.ToString())
    //    {
    //        case "Microsoft.Xrm.Sdk.EntityReference":
    //            output = ((EntityReference)entityValue).Name;
    //            break;
    //        case "Microsoft.Xrm.Sdk.OptionSetValue":
    //            output = ((OptionSetValue)entityValue).Value.ToString();
    //            break;
    //        case "Microsoft.Xrm.Sdk.Money":
    //            output = ((Money)entityValue).Value.ToString();
    //            break;
    //        case "Microsoft.Xrm.Sdk.AliasedValue":
    //            output = GetAttributeValue1(((Microsoft.Xrm.Sdk.AliasedValue)entityValue).Value);
    //            break;
    //        default:
    //            output = entityValue.ToString();
    //            break;
    //    }
    //    return output;
    //}

    //private object GetAttributeValue2(object entityValue)
    //{
    //    object output = "";
    //    switch (entityValue.ToString())
    //    {
    //        case "Microsoft.Xrm.Sdk.EntityReference":
    //            output = ((EntityReference)entityValue).Id.ToString();
    //            break;
    //        case "Microsoft.Xrm.Sdk.OptionSetValue":
    //            output = ((OptionSetValue)entityValue).Value.ToString();
    //            break;
    //        case "Microsoft.Xrm.Sdk.Money":
    //            output = ((Money)entityValue).Value.ToString();
    //            break;
    //        case "Microsoft.Xrm.Sdk.AliasedValue":
    //            output = GetAttributeValue2(((Microsoft.Xrm.Sdk.AliasedValue)entityValue).Value);
    //            break;
    //        default:
    //            output = entityValue.ToString();
    //            break;
    //    }
    //    return output;
    //}

    private object GetAttributeValue3(Entity item, string aName)
    {
        object output = "";
        object entityValue = item.Attributes[aName];
        switch (entityValue.ToString())
        {
            case "Microsoft.Xrm.Sdk.EntityReference":
                output = ((EntityReference)entityValue).Name;
                break;
            case "Microsoft.Xrm.Sdk.OptionSetValue":
                //output = ((OptionSetValue)entityValue).Value.ToString();
                output = item.FormattedValues[aName].ToString();
                break;
            case "Microsoft.Xrm.Sdk.Money":
                output = ((Money)entityValue).Value.ToString();
                break;
            case "Microsoft.Xrm.Sdk.AliasedValue":
                //output = GetAttributeValue1(((Microsoft.Xrm.Sdk.AliasedValue)entityValue).Value);
                output = item.FormattedValues[aName].ToString();
                break;
            default:
                output = entityValue.ToString();
                break;
        }
        return output;
    }


    #endregion

    protected void BindGridView_MSSQL()
    {
        // string sqlALL = "SELECT * FROM ProductInfo ORDER BY CustomerName;";
        string sqlALL = "SELECT * FROM ProductInfo ORDER BY CustomerName;";

        List<SqlParameter> paramList = new List<SqlParameter>();

        string tempFilter = (string)Session[_pageFilterKey];
        string startFilter = (string)Session[_pageFilterStDate];
        string finFilter = (string)Session[_pageFilterFinDate];
        string serialFilter = (string)Session[_pageFilterSerial];

        if (!string.IsNullOrEmpty(tempFilter) || !string.IsNullOrEmpty(startFilter) || !string.IsNullOrEmpty(finFilter) || !string.IsNullOrEmpty(serialFilter))
        //if (Session[_pageFilterKey] != null || Session[_pageFilterStDate] != null || Session[_pageFilterFinDate] != null)
        {
            Print("들어옴");

            if (!string.IsNullOrEmpty(startFilter))
            {
                startFilter = startFilter + "/01";
                DateTime stDate = DateTime.ParseExact(startFilter, "yyyy/MM/dd", null);
                //System.Diagnostics.Debug.WriteLine("startdate" + stDate);
            }
            if (!string.IsNullOrEmpty(finFilter))
            {
                finFilter = finFilter + "/31";
                DateTime finDate = DateTime.ParseExact(finFilter, "yyyy/MM/dd", null);
                //System.Diagnostics.Debug.WriteLine("finishdate" + finDate);
            }

            // 조건이 있으면
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" SELECT *  FROM ProductInfo  						 ");
            sb.AppendLine("  WHERE ( @CNAME = '' OR CustomerName like @CNAME) 	 ");
            sb.AppendLine("    AND ( @stDate = '' OR EndDate >= @stDate) 		 ");
            sb.AppendLine("    AND ( @finDate = '' OR EndDate <= @finDate)		 ");
            sb.AppendLine("  ORDER BY CustomerName;								 ");
            sqlALL = sb.ToString();

            paramList.Add(new SqlParameter("@CNAME", "%" + tempFilter+"%"));
            paramList.Add(new SqlParameter("@stDate", startFilter));
            paramList.Add(new SqlParameter("@finDate", finFilter));

        }

        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();

        SqlCommand sqlCmd = new SqlCommand(sqlALL, dbConn);
        foreach (SqlParameter p in paramList)
            sqlCmd.Parameters.Add(p);

        //System.Diagnostics.Debug.WriteLine(sqlALL);

        SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
        DataTable tempDT = new DataTable();
        da.Fill(tempDT);
        dt = tempDT;
        dbConn.Close();
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

    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        InitSessionDataByUIControl();

        //Print(string.Format("selected page no = {0}", e.NewPageIndex));

        //Print(string.Format("_pageFilterKey = {0}", Session[_pageFilterKey]));
        //Print(string.Format("_pageFilterYear = {0}", Session[_pageFilterYear]));
        //Print(string.Format("_pageFilterMon = {0}", Session[_pageFilterMon]));

        GridView1.PageIndex = e.NewPageIndex;
        if (Session[_pageSortedKey] != null)
        {
            GridView1.DataSource = Session[_pageSortedKey];
            GridView1.DataBind();
        }
        else
        {
            //BindGridView_MSSQL();
            BindGridView_CRM();
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }
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

    protected void Print(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
    }

    protected void InitSessionDataByUIControl()
    {
        String stDate = startDateSearch.Text;
        String finDate = finDateSearch.Text;
        string sSearch = txtSearch.Text;
        string serialnum = serialSearch.Text;

        if (string.IsNullOrEmpty(sSearch))
            sSearch = "";
        if (string.IsNullOrEmpty(serialnum))
            serialnum = "";

        Session[_pageFilterStDate] = stDate;
        Session[_pageFilterFinDate] = finDate;
        Session[_pageFilterKey] = sSearch;
        Session[_pageFilterSerial] = serialnum;
    }

    protected void searchbtn_Click(object sender, EventArgs e)
    {
        string stDate = startDateSearch.Text;
        string finDate = finDateSearch.Text;

        string sSearch = txtSearch.Text;
        sSearch = sSearch.Trim();

        string serialnum = serialSearch.Text;
        serialnum = serialnum.Trim();

        Session[_pageFilterStDate] = stDate;
        Session[_pageFilterFinDate] = finDate;
        Session[_pageFilterKey] = sSearch;
        Session[_pageFilterSerial] = serialnum;

        //BindGridView_MSSQL();
        BindGridView_CRM();
        GridView1.DataSource = dt;
        GridView1.PageIndex = 0; //since PageIndex starts from 0 by default.
        GridView1.DataBind();
        //Response.Redirect(Request.RawUrl);
    }

    protected void Calculatebtn_Click(object sender, EventArgs e)
    {
        selIDList.Text = "";
        HashSet<string> complist = new HashSet<string>();

        foreach (GridViewRow row in GridView1.Rows)
        {
            CheckBox chk = (CheckBox)row.FindControl("CheckBox1");
            if (chk.Checked == true)
            {

                selIDList.Text += row.Cells[COL_PRODUCTID].Text + " ";
                complist.Add(row.Cells[0].Text);
            }
        }

        string name = "";
        foreach (string str in complist)
        {
            name = str;
            Debug.WriteLine("complist: " + str + complist.Count);
        }

        if (complist.Count != 1)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('한 회사만 선택해 주세요')", true);
        }

        else
        {
            //List<int> listofIDs = ((string) selIDList.Text.Split(' ')).ToList();
            Session["sendIDList"] = selIDList.Text;
            Session["companyName"] = name;

            System.Diagnostics.Debug.WriteLine("IDLIST  " + selIDList.Text);

            //Server.Transfer("OS_Calculate.aspx?FROMMAIN=T");
            Response.Redirect("OS_Calculate.aspx?FROMMAIN=T");
        }
    }

    protected void resetbtn_Click(object sender, EventArgs e)
    {
        startDateSearch.Text = "";
        finDateSearch.Text = "";
        txtSearch.Text = "";

        InitSessionDataByUIControl();

        //BindGridView_MSSQL();
        //BindGridView_CRM();
        ClearData();
        GridView1.DataSource = dt;
        GridView1.DataBind();
    }

    public string ToKoreanDate(string dateGMT)
    {
        try
        {
            DateTime date1 = DateTime.Parse(dateGMT);
            date1 = date1.AddHours(9);
            return date1.ToString("yyyy-MM-dd");
        }
        catch
        {
            return "";
        }
    }


    #region " 참고용 과거 소스 "

    protected void GrantReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        //txtComment.Text = GridView1.Rows(Int32.Parse(arg[0])).col            ;
        GridViewRow row = GridView1.Rows[Int32.Parse(arg[0])];
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
        GridViewRow row = GridView1.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalDel').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalDel.Checked = true;
    }

    protected void RejectReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        GridViewRow row = GridView1.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        txtMailReject.Text = row.Cells[4].Text;

        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalRej').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalRej.Checked = true;
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


    protected void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox chkall = (CheckBox) sender;

        if (chkall.Checked == true)
        {
            foreach (GridViewRow row in GridView1.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("CheckBox1");
                chk.Checked = true;
            }
        }
        else if (chkall.Checked == false)
        {
            foreach (GridViewRow row in GridView1.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("CheckBox1");
                chk.Checked = false;
            }
        }
    }

    protected DataTable ConvertGrid2DataTable(GridView workView)
    {
        DataTable rtnTable = new DataTable();

        for (int i = 0; i < workView.Columns.Count; i++)
            rtnTable.Columns.Add(workView.Columns[i].HeaderText);

        for (int r = 0; r < workView.Rows.Count; r++)
        {
            DataRow newRow = rtnTable.NewRow();

            for (int i = 0; i < workView.Columns.Count; i++)
            {
                newRow[i] = workView.Rows[r].Cells[i].Text;
            }
            rtnTable.Rows.Add(newRow);

        }

        return rtnTable;
    }


    protected void reloadExcel_Click(object sender, EventArgs e)
    {
        // sharepoint의 excel읽어서 dataset 저장(in server)
        DataSet ds1 = Listup_PriceTable_SharePoint("OfferSystem", "SOLKO_SW_PriceBook.xlsx", 1); /// 주의... 2 번 시트를 읽음
        DataSet ds2 = Listup_PriceTable_SharePoint("OfferSystem", "SOLKO_SW_PriceBook.xlsx", 2); /// 주의... 2 번 시트를 읽음

    }


    protected DataSet Listup_PriceTable_SharePoint(string siteName, string priceFilename, int readSheetIndex)
    {
        DataSet xlsDS = new DataSet();

        string readFilename = "";

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
            readFilename = filepath;

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

            string xlsName = GetExcelName1(readSheetIndex);
            if (System.IO.File.Exists(xlsName))
                System.IO.File.Delete(xlsName);

            xlsDS.WriteXml(xlsName, XmlWriteMode.WriteSchema);
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
            if (System.IO.File.Exists(readFilename))
                System.IO.File.Delete(readFilename);
        }

    }

    protected string GetExcelName1(int index)
    {
        string xlsName = Server.MapPath("~/temp/") + string.Format( "cost_file{0}.xml", index);
        return xlsName;
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

}

