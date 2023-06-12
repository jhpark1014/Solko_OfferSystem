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

public partial class RequestList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataAdapter da = new SqlDataAdapter(new SqlCommand("SELECT * FROM Request WHERE Agree=1 AND Deleted=0  AND Granted=0 ORDER BY ReqDate DESC;", dbConn));
        DataSet ds = new DataSet();
        da.Fill(ds);
        GridView1.DataSource = ds;
        GridView1.DataBind();
        dbConn.Close();
        if (!hidemodalAdd.Checked)
        {
            string[] licDate = getLicDates(1);
            txtLicStart.Text = licDate[0];
            txtLicEnd.Text = licDate[1];
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
        txtswKey.Text = row.Cells[8].Text;

        txtprgName.Text = row.Cells[11].Text;
        txtprgVer.Text = row.Cells[12].Text;

        txtComment.Text = row.Cells[16].Text;


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

    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        GridView1.PageIndex = e.NewPageIndex;
        GridView1.DataBind();
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
        if((txtMail.Text + "").Length > 3)
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
}

