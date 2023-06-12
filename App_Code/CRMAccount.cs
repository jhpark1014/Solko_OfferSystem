using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CRMAccount
/// </summary>
public class CRMAccount
{
    public string guid = "";
    public string AccountID = "";
    public string CompanyName = "";
    public string SiteName = "";
    public string Grade = "";
    public string Address = "";
    public string TEL = "";
    public string FAX = "";
    public string techCount = "0";
    public string rnUserID = "";

    public List<CRMLicense> LicenseList = new List<CRMLicense>();
    public List<CRMLicense> RemainLicenseList = new List<CRMLicense>();  // 올해 내 계약필요 항목들
    public List<CRMLicense> MissLicenseList = new List<CRMLicense>();    // 3년 이내 미갱신 라이센스

    public string emailid = "";
    public bool sendWill = true;
    public string sendDate = "";


    public CRMAccount(string _guid, string _accountid, string _companyName, string _siteName, string _grade, string _address, string _tel, string _fax, string rnUserID)
    {
        this.guid = _guid;
        this.AccountID = _accountid;
        this.CompanyName = _companyName;
        this.SiteName = _siteName;
        this.Grade = _grade;

        this.Address = _address;
        this.TEL = _tel;
        this.FAX = _fax;
        this.rnUserID = rnUserID;
    }
}