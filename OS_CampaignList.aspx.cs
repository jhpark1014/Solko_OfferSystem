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
using System.Linq;

public partial class OS_CampaignList : System.Web.UI.Page
{
    static string _pageConstStDate = "PageKey_StartDate";
    static string _pageConstEnDate = "PageKey_EndDate";
    static string _pageConstWiDate = "PageKey_WillDate";
    static string _pageConstCRMKey = "PageKey_CRMKey";
    static string _pageConstAccountMap = "PageKey_AccountMap";
    static string _pageConstDataTable = "PageKey_GridDt";

    //private static readonly HttpClient client = new HttpClient();
    CRMService.CRMService crmService = new CRMService.CRMService();


    protected void Page_Load(object sender, EventArgs e)
    {
        Debug.WriteLine("Initial Page Load");

        if (!Page.IsPostBack)
        { // 최초 로딩인 경우 
            InitSessionDataByUIControl();
        }
        else
        {
            if(Session[_pageConstDataTable] != null)
            {
                GridView1.DataSource = Session[_pageConstDataTable];
                GridView1.DataBind();
            }
        }
    }
    
    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        if (Session[_pageConstDataTable] != null)
        {
            GridView1.DataSource = Session[_pageConstDataTable];
            GridView1.DataBind();
        }
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
        string sSearch = textFindyyyymm.Text;

        if (string.IsNullOrEmpty(sSearch)) sSearch = "";

