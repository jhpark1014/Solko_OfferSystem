using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CRMLicense
/// </summary>
public class CRMLicense
{
    public string ProductName = "";
    public string CategoryName = "";
    public string SerialNo = "";
    public string sDateInstall = "";
    public string sDateExpire = "";
    public string sDateEnd = "";
    public string LicType = "";

    public int Qty = 0;

    //public DateTime DateInstall;
    public DateTime DateExpire;
    public DateTime DateEnd;

    public CRMLicense(string _productName, string _cateName, string _serial, string _dateInstall, string _dateEnd, string _dateExpire, string _qty, string _lictype)
    {
        this.ProductName = _productName;
        this.CategoryName = _cateName;
        this.SerialNo = _serial;
        this.sDateInstall = _dateInstall;
        this.sDateEnd = _dateEnd;
        this.sDateExpire = _dateExpire;
        this.LicType = _lictype;

        this.Qty = int.Parse(_qty);

        //DateInstall = DateTime.Parse(sDateInstall);
        DateExpire = DateTime.Parse(sDateExpire);
        DateEnd = DateTime.Parse(sDateEnd);
    }

    public string GetSerialMozaic()
    {
        if (string.IsNullOrEmpty(this.SerialNo))
        {
            return "";
        }

        if (this.SerialNo.Length > 15)
        {
            string t2 = string.Format("{0}*******{1}", this.SerialNo.Substring(0, 4), this.SerialNo.Substring(this.SerialNo.Length - 6));
            return t2;
        }
        else
        {
            string t2 = string.Format("{0}", this.SerialNo);
            return t2;
        }
    }

    public int GetRemainDay(DateTime checkDate)
    {
        //return (int)DateExpire.Subtract(checkDate).TotalDays;
        return (int)DateEnd.Subtract(checkDate).TotalDays;
    }
}