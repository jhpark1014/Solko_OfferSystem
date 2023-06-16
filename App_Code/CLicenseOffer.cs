using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CLicenseOffer
/// </summary>
public class CLicenseOffer
{
    public string CompanyName { get; }
    //public string SiteName { get; }
    public string ProductCategory { get; }
    public string ProductName { get; }
    public DateTime EndDate { get; } // 현재 Expire Date
    public int Quantity { get; }

    public long Cost { get; }

    public long CostDollar { get; }

    public CLicenseOffer(string companyName, string productCategory, string productName, string endDate, string quantity, long cost, long costDollar)
    {
        this.CompanyName = companyName;
        //this.SiteName = siteName;
        this.ProductCategory = productCategory;
        this.ProductName = productName;

        this.EndDate = String2DateTime(endDate);
        this.Quantity = String2Integer(quantity);

        this.Cost = cost;
        this.CostDollar = costDollar;
    }

    static public int String2Integer(string str)
    {
        return int.Parse(str);
    }

    static public DateTime String2DateTime(string date)
    {
        String[] datesplit = date.Split('/');

        DateTime date1 = new DateTime(int.Parse(datesplit[0]), int.Parse(datesplit[1]), 1);
        //DateTime dateNext = date1.AddYears(1);

        DateTime dateNextMonthLastDay = new DateTime(date1.Year, date1.Month, DateTime.DaysInMonth(date1.Year, date1.Month));

        //DateTime endDate = DateTime.ParseExact(datesplit[0] + datesplit[1] + DateTime.DaysInMonth(Convert.ToInt32(datesplit[0]) + 1, Convert.ToInt32(datesplit[1])), "yyyyMMdd", null);

        return dateNextMonthLastDay;
    }

    public int GetBackdatingAsMonth(DateTime offerDate)
    {
        // "BackDate" Column
            //String[] datesplit = (row["new_dt_end"].ToString()).Split('/');
            //string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
            //DateTime endDate = DateTime.ParseExact(date, "yyyy/MM/dd", null);

            //int backdate = CalcBackDate(endDate);
            int backdate = CalcBackDate(this.EndDate, offerDate);
            if (backdate >= 36)
            {
                backdate = 36;
            }

            return backdate;
    }

    public int GetForwardAsMonth(DateTime offerStartDate, DateTime offerEndDate)
    {
        //string datenow = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        //DateTime expireDate = DateTime.ParseExact(datenow, "yyyy/M/dd", null);

        //// 새 견적종료일
        //string[] newexpdate = newExpire.Text.Split('.');
        //string newdate = newExpire.Text + "." + DateTime.DaysInMonth(Convert.ToInt32(newexpdate[0]), Convert.ToInt32(newexpdate[1]));
        //DateTime newExpireDate = DateTime.ParseExact(newdate, "yyyy.MM.dd", null);

        //// 기존 견적종료일
        //String[] datesplit = (row["new_dt_end"].ToString()).Split('/');
        //string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
        //DateTime endDate = DateTime.ParseExact(date, "yyyy/MM/dd", null);


        // endDate == license ExpireDate 임
        if (this.EndDate > offerStartDate)
        { // 견적시작일 보다 라이센스 종료일이 후(나중)인 경우에는, 견적기간의 시작을 라이센스 종료일(end date)로 하여 기간을 계산한다 
            return CalcForwardDate(offerEndDate, this.EndDate);
        }
        else
        {
            return CalcForwardDate(offerEndDate, offerStartDate);
        }
    }


    public string GetBackdatingAsDescription(DateTime offerDate)
    {
        /*
          SOLIDWORKS Professional Network Service Renewal - Backdating
          ( 2022. 7.1 ~ 2022. 7.31 ) - 1개월
         */

        int month = this.GetBackdatingAsMonth(offerDate);

        if (month >= 1)
        {
            DateTime d2 = this.EndDate.AddMonths(1);
            //DateTime d3 = CutOff36(d2, offerDate);
            string dateFrom = string.Format("{0}.{1}.1", d2.Year, d2.Month);
            string dateUntil = string.Format("{0}.{1}.{2}", offerDate.Year, offerDate.Month, DateTime.DaysInMonth(offerDate.Year, offerDate.Month));

            string result = string.Format("BackDating ({0} ~ {1}) = {2}개월", dateFrom, dateUntil, month);
            return result;
        }
        else
        {
            string result = "BackDating = 0";
            return result;
        }

    }
    public string GetForwardAsDescription(DateTime offerStartDate, DateTime offerEndDate)
    {
        /*
          SOLIDWORKS Professional Network Service Renewal - 1 Year
          ( 2022. 8.1 ~ 2023. 7.31 )
         */

        // 견적종료일이 아직 지나지 않음
        if (this.EndDate > offerStartDate)
        {
            DateTime d2 = this.EndDate.AddMonths(1);
            string dateFrom = string.Format("{0}.{1}.1", d2.Year, d2.Month);
            string dateUntil = string.Format("{0}.{1}.{2}", offerEndDate.Year, offerEndDate.Month, DateTime.DaysInMonth(offerEndDate.Year, offerEndDate.Month));
            int month = this.GetForwardAsMonth(offerStartDate, offerEndDate);

            string result = string.Format("Forwarding ({0} ~ {1}) = {2}개월", dateFrom, dateUntil, month);
            return result;
        }
        // 지났음
        else
        {
            DateTime d2 = offerStartDate.AddMonths(1);
            string dateFrom = string.Format("{0}.{1}.1", d2.Year, d2.Month);
            string dateUntil = string.Format("{0}.{1}.{2}", offerEndDate.Year, offerEndDate.Month, DateTime.DaysInMonth(offerEndDate.Year, offerEndDate.Month));
            int month = this.GetForwardAsMonth(offerStartDate, offerEndDate);

            string result = string.Format("Forwarding ({0} ~ {1}) = {2}개월", dateFrom, dateUntil, month);
            return result;

            //forwarddata = "Forwarding (" + datesplit[0] + "." + (Convert.ToInt32(datesplit[1]) + 1) + ".01 ~ " + newdates[0] + "." + newdates[1] + "."
            //+ DateTime.DaysInMonth(Convert.ToInt32(newdates[0]) + 1, Convert.ToInt32(newdates[1])) + ") = " + row["Forward"] + "개월";
        }
    }

