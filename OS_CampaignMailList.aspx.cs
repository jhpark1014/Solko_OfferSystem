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
using System.Drawing;

public partial class OS_CampaignMailList : System.Web.UI.Page
{
    static string _pageConstDataTable = "PageKey_GridDt";
    private static readonly HttpClient client = new HttpClient();
    static string mInternalPrefix = "(내부 테스트용) ";

    // CRM using 
    CRMService.CRMService crmService = new CRMService.CRMService();

    protected void Page_Load(object sender, EventArgs e)
    {

        if (!Page.IsPostBack)
        {
            if (IsTransferedOtherPage("FROMMAIN", "T"))
            {
                string caid = GetValue_GetMethod("activityid");
                string subj = GetValue_GetMethod("subject");
                textCAID.Text = caid;
                textSUBJ.Text = subj;

                ReloadDataAndPrintOutPage(caid, true);
            }
        }
    }

    protected void FindEmails(string caid)
    {
        DataSet ds = CCommon.decompressDS(crmService.CRM_FindScheduledMail_From_CampaignActivity(caid));

        CSimpleDS griddata = new CSimpleDS();
        for (int i = 0; i < CCommon.getDSCount(ds); i++)
        {
            griddata.SetValue(i, "no", (i + 1).ToString());
            griddata.SetValue(i, "guid", CCommon.getDSValueAsString(ds, i, "activityid"));
            griddata.SetValue(i, "거래처", CCommon.getDSValueAsString(ds, i, "COMP.name"));
            griddata.SetValue(i, "사이트", CCommon.getDSValueAsString(ds, i, "COMP.new_txt_place"));
            griddata.SetValue(i, "제목", CCommon.getDSValueAsString(ds, i, "new_send_key_title"));
            griddata.SetValue(i, "기술지원갯수", CCommon.getDSValueAsString(ds, i, "new_support_tech"));
            griddata.SetValue(i, "RN담당", CCommon.getDSValueAsString(ds, i, "RNUSER.lastname") + CCommon.getDSValueAsString(ds, i, "RNUSER.firstname"));
            griddata.SetValue(i, "예약발송일", GetDateSimpleFormat(CCommon.getDSValueAsString(ds, i, "new_will_date")));
            griddata.SetValue(i, "진행상태", CCommon.getDSValueAsString(ds, i, "new_sendstatus"));

            //bool b1 = bool.Parse(CCommon.getDSValueAsString(ds, i, "new_will_send"));
            //griddata.SetValue(i, "발송예정", b1 ? "발송예정":"발송안함");

            //bool b2 = bool.Parse(CCommon.getDSValueAsString(ds, i, "new_email_send_result"));
            //griddata.SetValue(i, "발송완료", b2 ? "발송완료" : "미발송");

            string simpleMsg = CCommon.getDSValueAsString(ds, i, "new_desc_summary");
            //simpleMsg = simpleMsg.Replace("<br />", "\r\n");
            griddata.SetValue(i, "간략보기", simpleMsg);
            griddata.SetValue(i, "CRM보기", GetCRMEntityUrl("email", CCommon.getDSValueAsString(ds, i, "activityid")));

        }
        Session[GetPageSessionKey(_pageConstDataTable, caid)] = griddata.GetTable();

        GridView1.DataSource = Session[GetPageSessionKey(_pageConstDataTable, caid)];
        GridView1.DataBind();

    }

    protected string GetPageSessionKey(string keyPrefix, string caid)
    {
        return string.Format("{0}_{1}", keyPrefix, caid);
    }