        Session[_pageConstAccountMap] = new Dictionary<string, CRMAccount>();
        Session[_pageConstDataTable] = null;

    }

    protected void btnCreate_Click(object sender, EventArgs e)
    {
        DateTime d1 = DateTime.Now.AddMonths(2);
        string s1 = d1.ToString("yyyy/MM"); // 2023-06
        s1 = s1.Replace("-", "/");
        textBox1.Text = s1;

        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalAdd').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalAdd.Checked = true;
        //Page_Load(this, new EventArgs());
    }

    protected bool SetupTextBox(string s)
    {
        try
        {
            if (string.IsNullOrEmpty(s))
            {
                ClearDateBox();
                return false;
            }

            if (s.Length == 7 || s.Length == 6)
            {
                SetupDateBox(s);
                return true;
            }
            else
            {
                ClearDateBox();
                return false;
            }
        }
        catch
        {
            ClearDateBox();
            return false;
        }
    }

    protected void ClearDateBox()
    {
        textLicenseSearchStart.Text = "";
        textLicenseSearchEnd.Text = "";
        textMailWillDate.Text = "";

    }


    protected void SetupDateBox(string yyyymm)
    {
        if(!yyyymm.Contains("/"))
        {
            ClearDateBox();
        }
        else
        {
            string[] tokens = yyyymm.Split(CConst.splitSlash);
            if( tokens.Length != 2)
            {
                ClearDateBox();
                return;
            }

            int t1 = int.Parse(tokens[0]);
            int t2 = int.Parse(tokens[1]);

            string s1 = string.Format("{0:D4}-{1:D2}-01", t1, t2);
            try
            {
                DateTime d1 = DateTime.Parse(s1);          // 2023-06-01
                DateTime d2 = d1.AddMonths(1).AddDays(-1); // 2023-06-30
                DateTime d3 = GetFixDay(d1.AddMonths(-2), 2, 1);

                textLicenseSearchStart.Text = d1.ToString("yyyy-MM-dd");
                textLicenseSearchEnd.Text = d2.ToString("yyyy-MM-dd");
                textMailWillDate.Text = d3.ToString("yyyy-MM-dd");

                string sKey = d3.ToString("yyyy/MM");
                string caKey = GetKey_CampaignActivity(sKey);
                textCRMKey.Text = caKey;
            }
            catch
            {
                ClearDateBox();
            }
        }
    }

    protected string GetKey_CampaignActivity(string sKey1)
    {
        string sKey2 = string.Format("YLC_info");

        return string.Format("solko_{0}_{1}", sKey1, sKey2);
    }

    protected DateTime GetFixDay(DateTime startDate, int nWeeks, int nDay)
    { // 지정된 월의 nWeeks번째 주, nDay 요일 

        DateTime d1 = startDate.AddDays(14); // 약 2번째 주 
        DayOfWeek dw = d1.DayOfWeek;

        int step = 0;
        switch (dw)
        {
            case DayOfWeek.Sunday: step = -6; break;
            case DayOfWeek.Saturday: step = -5; break;
            case DayOfWeek.Friday: step = -4; break;
            case DayOfWeek.Thursday: step = -3; break;
            case DayOfWeek.Wednesday: step = -2; break;
            case DayOfWeek.Tuesday: step = -1; break;
            case DayOfWeek.Monday: step = -7; break;
            default: step = 0; break;
        }

        DateTime d2 = d1.AddDays(step);

        return d2;
    }


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


    private string FindMainCampaignGUID()
    {

        // 메인 마케팅 - 캠페인 관리번호로 찾기
        string c_key = CConst.crmMKTID;
        string c_guid = crmService.CRM_FindOneAsGuidString("campaign", "codename", c_key);

        return c_guid;
    }

    private string GetKey_CampaignSubject(DateTime dtStart, DateTime expDate)
    {
        string msg = string.Format("{0}년 {1}월 발송 - {2}/{3} 만료예정 라이센스 재계약 안내메일 보내기", dtStart.Year, dtStart.Month, expDate.Year, expDate.Month);
        return msg;
    }

    private string GetKey_CampaignTitle(DateTime dtStart, DateTime expDate)
    {
        string msg = string.Format("{0}년 {1}월 발송 - {2}/{3} 만료예정 라이센스 재계약 안내메일 보내기", dtStart.Year, dtStart.Month, expDate.Year, expDate.Month);
        return msg;
    }

    private string GetKey_Email(string sKey1, string accountID)
    {
        return string.Format("solko_{0}_{1}", sKey1, accountID);
    }

    private string GetMailBody(CRMAccount acc)
    {
        string userdata = "";
        {
            string ud = crmService.CRM_GetUserData(acc.rnUserID);
            DataSet ds = CCommon.decompressDS(ud);

            string username = CCommon.getDSValueAsString(ds, 0, "lastname") + CCommon.getDSValueAsString(ds, 0, "firstname");
            string useremail = CCommon.getDSValueAsString(ds, 0, "internalemailaddress");
            string userphone = CCommon.getDSValueAsString(ds, 0, "mobilephone");

            StringBuilder sbUser = new StringBuilder();
            sbUser.Append(username);            // 유지보수 담당 SOLKO 직원
            sbUser.Append(" / 031-8069-8300");  // 회사 대표번호
            if (!string.IsNullOrEmpty(userphone)) sbUser.Append(" / " + userphone);
            if (!string.IsNullOrEmpty(useremail)) sbUser.Append(" / " + useremail);

            userdata = sbUser.ToString();
        }

        string licdata1 = "";
        if (acc.LicenseList.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p><br>■ 재계약 대상 라이선스<br></p>");
            sb.AppendLine("<font face=\"맑은 고딕\">");
            sb.AppendLine("<table width=\"600\" border=\"1\" cellspacing=\"0\" cellpadding=\"3\">");
            sb.AppendLine(" <tbody>");
            sb.AppendLine("  <tr>");
            sb.AppendLine("    <th align=\"center\" width=\"23\"  bgcolor=\"#E5E5E5\" scope=\"col\">No.</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"220\" bgcolor=\"#E5E5E5\" scope=\"col\">제품명</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"130\" bgcolor=\"#E5E5E5\" scope=\"col\">시리얼번호</th>  ");
            sb.AppendLine("    <th align=\"center\" width=\"33\"  bgcolor=\"#E5E5E5\" scope=\"col\">타입</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"77\"  bgcolor=\"#E5E5E5\" scope=\"col\">만료일</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"35\"  bgcolor=\"#E5E5E5\" scope=\"col\">수량</th>       ");
            sb.AppendLine("  </tr>");
            int cnt = 0;
            foreach (CRMLicense lic in acc.LicenseList)
            {
                cnt++;
                sb.AppendLine(" ");
                sb.AppendLine("  <tr>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + cnt.ToString() + "</td>");
                sb.AppendLine("    <td align=\"left\"   bgcolor=\"#FFFFFF\">" + lic.CategoryName + " " + lic.ProductName + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.GetSerialMozaic() + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.LicType + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.DateEnd.ToString("yyyy-MM-dd") + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.Qty.ToString() + "</td>");
                sb.AppendLine("  </tr>");
                sb.AppendLine(" ");
            }
            sb.AppendLine(" </tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</font>");
            licdata1 = sb.ToString();
        }


        string licdata2 = "";
        if (acc.RemainLicenseList.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p><br>■ 올해 재계약 대상 라이선스<br></p>");
            sb.AppendLine("<font face=\"맑은 고딕\">");
            sb.AppendLine("<table width=\"600\" border=\"1\" cellspacing=\"0\" cellpadding=\"3\">");
            sb.AppendLine(" <tbody>");
            sb.AppendLine("  <tr>");
            sb.AppendLine("    <th align=\"center\" width=\"23\"  bgcolor=\"#E5E5E5\" scope=\"col\">No.</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"220\" bgcolor=\"#E5E5E5\" scope=\"col\">제품명</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"130\" bgcolor=\"#E5E5E5\" scope=\"col\">시리얼번호</th>  ");
            sb.AppendLine("    <th align=\"center\" width=\"33\"  bgcolor=\"#E5E5E5\" scope=\"col\">타입</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"77\"  bgcolor=\"#E5E5E5\" scope=\"col\">만료일</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"35\"  bgcolor=\"#E5E5E5\" scope=\"col\">수량</th>       ");
            sb.AppendLine("  </tr>");
            int cnt = 0;
            foreach (CRMLicense lic in acc.RemainLicenseList)
            {
                cnt++;
                sb.AppendLine(" ");
                sb.AppendLine("  <tr>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + cnt.ToString() + "</td>");
                sb.AppendLine("    <td align=\"left\"   bgcolor=\"#FFFFFF\">" + lic.CategoryName + " " + lic.ProductName + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.GetSerialMozaic() + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.LicType + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.DateEnd.ToString("yyyy-MM-dd") + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.Qty.ToString() + "</td>");
                sb.AppendLine("  </tr>");
                sb.AppendLine(" ");
            }
            sb.AppendLine(" </tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</font>");
            licdata2 = sb.ToString();
        }


        string licdata3 = "";
        if (acc.MissLicenseList.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p><br>■ 미갱신 라이선스 현황<br></p>");
            sb.AppendLine("<font face=\"맑은 고딕\">");
            sb.AppendLine("<table width=\"600\" border=\"1\" cellspacing=\"0\" cellpadding=\"3\">");
            sb.AppendLine(" <tbody>");
            sb.AppendLine("  <tr>");
            sb.AppendLine("    <th align=\"center\" width=\"23\"  bgcolor=\"#E5E5E5\" scope=\"col\">No.</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"220\" bgcolor=\"#E5E5E5\" scope=\"col\">제품명</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"130\" bgcolor=\"#E5E5E5\" scope=\"col\">시리얼번호</th>  ");
            sb.AppendLine("    <th align=\"center\" width=\"33\"  bgcolor=\"#E5E5E5\" scope=\"col\">타입</th>       ");
            sb.AppendLine("    <th align=\"center\" width=\"77\"  bgcolor=\"#E5E5E5\" scope=\"col\">만료일</th>     ");
            sb.AppendLine("    <th align=\"center\" width=\"35\"  bgcolor=\"#E5E5E5\" scope=\"col\">수량</th>       ");
            sb.AppendLine("  </tr>");
            int cnt = 0;
            foreach (CRMLicense lic in acc.MissLicenseList)
            {
                cnt++;
                sb.AppendLine(" ");
                sb.AppendLine("  <tr>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + cnt.ToString() + "</td>");
                sb.AppendLine("    <td align=\"left\"   bgcolor=\"#FFFFFF\">" + lic.CategoryName + " " + lic.ProductName + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.GetSerialMozaic() + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.LicType + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.DateEnd.ToString("yyyy-MM-dd") + "</td>");
                sb.AppendLine("    <td align=\"center\" bgcolor=\"#FFFFFF\">" + lic.Qty.ToString() + "</td>");
                sb.AppendLine("  </tr>");
                sb.AppendLine(" ");
            }
            sb.AppendLine(" </tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</font>");
            licdata3 = sb.ToString();
        }


        //string body = Solko_CRMMailer.Properties.Resources.MAILFORMAT1;  // html 탬플릿 파일
        string templateMailPath = AppDomain.CurrentDomain.BaseDirectory + "\\MAILFORMAT_CRMLicense.txt";
        string body = System.IO.File.ReadAllText(templateMailPath);
        body = body.Replace("####userdata####", userdata);
        body = body.Replace("####accountname####", acc.CompanyName);
        body = body.Replace("####accountaddress####", acc.Address);
        body = body.Replace("####tablebody1####", licdata1);
        body = body.Replace("####tablebody2####", licdata2);
        body = body.Replace("####tablebody3####", licdata3);

        return body;
    }

    private string GetSimpleMsg(CRMAccount acc)
    {
        StringBuilder sb = new StringBuilder();

        if (acc.LicenseList.Count > 0)
        {
            foreach (CRMLicense lic in acc.LicenseList)
            {
                sb.AppendLine(string.Format("{0}, {1}, {2}", lic.CategoryName + " " + lic.ProductName, lic.LicType, lic.Qty));
                sb.AppendLine("<br />");
            }
        }

        return sb.ToString();
    }

    public static string GetDateSimpleFormat(string date)
    {
        DateTime dt = DateTime.Parse(date);
        return dt.ToString("yyyy-MM-dd");
    }

    Dictionary<string, string> mUserDic = new Dictionary<string, string>();

    private string GetUserName(string guid)
    {
        string returnValue = "";

        if (mUserDic.ContainsKey(guid))
        {
            return mUserDic[guid];
        }
        else
        {
            returnValue = crmService.CRM_GetUserName(guid);
            if (!mUserDic.ContainsKey(guid))
            {
                mUserDic.Add(guid, returnValue);
            }
        }

        return returnValue;
    }

    private string GetCRMEntityUrl(string entityName, string uid)
    {
        return string.Format("https://solko.crm5.dynamics.com/main.aspx?appid=ad988732-d042-eb11-bb23-000d3a851c32&forceUCI=1&pagetype=entityrecord&etn={0}&id={1}", entityName, uid);
    }

    private string GetMailUrl(string uid, string subject)
    {
        return string.Format("OS_CampaignMailList.aspx?FROMMAIN=T&activityid={0}&subject={1}", uid, subject);
    }

    protected void btnStep1GetLicense_Click(object sender, EventArgs e)
    { // 기존의 캠페인 읽어오기 

        // 메인 마케팅 - 캠페인 관리번호로 찾기
        string parentGuid = FindMainCampaignGUID();
        string findStr = textFindyyyymm.Text;
        DataSet ds = CCommon.decompressDS(crmService.CRM_FindCampaignActivity("campaignactivity", parentGuid, "subject", findStr));

        CSimpleDS griddata = new CSimpleDS();
        for(int i=0; i<CCommon.getDSCount(ds); i++)
        {
            griddata.SetValue(i, "no", (i+ 1).ToString());
            griddata.SetValue(i, "guid", CCommon.getDSValueAsString(ds, i, "activityid"));
            griddata.SetValue(i, "subject", CCommon.getDSValueAsString(ds, i, "subject"));
            griddata.SetValue(i, "createdon", CCommon.getDSValueAsString(ds, i, "createdon"));
            griddata.SetValue(i, "statusCode", CCommon.getDSValueAsString(ds, i, "statuscode"));
            griddata.SetValue(i, "mailCount", CCommon.getDSValueAsString(ds, i, "_linkedEmails"));
            griddata.SetValue(i, "CRM보기", GetCRMEntityUrl("campaignactivity", CCommon.getDSValueAsString(ds, i, "activityid")));
            griddata.SetValue(i, "MAIL보기", GetMailUrl(CCommon.getDSValueAsString(ds, i, "activityid"), CCommon.getDSValueAsString(ds, i, "subject")));
        }
        Session[_pageConstDataTable] = griddata.GetTable();

        GridView1.DataSource = Session[_pageConstDataTable];
        GridView1.DataBind();
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        // 캠페인 활동 생성 

        string sInputYYYYMM = textBox1.Text;
        if( !SetupTextBox(sInputYYYYMM) )
        {
            msgb("날짜 형태를 yyyy/mm 형태로 입력해 주세요.");
            return;
        }


        // 메인 마케팅 - 캠페인 관리번호로 찾기
        string c_guid = FindMainCampaignGUID();

        string willDateS = textMailWillDate.Text;
        DateTime willDate = DateTime.Parse(willDateS);

        string startDate = textLicenseSearchStart.Text;
        DateTime expDateStart = DateTime.Parse(startDate);

        string endDate = textLicenseSearchEnd.Text;
        DateTime expDateEnd = DateTime.Parse(endDate);

        if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            return;


        //Page.ClientScript.RegisterStartupScript(GetType(), "", "InvokeWaitCursor();", true);

        

        string sKey = willDate.ToString("yyyy/MM");
        string caKey = GetKey_CampaignActivity(sKey);
        string caSubject = GetKey_CampaignSubject(willDate, expDateStart);
        string caDescription = GetKey_CampaignTitle(willDate, expDateStart);

        string caSendDate = willDate.ToString("yyyy/MM/dd");
        //                                                                                               초안, 전자메일
        string ca_guid = crmService.CRM_Create_CampaignActivity(c_guid, caKey, caSubject, caDescription, "8", "7", caSendDate, caSendDate);


        // 만료예정 라이센스 검색 
        Dictionary<string, CRMAccount> mAccountMap = new Dictionary<string, CRMAccount>();

        string data = crmService.CRM_GetExpireAbout(startDate, endDate, "*", "YES");

        // 디버깅용 2023/6 헬러코리아만 검색 
        //string data = crmService.CRM_GetExpireAbout(startDate, endDate, "5be6d5c7-6b7f-ed11-81ad-000d3a856300", "YES");


        DataSet ds = CCommon.decompressDS(data);
        if (CCommon.getDSCount(ds) > 0)
        {
            // 각 데이터를 회사별로 모아서 e-mail형태로 작성 및 발송하기 
            for (int i = 0; i < CCommon.getDSCount(ds); i++)
            {
                string compID = CCommon.getDSValueAsString(ds, i, "accountid");
                string compName = CCommon.getDSValueAsString(ds, i, "new_l_account");
                string compSite = CCommon.getDSValueAsString(ds, i, "ACC.new_txt_place");
                string compGrade = CCommon.getDSValueAsString(ds, i, "ACC.new_p_level");
                string compAddr = CCommon.getDSValueAsString(ds, i, "ACC.new_txt_address");
                string compTel = CCommon.getDSValueAsString(ds, i, "ACC.telephone1");
                string compFax = CCommon.getDSValueAsString(ds, i, "ACC.fax");
                string compRNuser = CCommon.getDSValueAsString(ds, i, "rnuserid");

                string licSerial = CCommon.getDSValueAsString(ds, i, "new_name");
                string licCategory = CCommon.getDSValueAsString(ds, i, "new_l_product_category");
                string licProduct = CCommon.getDSValueAsString(ds, i, "new_l_products");
                string licDateInstall = CCommon.getDSValueAsString(ds, i, "new_dt_install");
                string licDateEnd = CCommon.getDSValueAsString(ds, i, "new_dt_end");
                string licDateExpire = CCommon.getDSValueAsString(ds, i, "new_dt_expired");
                string licQty = CCommon.getDSValueAsString(ds, i, "new_i_qty");
                string licType = CCommon.getDSValueAsString(ds, i, "new_p_type");

                string techQty = CCommon.getDSValueAsString(ds, i, "techCount");

                string compKey = compName + "-" + compSite;

                if (mAccountMap.ContainsKey(compKey))
                {
                    CRMAccount acc = mAccountMap[compKey];
                    acc.techCount = techQty;
                    acc.sendWill = true;
                    acc.sendDate = willDateS;
                    acc.LicenseList.Add(new CRMLicense(licProduct, licCategory, licSerial, licDateInstall, licDateEnd, licDateExpire, licQty, licType));
                }
                else
                {
                    if (string.IsNullOrEmpty(compRNuser))
                    {
                        // RN 담당이 없는 경우, CRM 계정을 기본으로 넣기 
                        compRNuser = crmService.CRM_FindOneAsGuidString("systemuser", "internalemailaddress", CConst.crmDefaultRN);
                    }

                    CRMAccount acc = new CRMAccount(compID, compKey, compName, compSite, compGrade, compAddr, compTel, compFax, compRNuser);
                    acc.techCount = techQty;
                    acc.sendWill = true;
                    acc.sendDate = willDateS;
                    acc.LicenseList.Add(new CRMLicense(licProduct, licCategory, licSerial, licDateInstall, licDateEnd, licDateExpire, licQty, licType));

                    mAccountMap.Add(compKey, acc);
                }
            }


            // account로 데이터 모으기
            List<string> keyList = mAccountMap.Keys.ToList();
            keyList.Sort();


            // 기타 라이센스 수집하기.
            foreach (string key in keyList)
            {
                CRMAccount acc = mAccountMap[key];

                // 올해 내 남은 다른 라이센스 수집
                {
                    DateTime e1 = DateTime.Parse(endDate);  // e1=2023/4/30
                    DateTime e2 = e1.AddDays(1);            // e2=2023/5/1
                    if (e1.Year == e2.Year)
                    {
                        DateTime e3 = new DateTime(e2.Year, 12, 31);  // e3=2023/12/31

                        string tempStart = e2.ToString("yyyy/MM/dd");
                        string tempEnd = e3.ToString("yyyy/MM/dd");
                        string data2 = crmService.CRM_GetExpireAbout(tempStart, tempEnd, acc.guid, "NO");  // 2023/5/1 ~ 2023/12/31
                        DataSet ds2 = CCommon.decompressDS(data2);
                        if (CCommon.getDSCount(ds2) > 0)
                        { // 올해내 연장해야 할 것이 또 있음

                            for (int i = 0; i < CCommon.getDSCount(ds2); i++)
                            {
                                string licSerial = CCommon.getDSValueAsString(ds2, i, "new_name");
                                string licCategory = CCommon.getDSValueAsString(ds2, i, "new_l_product_category");
                                string licProduct = CCommon.getDSValueAsString(ds2, i, "new_l_products");
                                string licDateInstall = CCommon.getDSValueAsString(ds2, i, "new_dt_install");
                                string licDateEnd = CCommon.getDSValueAsString(ds2, i, "new_dt_end");
                                string licDateExpire = CCommon.getDSValueAsString(ds2, i, "new_dt_expired");
                                string licQty = CCommon.getDSValueAsString(ds2, i, "new_i_qty");
                                string licType = CCommon.getDSValueAsString(ds2, i, "new_p_type");

                                acc.RemainLicenseList.Add(new CRMLicense(licProduct, licCategory, licSerial, licDateInstall, licDateEnd, licDateExpire, licQty, licType));
                            }
                        }
                        else
                        { // 올해내 연장할 것이 없음
                        }
                    }
                    else
                    { // 검색범위가 년말까지였음. 내년것은 안 찾음
                    }
                }
            }


            // 연장하지 않은 것 찾기 
            foreach (string key in keyList)
            {
                CRMAccount acc = mAccountMap[key];

                // 연장하지 않는 것 찾기 : start-1day ~ start-3year
                {
                    DateTime e1 = DateTime.Parse(startDate);  // e1=2023/4/1
                    DateTime e2 = e1.AddDays(-1);            // e2=2023/3/31
                    {
                        DateTime e3 = e1.AddYears(-3);  // e3=2020/4/1

                        string tempStart = e3.ToString("yyyy/MM/dd");
                        string tempEnd = e2.ToString("yyyy/MM/dd");
                        string data2 = crmService.CRM_GetExpireAbout(tempStart, tempEnd, acc.guid, "NO");  // 2020/4/1 ~ 2023/3/31
                        DataSet ds2 = CCommon.decompressDS(data2);
                        if (CCommon.getDSCount(ds2) > 0)
                        { // 연장계약하지 않는 것

                            for (int i = 0; i < CCommon.getDSCount(ds2); i++)
                            {
                                string licSerial = CCommon.getDSValueAsString(ds2, i, "new_name");
                                string licCategory = CCommon.getDSValueAsString(ds2, i, "new_l_product_category");
                                string licProduct = CCommon.getDSValueAsString(ds2, i, "new_l_products");
                                string licDateInstall = CCommon.getDSValueAsString(ds2, i, "new_dt_install");
                                string licDateEnd = CCommon.getDSValueAsString(ds2, i, "new_dt_end");
                                string licDateExpire = CCommon.getDSValueAsString(ds2, i, "new_dt_expired");
                                string licQty = CCommon.getDSValueAsString(ds2, i, "new_i_qty");
                                string licType = CCommon.getDSValueAsString(ds2, i, "new_p_type");

                                acc.MissLicenseList.Add(new CRMLicense(licProduct, licCategory, licSerial, licDateInstall, licDateEnd, licDateExpire, licQty, licType));
                            }
                        }
                        else
                        { // 올해내 연장할 것이 없음
                        }
                    }
                }
            }
        }


        // 전자메일 생성 
        {
            List<string> keyList = mAccountMap.Keys.ToList();
            keyList.Sort();

            foreach (string key in keyList)
            {
                CRMAccount acc = mAccountMap[key];
                if (!string.IsNullOrEmpty(ca_guid))
                {
                    string em_Key = GetKey_Email(sKey, acc.AccountID);
                    string em_cnt_tech = acc.techCount;
                    string mailbody = GetMailBody(acc);
                    string mailsummary = GetSimpleMsg(acc);

                    CSimpleDS sds = new CSimpleDS();
                    sds.SetValue(0, "key", em_Key);
                    sds.SetValue(0, "accountName", acc.CompanyName);
                    sds.SetValue(0, "accountid", acc.AccountID);
                    sds.SetValue(0, "send_date", acc.sendDate);
                    sds.SetValue(0, "cnt_tech_support", em_cnt_tech);
                    sds.SetValue(0, "rnUser_guid", acc.rnUserID);
                    sds.SetValue(0, "campaignactivity_guid", ca_guid);
                    sds.SetValue(0, "account_guid", acc.guid);
                    sds.SetValue(0, "mailbody", mailbody);
                    sds.SetValue(0, "mailsummary", mailsummary);

                    string em_guid = crmService.CRM_Create_EMail(CCommon.compressDS(sds.GetDataSet()));
                    if (string.IsNullOrEmpty(em_guid))
                    {
                        //CLog.WriteLog(string.Format("error while create email in CRM = {0}", em_Key));
                    }
                }
            }
        }


        //Page.ClientScript.RegisterStartupScript(GetType(), "", "InvokeDefaultCursor();", true);



        msgb(string.Format("캠페인 활동이 생성되었습니다."));

        hidemodalAdd.Checked = false;
        Page_Load(this, new EventArgs());
    }


    protected void btnWait_Click(object sender, EventArgs e)
    {
        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>javascript:LoadingWithMask(); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);


        //hidemodalWai.Checked = false;
        //Page_Load(this, new EventArgs());
    }


    protected void OnClick_DetailView(object sender, EventArgs e)
    {
        string[] args = (sender as LinkButton).CommandArgument.ToString().Split(CConst.splitComma);
        string guid = args[0];
        string subj = args[1];

        //GridViewRow row = GridView1.Rows[Int32.Parse(rowid)];
        //string guid2 = row.Cells[1].Text;
        //string subj = row.Cells[2].Text;

        string url = GetMailUrl(guid, subj);

        Response.Redirect(url);
    }

}