    // (back) 기간 * 단가 ==> List Price
    public long GetPrice_BackCost(DateTime offerDate)
    {
        int dueMonth = GetBackdatingAsMonth(offerDate);
        //long back = RoundingCut1000(this.Cost / 12 * dueMonth);
        long back = (long)(Math.Round((double)this.Cost / 12 * dueMonth));
        return back;
    }

    // (forward) 기간 * 단가 ==> List Price
    public long GetPrice_ForwardCost(DateTime offerStartDate, DateTime offerEndDate)
    {
        int dueMonth = GetForwardAsMonth(offerStartDate, offerEndDate);
        //long fwd = RoundingCut1000(this.Cost / 12 * dueMonth);
        long fwd = (long)(Math.Round((double) this.Cost / 12 * dueMonth));
        return fwd;
    }

    // 단가 * 할인율 ==> 제안단가
    public long GetPrice_DiscountedCost(long cost, double discountPercent)
    {
        double price1 = (double)cost * (1.0 - discountPercent * 0.01);
        long price1000 = FloorCut1000(price1);
        return price1000;
    }


    // ( back 할인가 + foward 할인가 ) * 수량 ==> 합계견적금액 
    public long GetPrice_Amount(long priceSum)
    {
        double price1 = (double)priceSum * (double)this.Quantity;
        long price1000 = RoundingCut1000(price1);
        return price1000;
    }

    public double GetPrice_BackDollar(DateTime offerDate)
    {
        int dueMonth = GetBackdatingAsMonth(offerDate);
        //long back = RoundingCut1000(this.Cost / 12 * dueMonth);
        double back = (double)(this.CostDollar / 12 * dueMonth);
        return back;
    }

    // (forward) 기간 * 단가 ==> List Price
    public double GetPrice_ForwardDollar(DateTime offerStartDate, DateTime offerEndDate)
    {
        int dueMonth = GetForwardAsMonth(offerStartDate, offerEndDate);
        //long fwd = RoundingCut1000(this.Cost / 12 * dueMonth);
        double fwd = (double)(this.CostDollar / 12 * dueMonth);
        return fwd;
    }

    //public double GetPrice_AmountDollar(double priceSum)
    //{
    //    double price1 = (double)priceSum * (double)this.Quantity;
    //    return price1;
    //}

    public double GetPrice_AmountDollar(double priceSum)
    {
        double price1 = (double)priceSum * (double)this.Quantity;
        return price1;
    }

    #region "INTERNAL API"

    private int CalcBackDate(DateTime expDate, DateTime offerDate)
    {
        // 3. backdating 계산
        //    3.1 견적시작월(또는 현재월)보다 라이센스 종료일이 작은 경우(이미 종료되었음) ==> 견적시작월 - 라이센스 종료월
        //    3.2 위 3.1 아닌 경우 무조건 0
        //    3.2 만약 backdating 계산결과가 36개월을 넘어서는 경우 36개월 까지로 라이센스 종료월을 36개월로 되도록 조정한다.

        int evalMonth = 0;

        if (expDate < offerDate)
        {
            // expDate : 2020/12/31   now : 2022/08/31   => 20 개월 
            evalMonth = (int)((offerDate.Year - expDate.Year) * 12) + offerDate.Month - expDate.Month;
        }

        return evalMonth;

        //if (expDate < DateTime.Now)
        //{
        //    // expDate : 2020/12/31   now : 2022/08/31   => 20 개월 
        //    return (int)((DateTime.Now.Year - expDate.Year) * 12) + DateTime.Now.Month - expDate.Month;
        //}
        //else
        //{
        //    return 0;
        //}
    }

    public override string ToString()
    {
        return this.ProductCategory + " " + this.ProductName;
    }

    private int CalcForwardDate(DateTime newExpDate, DateTime expDate)
    {
        return (int)((newExpDate.Year - expDate.Year) * 12) + newExpDate.Month - expDate.Month;
    }

    private long RoundingCut1000(double price)
    {
        //double v1 = double.Parse(price);

        double v2 = price * 0.001;

        double v3 = Math.Round(v2);

        long v4 = (long)v3;

        long v5 = v4 * 1000;

        return v5;
    }

    private long FloorCut1000(double price)
    {
        //double v1 = double.Parse(price);

        double v2 = price * 0.001;

        double v3 = Math.Floor(v2);

        long v4 = (long)v3;

        long v5 = v4 * 1000;

        return v5;
    }

    #endregion

}