    protected void GridView1_OnRowCommand(object sender, GridViewCommandEventArgs e)
    {
        // 메일발송 상태 변경하기
        if (e.CommandName.Equals("ChangeWill", StringComparison.OrdinalIgnoreCase))
            OnClick_ChangeWill((string)e.CommandArgument);

        else if (e.CommandName.Equals("SendNowOne", StringComparison.OrdinalIgnoreCase))
            OnClick_SendMailOneExternal((string)e.CommandArgument);

        else if(e.CommandName.Equals("SendNowOneInternal", StringComparison.OrdinalIgnoreCase))
            OnClick_SendMailOneInternal((string)e.CommandArgument);

        /*
        if (e.CommandName != "ChangeWill") return;

        string guid = (string)e.CommandArgument;
        if (string.IsNullOrEmpty(guid))
            return;

        string caid = textCAID.Text;

        // change send will 
        //crmService.CRM_UpdateData("email", "activityid", guid, "new_will_send", "bool", "-1");
        //string updateResult = crmService.CRM_UpdateData_Toggle("email", "activityid", guid, "new_will_send");
        string updateResult = crmService.CRM_UpdateData_Toggle("email", "activityid", guid, "new_sendstatus");
        if (!string.IsNullOrEmpty(updateResult) )
        {

            if("ERROR".Equals(updateResult, StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {

                try
                {

                    if(Session[GetPageSessionKey(_pageConstDataTable, caid)] != null)
                    {
                        DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
                        for(int i=0; i<CCommon.getDTCount(dt); i++)
                        {
                            string id = CCommon.getDTValueAsString(dt, i, "guid");
                            if( guid.Equals(id) )
                            {
                                CCommon.setDTValue(dt, i, "진행상태", updateResult);
                                break;
                            }
                        }

                        // refresh
                        ReloadDataAndPrintOutPage(caid);

                        // 모래시계 닫기
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closeMask1", "CloseLoadingWithMask()", true);
                    }
                    else
                    {
                        Page_Load(this, new EventArgs());
                    }
                }
                catch { }

                // bool type 
                //try
                //{
                //    bool res = bool.Parse(updateResult);

                //    if (Session[GetPageSessionKey(_pageConstDataTable, caid)] != null)
                //    {
                //        DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
                //        for (int i = 0; i < CCommon.getDTCount(dt); i++)
                //        {
                //            string id = CCommon.getDTValueAsString(dt, i, "guid");
                //            if (guid.Equals(id))
                //            {
                //                CCommon.setDTValue(dt, i, "발송예정", res ? "발송예정" : "발송안함");
                //                break;
                //            }
                //        }

                //        // refresh
                //        ReloadDataAndPrintOutPage(caid);

                //        // 모래시계 닫기
                //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closeMask1", "CloseLoadingWithMask()", true);
                //    }
                //    else
                //    {
                //        Page_Load(this, new EventArgs());
                //    }
                //}
                //catch { }

            }
        }
        */
    }

    
    protected void OnClick_SendMailOneInternal(string guid)
    {
        // guid : email guid

        //string guid = (string)e.CommandArgument;
        if (string.IsNullOrEmpty(guid))
            return;

        string result = crmService.CRM_SendMail_Internal(guid, mInternalPrefix);
        
        if(result.StartsWith("OK"))
        {
            // 완료메시지 및 모래시계 닫기
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "mailmsg1", "CloseLoadingWithMask(); alert('메일발송 완료'); ", true);
        }
        else
        {
            // 완료메시지 및 모래시계 닫기
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "mailmsg1", "CloseLoadingWithMask(); alert('" + result + "'); ", true);
        }
    }

    protected void OnClick_SendMailOneExternal(string guid)
    { // 실제 외부에 발송하기 

        // guid : email guid

        //string guid = (string)e.CommandArgument;
        if (string.IsNullOrEmpty(guid))
            return;

        string result = crmService.CRM_SendMail_External(guid, "");

        if (result.StartsWith("OK"))
        {
            // 완료메시지 및 모래시계 닫기
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "mailmsg1", "CloseLoadingWithMask(); alert('메일발송 완료'); ", true);
        }
        else
        {
            // 완료메시지 및 모래시계 닫기
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "mailmsg1", "CloseLoadingWithMask(); alert('" + result + "'); ", true);
        }
    }

    protected void OnClick_ChangeWill(string guid)
    {
        // guid : email guid

        //string guid = (string)e.CommandArgument;
        if (string.IsNullOrEmpty(guid))
            return;

        string caid = textCAID.Text;

        // change send will 
        //crmService.CRM_UpdateData("email", "activityid", guid, "new_will_send", "bool", "-1");
        //string updateResult = crmService.CRM_UpdateData_Toggle("email", "activityid", guid, "new_will_send");
        string updateResult = crmService.CRM_UpdateData_Toggle("email", "activityid", guid, "new_sendstatus");
        if (!string.IsNullOrEmpty(updateResult))
        {

            if ("ERROR".Equals(updateResult, StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {

                try
                {

                    if (Session[GetPageSessionKey(_pageConstDataTable, caid)] != null)
                    {
                        DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
                        for (int i = 0; i < CCommon.getDTCount(dt); i++)
                        {
                            string id = CCommon.getDTValueAsString(dt, i, "guid");
                            if (guid.Equals(id))
                            {
                                CCommon.setDTValue(dt, i, "진행상태", updateResult);
                                break;
                            }
                        }

                        // refresh
                        ReloadDataAndPrintOutPage(caid);

                        // 모래시계 닫기
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closeMask1", "CloseLoadingWithMask()", true);
                    }
                    else
                    {
                        Page_Load(this, new EventArgs());
                    }
                }
                catch { }

                // bool type 
                //try
                //{
                //    bool res = bool.Parse(updateResult);

                //    if (Session[GetPageSessionKey(_pageConstDataTable, caid)] != null)
                //    {
                //        DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
                //        for (int i = 0; i < CCommon.getDTCount(dt); i++)
                //        {
                //            string id = CCommon.getDTValueAsString(dt, i, "guid");
                //            if (guid.Equals(id))
                //            {
                //                CCommon.setDTValue(dt, i, "발송예정", res ? "발송예정" : "발송안함");
                //                break;
                //            }
                //        }

                //        // refresh
                //        ReloadDataAndPrintOutPage(caid);

                //        // 모래시계 닫기
                //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closeMask1", "CloseLoadingWithMask()", true);
                //    }
                //    else
                //    {
                //        Page_Load(this, new EventArgs());
                //    }
                //}
                //catch { }

            }
        }
    }

    protected void ReloadDataAndPrintOutPage(string caid, bool forceUpdate = false)
    {
        if(forceUpdate)
        {
            // caid로 부터 campaign의 하위 email을 검색하여 출력한다.
            FindEmails(caid);
        }
        else
        {
            if (Session[GetPageSessionKey(_pageConstDataTable, caid)] == null)
                return;

            GridView1.DataSource = Session[GetPageSessionKey(_pageConstDataTable, caid)];
            GridView1.DataBind();

        }
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

    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        string caid = textCAID.Text;
        if( !string.IsNullOrEmpty(caid) )
        {
            if (Session[GetPageSessionKey(_pageConstDataTable, caid)] != null)
            {
                GridView1.DataSource = Session[GetPageSessionKey(_pageConstDataTable, caid)];
                GridView1.DataBind();
            }
            else
            {
                FindEmails(caid);
            }
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

    protected string GetKey_CampaignActivity(string sKey1)
    {
        string sKey2 = string.Format("YLC_info");

        return string.Format("solko_{0}_{1}", sKey1, sKey2);
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

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // 발송여부 grid 버튼의 색상변경 처리
            Button myLinkButton = (Button)e.Row.FindControl("buttonChangeWill") as Button;
            string sendWill = myLinkButton.Text;

            if ("초안".Equals(sendWill))
                myLinkButton.BackColor = Color.Yellow;
            else if ("검토됨-발송준비됨".Equals(sendWill))
                myLinkButton.BackColor = Color.LightGreen;
            else if ("발송완료".Equals(sendWill))
                myLinkButton.BackColor = Color.LightBlue;
            else if ("발송실패".Equals(sendWill))
                myLinkButton.BackColor = Color.OrangeRed;
            else
                myLinkButton.BackColor = Color.IndianRed;

            // 

            //if ("발송예정".Equals(sendWill))
            //    myLinkButton.BackColor = Color.GreenYellow;
            //else
            //    myLinkButton.BackColor = Color.IndianRed;
        }
    }


    protected void btnSendMailNow_Click(object sender, EventArgs e)
    {
        string caid = textCAID.Text;
        if( !string.IsNullOrEmpty(caid) )
        {

            try
            {
                string result = crmService.CRM_SendMail_using_CampaignActivity(caid);
                if ("NO".Equals(result))
                {
                    msgb("CRM 연결이 되지않아 메일발송에 실패하였습니다.");
                }
                else
                {
                    string[] tt = result.Split(CConst.splitSemiColon);
                    string[] okCnt = tt[0].Split(CConst.splitColon);
                    string[] erCnt = tt[1].Split(CConst.splitColon);


                    msgb(string.Format("메일발송 성공:{0}건, 실패:{1}건", okCnt[1], erCnt[1]));
                }
            }
            catch(Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('메일발송에 실패하였습니다')", true);
            }
        }
        else
        {
            msgb("메일발송을 위한 캠페인 활동 ID를 찾지 못했습니다.");
        }

        Page_Load(this, new EventArgs());
    }


    protected void btnSendMailNow2_Click(object sender, EventArgs e)
    {
        // 직접 메일발송 처리하기

        string caid = textCAID.Text;
        if (!string.IsNullOrEmpty(caid))
        {

            try
            {
                string sds = crmService.CRM_SendScheduledMail_using_CampaignActivity(caid);
                DataSet ds = CCommon.decompressDS(sds);

                int okCount = 0;
                int errCount = 0;
                for (int i = 0; i < CCommon.getDSCount(ds); i++)
                {
                    string rt = CCommon.getDSValueAsString(ds, i, "SENDRESULT");
                    if( "OK".Equals(rt) )
                    {
                        okCount++;
                    }
                    else
                    {
                        errCount++;
                    }
                    //string s1 = CCommon.getDSValueAsString(ds, i, "activityid");
                    //string s2 = CCommon.getDSValueAsString(ds, i, "new_will_send");
                    //string s3 = CCommon.getDSValueAsString(ds, i, "new_will_date");
                    //string s4 = CCommon.getDSValueAsString(ds, i, "new_email_send_result");

                    //if ("TRUE".Equals(s2.ToUpper()) && "FALSE".Equals(s4.ToUpper()))
                    //{
                    //    string s5 = CCommon.getDSValueAsString(ds, i, "new_send_key");
                    //    string s6 = CCommon.getDSValueAsString(ds, i, "subject");
                    //    string s7 = CCommon.getDSValueAsString(ds, i, "description");

                    //    string s8 = CCommon.getDSValueAsString(ds, i, "from");
                    //    string s9 = CCommon.getDSValueAsString(ds, i, "to");
                    //    string s10 = CCommon.getDSValueAsString(ds, i, "cc");
                    //    string s11 = CCommon.getDSValueAsString(ds, i, "bcc");

                    //    if (sendMail_V5(s8, s9, s10, s11, s6, s7))
                    //    {
                    //        // success 
                    //        okCount++;
                    //        crmService.CRM_UpdateData("email", "activityid", s1, "new_email_send_result", "bool", "true");
                    //    }
                    //    else
                    //    {
                    //        // fail 
                    //        errCount++;
                    //        crmService.CRM_UpdateData("email", "activityid", s1, "new_email_send_result", "bool", "false");
                    //    }
                    //}
                }

                msgb(string.Format("메일발송 성공:{0}건,  실패:{1}건", okCount, errCount));


                //if ("NO".Equals(result))
                //{
                //    msgb("CRM 연결이 되지않아 메일발송에 실패하였습니다.");
                //}
                //else
                //{
                //    string[] tt = result.Split(CConst.splitSemiColon);
                //    string[] okCnt = tt[0].Split(CConst.splitColon);
                //    string[] erCnt = tt[1].Split(CConst.splitColon);


                //    msgb(string.Format("메일발송 성공 : {0}건 <br/>메일발송 실패 : {1}건", okCnt[1], erCnt[1]));
                //}
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('메일발송에 실패하였습니다')", true);
            }
        }
        else
        {
            msgb("메일발송을 위한 캠페인 활동 ID를 찾지 못했습니다.");
        }

        Page_Load(this, new EventArgs());
    }

    protected void btnSendMailInternalAll_Click(object sender, EventArgs e)
    {
        // 고객사가 아닌, SOLKO 내부에 메일 발송하기

        string caid = textCAID.Text;
        if (!string.IsNullOrEmpty(caid))
        {
            try
            {
                string sds = crmService.CRM_SendScheduledMail_Internal_using_CampaignActivity(caid, "ALL", mInternalPrefix);
                DataSet ds = CCommon.decompressDS(sds);

                int okCount = 0;
                int errCount = 0;
                for (int i = 0; i < CCommon.getDSCount(ds); i++)
                {
                    string rt = CCommon.getDSValueAsString(ds, i, "SENDRESULT");
                    if ("OK".Equals(rt))
                    {
                        okCount++;
                    }
                    else
                    {
                        errCount++;
                    }
                }

                msgb(string.Format("메일발송 성공:{0}건,  실패:{1}건", okCount, errCount));
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('메일발송에 실패하였습니다')", true);
            }
        }
        else
        {
            msgb("메일발송을 위한 캠페인 활동 ID를 찾지 못했습니다.");
        }

        Page_Load(this, new EventArgs());
    }

    protected void btnSendMailInternal_Click(object sender, EventArgs e)
    {
        // 고객사가 아닌, SOLKO 내부에 메일 발송하기

        string caid = textCAID.Text;
        if (!string.IsNullOrEmpty(caid))
        {
            try
            {
                string sds = crmService.CRM_SendScheduledMail_Internal_using_CampaignActivity(caid, "READY", mInternalPrefix);
                DataSet ds = CCommon.decompressDS(sds);

                int okCount = 0;
                int errCount = 0;
                for (int i = 0; i < CCommon.getDSCount(ds); i++)
                {
                    string rt = CCommon.getDSValueAsString(ds, i, "SENDRESULT");
                    if ("OK".Equals(rt))
                    {
                        okCount++;
                    }
                    else
                    {
                        errCount++;
                    }
                }

                msgb(string.Format("메일발송 성공:{0}건,  실패:{1}건", okCount, errCount));
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('메일발송에 실패하였습니다')", true);
            }
        }
        else
        {
            msgb("메일발송을 위한 캠페인 활동 ID를 찾지 못했습니다.");
        }

        Page_Load(this, new EventArgs());
    }



    protected void btnExcelExport_Click(object sender, EventArgs e)
    { // 엑셀 저장

        string caid = textCAID.Text;
        if (string.IsNullOrEmpty(caid))
            return;

        DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
        if (dt == null)
            return;

        if (CCommon.getDTCount(dt) <= 0)
            return;

        string caSubject = textSUBJ.Text;


        List<string> colList = new List<string>();
        colList.Add("guid");
        colList.Add("거래처");
        colList.Add("사이트");
        colList.Add("기술지원갯수");
        colList.Add("RN담당");
        colList.Add("예약발송일");
        colList.Add("진행상태");
        colList.Add("CRM보기");
        colList.Add("간략보기");

        // 엑셀로 내보내기
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                try
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets.Add(caSubject);

                    DataSet DS = new DataSet();
                    DS.Tables.Add(dt.Copy());

                    int dscnt = CCommon.getDSCount(DS);
                    int lineCount = 0;

                    // 컬럼 정의 출력 
                    {
                        lineCount++;
                        sheet.InsertRow(lineCount, 1, lineCount + 1);   // 행 추가. 직전 행을 양식을 복사 함.
                        int nColCount = 0;  // 개발 후 엑셀양식이 변경될 경우. 아래에 순서만 맞추면 됨.

                        foreach (string col in colList)
                        {
                            nColCount++;

                            if ("guid".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = "관리번호";
                                sheet.Column(nColCount).Width = 10;
                            }
                            else if ("거래처".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 40;
                            }
                            else if ("사이트".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 30;
                            }
                            else if ("제목".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 60;
                            }
                            else if ("기술지원갯수".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 18;
                            }
                            else if ("RN담당".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 10;
                            }
                            else if ("예약발송일".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 15;
                            }
                            else if ("진행상태".Equals(col))
                            {
                                // 편집가능
                                sheet.Cells[lineCount, nColCount].Value = col;
                                // sheet.Cells[lineCount, nColCount].AddComment(string.Format("전송할 것 : 발송true, yes, 예, 전송\r\n전송하지 말 것 : false, no, 아니오, 비전송"), "system");
                                //sheet.Cells[lineCount, nColCount].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                //sheet.Cells[lineCount, nColCount].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                                sheet.Column(nColCount).Width = 10;
                            }
                            else if ("CRM보기".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 20;
                            }
                            else if ("간략보기".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                                sheet.Column(nColCount).Width = 100;
                            }
                            else
                            {
                                sheet.Cells[lineCount, nColCount].Value = col;
                            }
                        }
                    }


                    // 데이터 영역
                    for (int i = 0; i < dscnt; i++)
                    {
                        lineCount++;
                        sheet.InsertRow(lineCount, 1, lineCount + 1);   // 행 추가. 직전 행을 양식을 복사 함.

                        int nColCount = 0;  // 개발 후 엑셀양식이 변경될 경우. 아래에 순서만 맞추면 됨.
                        foreach (string col in colList)
                        {
                            nColCount++;

                            if ("예약발송일".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Style.Numberformat.Format = "yyyy-mm-dd";
                                sheet.Cells[lineCount, nColCount].Value = DateTime.Parse(CCommon.getDSValueAsString(DS, i, col));
                            }
                            else if ("진행상태".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Value = CCommon.getDSValueAsString(DS, i, col);

                                sheet.Cells[lineCount, nColCount].AddComment(string.Format("발송할 것 : 발송\r\n발송하지 말 것 : 발송안함, 미발송, 비발송"), "system");
                                sheet.Cells[lineCount, nColCount].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                sheet.Cells[lineCount, nColCount].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                            }
                            else if ("CRM보기".Equals(col))
                            {
                                sheet.Cells[lineCount, nColCount].Hyperlink = new Uri(CCommon.getDSValueAsString(DS, i, col));
                                sheet.Cells[lineCount, nColCount].Style.Font.Color.SetColor(Color.Blue);
                                sheet.Cells[lineCount, nColCount].Style.Font.UnderLine = true;
                                sheet.Cells[lineCount, nColCount].Value = "CRM 데이터보기";
                            }
                            else
                            {
                                sheet.Cells[lineCount, nColCount].Value = CCommon.getDSValueAsString(DS, i, col);
                            }
                        }
                    }

                    
                    // 파일 다운로드 지원 
                    {
                        string excelName = string.Format("재계약메일안내_{0}_{1}", caSubject, DateTime.Now.ToLongDateString());
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment; filename=" + excelName + ".xlsx");
                            package.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }

                    }
                }
                catch (Exception ex)
                {
                    msgb(ex.Message);
                }
            }
        }

    }

    private bool GetBool(string yesno)
    {
        if ("true,True,TRUE,yes,YES,Yes,예,전송,발송,발송함,발송예정,".Contains(yesno))
            return true;
        else
            return false;
    }

    private int GetInt(string yesno)
    {
        if ("초안".Equals(yesno))
            return 100000000;
        else if ("true,True,TRUE,yes,YES,Yes,예,전송,발송,발송함,발송예정,검토됨-발송준비됨".Contains(yesno))
            return 100000001;
        else
            return 100000002;
    }

    private string excel_GetValue_EPP(ExcelWorksheet worksheet, int r1, int c1)
    {
        int colCount = worksheet.Dimension.End.Column;  //get Column Count
        int rowCount = worksheet.Dimension.End.Row;     //get row count

        if (r1 > rowCount) return "";
        if (c1 > colCount) return "";

        return worksheet.Cells[r1, c1].Text + "";

        //Microsoft.Office.Interop.Excel.Range cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[r1, c1];
        //return cell.Value + "";
    }

    protected void btnExcelImport_Click(object sender, EventArgs e)
    { // 엑셀 읽기 


        if (this.fileUpload.HasFile)
        {
            string caid = textCAID.Text;

            if (Session[GetPageSessionKey(_pageConstDataTable, caid)] == null)
                return;


            using (ExcelPackage package = new ExcelPackage())
            {
                package.Load(this.fileUpload.FileContent);

                try
                {
                    int skipList = 0;
                    int updateList = 0;

                    for (int ei = 1; ei <= package.Workbook.Worksheets.Count; ei++)
                    {
                        if (ei != 1) break;

                        ExcelWorksheet worksheet = package.Workbook.Worksheets[ei];

                        int rows = worksheet.Dimension.Rows;
                        int cols = worksheet.Dimension.Columns;


                        List<string> colNames = new List<string>();
                        Dictionary<string, int> colDic = new Dictionary<string, int>();
                        {
                            // 헤더 인덱수 수집 : 관리번호, 발송예정 
                            for (int j = 1; j < cols; j++)
                            {
                                string getString = excel_GetValue_EPP(worksheet, 1, j);
                                if (getString.Length < 1)
                                {
                                    break;
                                }

                                colNames.Add(getString);
                            }

                            for (int i = 0; i < colNames.Count; i++)
                            {
                                string colName = colNames[i];
                                if ("관리번호".Equals(colName))
                                {
                                    colDic.Add(colName, i + 1);
                                }
                                else if ("진행상태".Equals(colName))
                                {
                                    colDic.Add(colName, i + 1);
                                }
                                else if ("거래처".Equals(colName))
                                {
                                    colDic.Add(colName, i + 1);
                                }
                            }
                        }


                        for (int i = 2; i < rows; i++)
                        {
                            string guid = excel_GetValue_EPP(worksheet, i, colDic["관리번호"]);
                            string yesno = excel_GetValue_EPP(worksheet, i, colDic["진행상태"]);
                            string compName = excel_GetValue_EPP(worksheet, i, colDic["거래처"]);

                            if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(yesno))
                            {
                                if ("true,True,TRUE,yes,YES,Yes,예,발송,false,FALSE,False,no,NO,No,초안,아니오,아니요,미발송,비발송,발송예정,발송함,발송안함,발송 안 함,발송 안함,안함,검토됨-발송준비됨,검토됨-발송안함".Contains(yesno))
                                {
                                    int yn = GetInt(yesno);

                                    int curYN = 100000000;
                                    DataTable dt = (DataTable)Session[GetPageSessionKey(_pageConstDataTable, caid)];
                                    for (int j = 0; j < CCommon.getDTCount(dt); j++)
                                    {
                                        string id = CCommon.getDTValueAsString(dt, j, "guid");
                                        if (guid.Equals(id))
                                        {
                                            curYN = GetInt(CCommon.getDTValueAsString(dt, j, "진행상태"));
                                            break;
                                        }
                                    }

                                    if (curYN == yn)
                                    {
                                        skipList++;
                                    }
                                    else
                                    {
                                        updateList++;
                                        crmService.CRM_UpdateData("email", "activityid", guid, "new_sendstatus", "optionSet", yn.ToString());
                                    }
                                }
                                else
                                {
                                    msgb(string.Format("{0}은 진행상태에 사용가능한 값이 아닙니다.", yesno));
                                }
                            }

                        }
                    } // for sheetCount

                    if(updateList > 0)
                    {
                        msgb("Excel 데이터로 CRM에 업데이트 되었습니다\r\n" + string.Format("  업데이트 : {0}건", updateList));
                    }

                    // refresh
                    ReloadDataAndPrintOutPage(caid, true);

                }
                catch (Exception ex)
                {
                    msgb(ex.Message);
                }
            }
        }
        else
        {
            msgb("파일을 먼저 선택 후 다시 실행해 주세요.");
        }

    }
}

