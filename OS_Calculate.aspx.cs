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
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.SharePoint.Client;
using OfficeOpenXml;
using System.IO;
using System.Security;
using OfficeOpenXml.Style;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Drawing;
using OfficeOpenXml.Drawing;
using System.Data.Entity.Core.Mapping;
using Microsoft.AspNet.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using xPMWorksWeb;
using System.Runtime.Remoting.Contexts;

public partial class RequestList : System.Web.UI.Page
{
    //[STAThreadAttribute]
    private DataTable dt;
    private DataTable datas;
    static string _pageFilterKey = "Usage_FilterText";  // 페이지의 필터링데이터 키값
    static string _pageSortedKey = "Usage_SortedView";  // 페이지의 정렬데이터 키값

    // CRM using 
    Microsoft.Xrm.Sdk.IOrganizationService os = null;
    CrmServiceClient crmSvc = null;

    private DateTime mOfferDate = DateTime.Now;

    // Excel Design 상수선언

    ExcelBorderStyle lineThick = ExcelBorderStyle.Medium;

    ExcelBorderStyle lineThin = ExcelBorderStyle.Thin;

    ExcelBorderStyle lineHair = ExcelBorderStyle.Hair;

    ExcelVerticalAlignment verCenter = ExcelVerticalAlignment.Center;

    ExcelVerticalAlignment verBottom = ExcelVerticalAlignment.Bottom;

    ExcelVerticalAlignment verTop = ExcelVerticalAlignment.Top;

    ExcelHorizontalAlignment horCenter = ExcelHorizontalAlignment.CenterContinuous;

    ExcelHorizontalAlignment horLeft = ExcelHorizontalAlignment.Left;

    ExcelHorizontalAlignment horRight = ExcelHorizontalAlignment.Right;


    protected void Page_Load(object sender, EventArgs e)
    {
        string id_list = "";
        Debug.WriteLine("Calculate 페이지 로드됨");

        if (!Page.IsPostBack)
        {
            if (IsTransferedOtherPage("FROMMAIN", "T"))
            {
                id_list = (string)GetValue_Session("sendIDList");
                System.Diagnostics.Debug.WriteLine("다른 페이지에서 호출된 것임");
                //System.Diagnostics.Debug.WriteLine("id_list" + id_list);
                //System.Diagnostics.Debug.WriteLine("idlist length" + id_list.Length);
                //TextBox1.Text = "다른 페이지에서 호출된 것임";
                //TextBox2.Text = (string)GetValue_PostMethod("TextBox1");
                //TextBox3.Text = (string)GetValue_Session("Main_TextBox1");
            }
            else
            {
                id_list = "";
                System.Diagnostics.Debug.WriteLine("직접 열린 페이지");
                //System.Diagnostics.Debug.WriteLine(id_list.Length);
                //TextBox1.Text = "직접 열린 페이지";
                //TextBox2.Text = "";
                //TextBox3.Text = "";
            }

            companyName.Text = Session["companyName"].ToString();
            double exchangeRate = Convert.ToDouble(chgExchangeRate.Text);

            // session backup
            {
                Session["BK_sendIDList"] = id_list;
                Session["BK_companyName"] = companyName.Text;
            }


            // 단가표 읽어서 dictionary로 만들기

            Dictionary<string, long> priceByProductName = new Dictionary<string, long>();
            Dictionary<string, long> dollarByProductName = new Dictionary<string, long>();
            {

                Dictionary<string, long> costByXlsProductName = GetPriceCost("SOLKO_SW_PriceBook.xlsx");
                Dictionary<string, long> dollarByXlsProductName = GetPriceDollar("SOLKO_SW_PriceBook.xlsx");
                Dictionary<string, string> xlsProductNameByCrmProductName = GetPriceMap("SOLKO_SW_PriceBook.xlsx");
                foreach (KeyValuePair<string, string> xlsByCrm in xlsProductNameByCrmProductName)
                {
                    string crmName = xlsByCrm.Key;
                    string xlsName = xlsByCrm.Value;

                    if (!priceByProductName.ContainsKey(crmName))
                    {
                        if (costByXlsProductName.ContainsKey(xlsName))
                        {
                            priceByProductName.Add(crmName, costByXlsProductName[xlsName]);
                            dollarByProductName.Add(crmName, dollarByXlsProductName[xlsName]);
                        }
                    }
                }
            }


            Session["BK_priceBook"] = priceByProductName;
            Session["BK_dollarBook"] = dollarByProductName;


            // Calendar Default 날짜 설정
            newExpire.Text = (mOfferDate.Year + 1) + "/" + mOfferDate.Month;
            //Debug.WriteLine("newexpire" + newExpire.Text);
            string[] newExpireSplit = newExpire.Text.Split('/');
            expireCalendar.VisibleDate = new DateTime(Convert.ToInt32(newExpireSplit[0]), Convert.ToInt32(newExpireSplit[1]), 1);

            ReloadDataAndPrintOutPage(id_list, companyName.Text);
        }

    }

    protected long GetPriceByProductName(Dictionary<string, long> priceByProductName, string productName)
    {
        if (priceByProductName.ContainsKey(productName))
        {
            return priceByProductName[productName];
        }
        else
        {
            throw new Exception("단가표에 정의되지 않은 제품명 입니다 : " + productName);
        }
    }

    protected long GetDollarByProductName(Dictionary<string, long> dollarByProductName, string productName)
    {
        if (dollarByProductName.ContainsKey(productName))
        {
            return dollarByProductName[productName];
        }
        else
        {
            throw new Exception("단가표에 정의되지 않은 제품명 입니다 : " + productName);
        }
    }

    protected void ReloadDataAndPrintOutPage(string id_list, string company_name)
    {
        Dictionary<string, int> combine = BindGridView_CRM(id_list);
        GridView2.DataSource = dt;

        // Column 합치기
        {
            //Debug.WriteLine("시작한다");
            foreach (DataRow row in dt.Rows)
            {
                //Debug.WriteLine("아오" + row[2]);
                //Debug.WriteLine("들어오냐");
                string key = row["new_l_product_category"].ToString() + "," + row["new_l_products"].ToString() + "," + row["new_dt_end"].ToString();
                //Debug.WriteLine("keys " + key);

                if (!combine.ContainsKey(key))
                {
                    combine.Add(key, Convert.ToInt32(row["new_i_qty"]));
                }
                else
                {
                    combine[key] += Convert.ToInt32(row["new_i_qty"]);
                }
            }

            List<string> added = new List<string>();

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                //Debug.WriteLine("여기는 들어오냐");
                foreach (string key in combine.Keys.ToList())
                {
                    //Debug.WriteLine("그럼 여기는?" + key);
                    string[] keys = key.Split(',');
                    DataRow dr = dt.Rows[i];
                    if (keys[0] == dr["new_l_product_category"].ToString() && keys[1] == dr["new_l_products"].ToString() && keys[2] == dr["new_dt_end"].ToString())
                    {
                        if (!added.Contains(key))
                        {
                            dr["new_i_qty"] = combine[key];
                            added.Add(key);
                            //Debug.WriteLine("not contains " + key + "i: " + i);
                            break;
                        }
                        else
                        {
                            //Debug.WriteLine("contains " + key + "i: " + i);
                            dr.Delete();
                            break;
                        }
                    }
                }
            }
        }

        // row 복제하기
        for (int i = dt.Rows.Count - 1; i >= 0; i--)
        {
            DataRow row1 = dt.NewRow();
            DataRow row2 = dt.NewRow();
            DataRow copyrow = dt.Rows[i];
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                row1[j] = copyrow[j];
                row2[j] = copyrow[j];
            }

            dt.Rows.InsertAt(row1, i);
            dt.Rows.InsertAt(row2, i);
        }

        Binding(dt);
        {
            // row에 데이터 분배? 하기/ 합계 row 만들기
            /*
            int qty = 0;
            int price = 0;
            int finalPrice = 0;
            int no = 1;

            for (int i = 0; i < dt.Rows.Count/3; i++)
            {
                // "finalPrice"
                DataRow row = dt.Rows[i * 3];

                String[] datesplit = (row["new_dt_expired"].ToString()).Split('/');
                String date = datesplit[0] + "." + datesplit[1] + "." + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));

                // forward
                string forwarddata;
                string backdata;
                DateTime now = DateTime.Now;
                string[] newdates = newExpire.Text.Split('.');

                forwarddata = "Forwarding (" + now.Year + "." + (now.Month + 1) + ".1 ~ " + newdates[0] + "." + newdates[1] + "."
                    + DateTime.DaysInMonth(Convert.ToInt32(newdates[0]) + 1, Convert.ToInt32(newdates[1])) + ") = " + row["Forward"] + "개월";

                // backdating
                if (Convert.ToInt32(row["BackDate"]) != 0)
                {
                    backdata = "BackDating (" + datesplit[0] + "." + (Convert.ToInt32(datesplit[1]) + 1) + ".1" + " ~ "
                        + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)
                        + ") = " + row["BackDate"] + "개월";
                }
                else
                {
                    backdata = "BackDating = 0";
                }

                string final = forwarddata + "<br>" + backdata;
                //row["기간요약"] = final;

                // 소비자 단가
                int danga = 10000;
                long fwd = rounding(danga / 12 * Convert.ToInt32(row["Forward"]));
                long back = rounding(danga / 12 * Convert.ToInt32(row["BackDate"]));

                dt.Rows[i*3+0]["기간요약"] = dt.Rows[i*3]["new_l_product_category"];
                dt.Rows[i*3+1]["기간요약"] = "&nbsp&nbsp&nbsp" + forwarddata;
                dt.Rows[i*3+2]["기간요약"] = "&nbsp&nbsp&nbsp" + backdata;

                dt.Rows[i*3+1]["customerRRP"] = fwd;
                dt.Rows[i*3+2]["customerRRP"] = back;
                dt.Rows[i*3+0]["customerRRP"] = Convert.ToInt32(dt.Rows[i*3+1]["customerRRP"]) + Convert.ToInt32(dt.Rows[i*3+2]["customerRRP"]);

                // 제안 단가
                double fwdPrice = Convert.ToInt32(dt.Rows[i*3+1]["customerRRP"]) * (1 - Convert.ToDouble(forwardDiscount.Text) * 0.01);
                double backPrice = Convert.ToInt32(dt.Rows[i*3+2]["customerRRP"]) * (1 - Convert.ToInt32(backDiscount.Text) * 0.01);

                long finalFwd = rounding(fwdPrice);
                long finalBack = rounding(backPrice);

                dt.Rows[i*3+0]["proposalRRP"] = finalFwd + finalBack;
                dt.Rows[i*3+1]["proposalRRP"] = finalFwd;
                dt.Rows[i*3+2]["proposalRRP"] = finalBack;

                dt.Rows[i*3+0]["finalPrice"] = (finalFwd + finalBack) * Convert.ToInt32(dt.Rows[i*3+0]["new_i_qty"]);

                // number
                row["number"] = no;
                no++;
                Debug.WriteLine("no: " + no);

                // 합계 구하기
                qty += Convert.ToInt32(row["new_i_qty"]);
                price += Convert.ToInt32(row["customerRRP"]) * Convert.ToInt32(row["new_i_qty"]);
                finalPrice += Convert.ToInt32(row["finalPrice"]);
            }

            // 합계 row 
            DataRow newRow = dt.NewRow();
            newRow[0] = "합계";
            newRow["new_l_product_category"] = "합계";
            newRow["new_i_qty"] = qty;
            newRow["customerRRP"] = price;
            //newRow["proposalRRP"] = "";
            newRow["finalPrice"] = finalPrice;
            dt.Rows.Add(newRow);
            */
        }

        GridView2.DataBind();

        // rowspan
        for (int i = 0; i < GridView2.Rows.Count - 3; i++)
        {
            GridViewRow rowss = GridView2.Rows[i];
            //Debug.WriteLine("왜" + rowss.Cells[1].Text + GridView2.Rows[i + 1].Cells[1].Text + GridView2.Rows[i + 2].Cells[1].Text);
            foreach (int j in new int[] { 0, 2, 3 })
            {
                rowss.Cells[j].RowSpan = 3;
                GridView2.Rows[i + 2].Cells[j].Visible = false;
                GridView2.Rows[i + 1].Cells[j].Visible = false;
            }
            i += 2;
        }

        dt = (DataTable)GridView2.DataSource;
        Debug.WriteLine("**** Count: " + dt.Rows.Count);

        Session["GridData"] = dt;

    }

    protected void Binding(DataTable dt)
    {
        // row에 데이터 분배? 하기/ 합계 row 만들기
        long qty = 0;
        long price = 0;
        long finalPrice = 0;
        double finalDollar = 0;
        int no = 1;
        double exchangeRate = Convert.ToDouble(chgExchangeRate.Text);


        Dictionary<string, long> priceByProductName = (Dictionary<string, long>)Session["BK_priceBook"];
        Dictionary<string, long> dollarByProductName = (Dictionary<string, long>)Session["BK_dollarBook"];


        // 새 견적종료일
        DateTime newExpireDate = DateTime.Now;
        {
            string[] newexpdate = newExpire.Text.Split('/');
            string newdate = newExpire.Text + "/" + DateTime.DaysInMonth(Convert.ToInt32(newexpdate[0]), Convert.ToInt32(newexpdate[1]));
            newExpireDate = DateTime.ParseExact(newdate, "yyyy/M/dd", null);
        }


        for (int i = 0; i < dt.Rows.Count / 3; i++)
        {
            //---------------------------------------------------------------------
            // 라이센스의 종료일로 견적기간(back/forward)를 계산한다(각 줄마다 수행) 
            //---------------------------------------------------------------------
            // 1. 종료월 구하기(end date ==> end date month)
            // 2. forward 계산
            //    2.1 견적시작월(또는 현재월)보다 라이센스 종료일이 큰 경우(아직 종료일이 도래하지 않음) ==> 라이센스 종료월부터 ~ 견적종료월 까지로 계산 
            //    2.2 견적시작월(또는 현재월)보다 라이센스 종료일이 작은 경우(이미 종료되었음) ==> 견적시작월 부터 견적종료월까리로 계산
            //    2.3 만약 forward 계산결과가 36개월을 넘어서는 경우 36개월 까지로 견적종료월을 조정한다.
            // 3. backdating 계산
            //    3.1 견적시작월(또는 현재월)보다 라이센스 종료일이 작은 경우(이미 종료되었음) ==> 견적시작월 - 라이센스 종료월
            //    3.2 위 3.1 아닌 경우 무조건 0
            //    3.2 만약 backdating 계산결과가 36개월을 넘어서는 경우 36개월 까지로 라이센스 종료월을 36개월로 되도록 조정한다.


            // "finalPrice"
            DataRow row = dt.Rows[i * 3];

            //string _companyName = row[""].ToString();
            //string _siteName = row[""].ToString();
            string _companyName = companyName.Text;
            string _productCate = row["new_l_product_category"].ToString();
            string _productName = row["new_l_products"].ToString();
            string _quantity = row["new_i_qty"].ToString();
            string _endDate = row["new_dt_end"].ToString();
            long _price = GetPriceByProductName(priceByProductName, _productName);
            long _dollar = GetDollarByProductName(dollarByProductName, _productName);
            //Debug.WriteLine("dollar: " + _dollar);
            CLicenseOffer newOffer = new CLicenseOffer(_companyName, _productCate, _productName, _endDate, _quantity, _price, _dollar);

            // 종료월 출력
            row["new_dt_end"] = _endDate;
            dt.Rows[i * 3 + 1]["new_dt_end"] = "";
            dt.Rows[i * 3 + 2]["new_dt_end"] = "";


            // 수량 출력
            row["new_i_qty"] = _quantity;
            dt.Rows[i * 3 + 1]["new_i_qty"] = "";
            dt.Rows[i * 3 + 2]["new_i_qty"] = "";

            // 기간요약 출력 
            dt.Rows[i * 3 + 0]["기간요약"] = newOffer.ToString() + " (" + (newOffer.GetBackdatingAsMonth(mOfferDate) + newOffer.GetForwardAsMonth(mOfferDate, newExpireDate)) + "개월)"; //  dt.Rows[i * 3]["new_l_product_category"];
            dt.Rows[i * 3 + 1]["기간요약"] = "           " + newOffer.GetForwardAsDescription(mOfferDate, newExpireDate);   //   "&nbsp&nbsp&nbsp" + forwarddata;
            dt.Rows[i * 3 + 2]["기간요약"] = "           " + newOffer.GetBackdatingAsDescription(mOfferDate);//"&nbsp&nbsp&nbsp" + backdata;


            // 제품단가
            long fwd = newOffer.GetPrice_ForwardCost(mOfferDate, newExpireDate);
            //Debug.WriteLine("fwd" + fwd);
            long back = newOffer.GetPrice_BackCost(mOfferDate);
            dt.Rows[i * 3 + 1]["customerRRP"] = fwd;
            dt.Rows[i * 3 + 2]["customerRRP"] = back;
            dt.Rows[i * 3 + 0]["customerRRP"] = fwd + back;


            // 제안단가
            double fwdDiscountPercent;
            double backDiscountPercent;

            // 비어있으면 
            try
            {
                fwdDiscountPercent = Convert.ToDouble(forwardDiscount.Text);
                backDiscountPercent = Convert.ToDouble(backDiscount.Text);
                long finalFwd = newOffer.GetPrice_DiscountedCost(fwd, fwdDiscountPercent);
                long finalBack = newOffer.GetPrice_DiscountedCost(back, backDiscountPercent);
                dt.Rows[i * 3 + 0]["proposalRRP"] = (finalFwd + finalBack).ToString("N0");
                //dt.Rows[i * 3 + 1]["proposalRRP"] = finalFwd;
                //dt.Rows[i * 3 + 2]["proposalRRP"] = finalBack;

                // 최종금액 
                dt.Rows[i * 3 + 0]["finalPrice"] = (newOffer.GetPrice_Amount(finalFwd + finalBack)).ToString("N0");
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('숫자를 입력해 주세요')", true);
            }

            {
                //int danga = 10000;
                //try
                //{
                //    // 소비자 단가
                //    long fwd = rounding(danga / 12 * Convert.ToInt32(row["Forward"]));
                //    long back = rounding(danga / 12 * Convert.ToInt32(row["BackDate"]));

                //    dt.Rows[i * 3 + 1]["customerRRP"] = fwd;
                //    dt.Rows[i * 3 + 2]["customerRRP"] = back;
                //    dt.Rows[i * 3 + 0]["customerRRP"] = Convert.ToInt32(dt.Rows[i * 3 + 1]["customerRRP"]) + Convert.ToInt32(dt.Rows[i * 3 + 2]["customerRRP"]);



                //    // 제안 단가
                //    double fwdPrice = Convert.ToInt32(dt.Rows[i * 3 + 1]["customerRRP"]) * (1 - Convert.ToDouble(forwardDiscount.Text) * 0.01);
                //    double backPrice = Convert.ToInt32(dt.Rows[i * 3 + 2]["customerRRP"]) * (1 - Convert.ToInt32(backDiscount.Text) * 0.01);

                //    long finalFwd = rounding(fwdPrice);
                //    long finalBack = rounding(backPrice);

                //    dt.Rows[i * 3 + 0]["proposalRRP"] = finalFwd + finalBack;
                //    dt.Rows[i * 3 + 1]["proposalRRP"] = finalFwd;
                //    dt.Rows[i * 3 + 2]["proposalRRP"] = finalBack;

                //    // 최종금액 
                //    dt.Rows[i * 3 + 0]["finalPrice"] = (finalFwd + finalBack) * Convert.ToInt32(dt.Rows[i * 3 + 0]["new_i_qty"]);
                //}
                //catch
                //{
                //    dt.Rows[i * 3 + 1]["customerRRP"] = "";
                //    dt.Rows[i * 3 + 2]["customerRRP"] = "";
                //    dt.Rows[i * 3 + 0]["customerRRP"] = "";

                //    dt.Rows[i * 3 + 0]["proposalRRP"] = "";
                //    dt.Rows[i * 3 + 1]["proposalRRP"] = "";
                //    dt.Rows[i * 3 + 2]["proposalRRP"] = "";

                //    dt.Rows[i * 3 + 0]["finalPrice"] = "";
                //}
            }

            // 달러 원가 계산하기
            double fwdDollar = newOffer.GetPrice_ForwardDollar(mOfferDate, newExpireDate);
            double backDollar = newOffer.GetPrice_BackDollar(mOfferDate);
            double totalDollar = newOffer.GetPrice_AmountDollar(fwdDollar + backDollar);

            // number
            row["number"] = no;
            no++;
            //Debug.WriteLine("no: " + no);

            // 합계 구하기
            qty += Convert.ToInt32(row["new_i_qty"]);
            price += long.Parse(row["customerRRP"].ToString()) * Convert.ToInt32(row["new_i_qty"]);
            finalPrice += NormalizeToLong(row["finalPrice"].ToString());
            finalDollar += totalDollar;
        }

        // 합계 row 
        DataRow newRow = dt.NewRow();
        //newRow[""] = "합계";
        newRow["기간요약"] = "합계";
        newRow["new_i_qty"] = qty;
        newRow["customerRRP"] = price;
        //newRow["proposalRRP"] = "";
        newRow["finalPrice"] = finalPrice.ToString("N0");
        dt.Rows.Add(newRow);


        // Dollar row
        DataRow dollarRow = dt.NewRow();
        dollarRow["customerRRP"] = finalDollar;
        //dollarRow["customerRRP"] = "USD " + finalDollar;
        //dollarRow["proposalRRP"] = "KRW " + (finalDollar * exchangeRate).ToString("N0");
        dollarRow["finalPrice"] = "Margin: " + ((finalPrice - (finalDollar * exchangeRate)) / finalPrice * 100).ToString("N2") + " %";

        dt.Rows.Add(dollarRow);
    }

    static string staticLogFilename = AppDomain.CurrentDomain.BaseDirectory + "\\OfferSystemLog.txt";

    protected void WriteLog(int logLevel, string sMethodName, string sMsg)
    {
        try
        {
            if (CConst.logLevel <= logLevel)
            {
                System.IO.File.AppendAllText(staticLogFilename, string.Concat(DateTime.Now.ToString(), "  ", sMethodName, "  ", sMsg, "\r\n").ToString());
            }
        }
        catch
        {
        }
    }

    // 원가표
    protected Dictionary<string, long> GetPriceDollar(string priceFilename)
    {
        Dictionary<string, long> dicName = new Dictionary<string, long>();
        {
            try
            {
                DataSet dsMap = ReadXML(1);

                for (int i = 0; i < CCommon.getDSHeaderCount(dsMap); i++)
                {
                    WriteLog(0, "tst", string.Format("[{0}] = {1}", i, CCommon.getDSHeaderByIndex(dsMap, i)));
                }

                //DataSet dsMap = Listup_PriceTable_SharePoint("OfferSystem", priceFilename, 1);  /// 주의... 1 번 시트를 읽음
                for (int i = 0; i < CCommon.getDSCount(dsMap); i++)
                {
                    WriteLog(1, "GetPriceDollar()", string.Format("read xml rowid={0}", i));

                    string nameXLSName = CCommon.getDSValueAsString(dsMap, i, 1);  // B 컬럼
                    WriteLog(1, "GetPriceDollar()", string.Format("read xml colname={0}", nameXLSName));

                    //long nameVALUE = CCommon.getDSValueAsLong(dsMap, i, "ALC_List_Price");  // E 컬럼
                    //WriteLog(1, "GetPriceDollar()", string.Format("read xml name={0}", nameVALUE));

                    string nameVALUE_VAR_S = CCommon.getDSValueAsString(dsMap, i, "ALC_VAR_Price");  // G 컬럼
                    long nameVALUE_VAR = (long)Math.Floor(double.Parse(nameVALUE_VAR_S));
                    WriteLog(1, "GetPriceDollar()", string.Format("read xml value=={0}", nameVALUE_VAR));

                    // string nameXLS2 = CCommon.getDSValueAsString(dsMap, i, "XLSNAME");
                    if (!dicName.ContainsKey(nameXLSName))
                        dicName.Add(nameXLSName, nameVALUE_VAR);
                }
            }
            catch (Exception ex)
            {
                WriteLog(4, "GetPriceDollar()", ex.Message);
            }

            //// print 
            //foreach (KeyValuePair<string, string> adata in dicName)
            //{
            //    Debug.WriteLine(string.Format("가격표 CRM이름={0}, XLS이름={1}", adata.Key, adata.Value));
            //}


            // print 2 
            List<string> keyNames = new List<string>();
            foreach (KeyValuePair<string, long> adata in dicName)
                keyNames.Add(adata.Key);
            keyNames.Sort();
            for (int i = 0; i < keyNames.Count; i++)
            {
                string productName_XLSName = keyNames[i];
                long productName_Price = dicName[productName_XLSName];
                //Debug.WriteLine(string.Format("가격표 XLS이름={0}, 달러={1}", productName_XLSName, productName_Price));
            }
        }

        return dicName;
    }

    // 정가표
    protected Dictionary<string, long> GetPriceCost(string priceFilename)
    {
        Dictionary<string, long> dicName = new Dictionary<string, long>();
        {
            {
                DataSet dsMap = ReadXML(1);
                ////DataSet dsMap = Listup_PriceTable_SharePoint("OfferSystem", priceFilename, 1);  /// 주의... 1 번 시트를 읽음
                for (int i = 0; i < CCommon.getDSCount(dsMap); i++)
                {
                    string nameXLSName = CCommon.getDSValueAsString(dsMap, i, 1);  // B 컬럼
                    long nameVALUE = CCommon.getDSValueAsLong(dsMap, i, "ALC_List_Price");  // G 컬럼
                    // string nameXLS2 = CCommon.getDSValueAsString(dsMap, i, "XLSNAME");
                    if (!dicName.ContainsKey(nameXLSName))
                        dicName.Add(nameXLSName, nameVALUE);
                }
            }

            // print 2 
            List<string> keyNames = new List<string>();
            foreach (KeyValuePair<string, long> adata in dicName)
                keyNames.Add(adata.Key);
            keyNames.Sort();
            for (int i = 0; i < keyNames.Count; i++)
            {
                string productName_XLSName = keyNames[i];
                long productName_Price = dicName[productName_XLSName];
                //Debug.WriteLine(string.Format("가격표 XLS이름={0}, 가격={1}", productName_XLSName, productName_Price));
            }
        }

        return dicName;
    }

    protected string GetExcelName1(int index)
    {
        string xlsName = Server.MapPath("~/temp/") + string.Format("cost_file{0}.xml", index);
        return xlsName;
    }

    protected DataSet ReadXML(int index)
    {
        string xlsName = GetExcelName1(index);

        DataSet xlsDS = new DataSet();
        xlsDS.ReadXml(xlsName, XmlReadMode.ReadSchema);
        return xlsDS;
    }

    protected Dictionary<string, string> GetPriceMap(string priceFilename)
    {
        Dictionary<string, string> dicName = new Dictionary<string, string>();
        {
            {
                DataSet dsMap = ReadXML(2);
                // DataSet dsMap = Listup_PriceTable_SharePoint("OfferSystem", priceFilename, 2); /// 주의... 2 번 시트를 읽음
                for (int i = 0; i < CCommon.getDSCount(dsMap); i++)
                {
                    try
                    {
                        string nameCRM = CCommon.getDSValueAsString(dsMap, i, 1);
                        string nameXLS = CCommon.getDSValueAsString(dsMap, i, 2);
                        // string nameXLS2 = CCommon.getDSValueAsString(dsMap, i, "XLSNAME");
                        if (!dicName.ContainsKey(nameCRM))
                            dicName.Add(nameCRM, nameXLS);
                    }
                    catch
                    {
                    }
                }
            }

            // print 
            foreach (KeyValuePair<string, string> adata in dicName)
            {
                //Debug.WriteLine(string.Format("가격표 CRM이름={0}, XLS이름={1}", adata.Key, adata.Value));
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
                //Debug.WriteLine(string.Format("가격표 CRM이름={0}, XLS이름={1}", productName_CRM, productName_XLS));
            }
        }

        return dicName;
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

    protected Dictionary<string, int> BindGridView_CRM(string id_list)
    {
        //QueryDataFilter2("new_customer_products", new ColumnSet(true), new List<string>() { "new_l_account", "new_l_products", "new_p_status", "new_l_product_category" }, id_list);
        Dictionary<string, int> combine = QueryDataFilter3("new_customer_products", new ColumnSet(true), new List<string>() { "new_l_account", "new_l_products", "new_p_status", "new_l_product_category" }, id_list);
        return combine;
    }

    protected Dictionary<string, int> QueryDataFilter3(string entityName, ColumnSet columnSet, List<string> relAttrNames, string id_list)
    {
        //DataSet ds = new DataSet();
        Dictionary<string, int> combine = new Dictionary<string, int>();
        Dictionary<string, long> priceByProductName = (Dictionary<string, long>)Session["BK_priceBook"];
        Dictionary<string, long> dollarByProductName = (Dictionary<string, long>)Session["BK_dollarBook"];

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

            QueryExpression query = new QueryExpression(entityName);
            query.ColumnSet = columnSet;

            // filter link condition style 1.
            //LinkEntity customerRef = query.AddLink("account", "new_l_account", "accountid");
            //customerRef.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "필옵틱스");

            // filter by id(primary key)
            if (idList.Count > 0)
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
                //Debug.WriteLine(string.Format("column name = {0}", aName));
            }

            // row list to datatable
            foreach (var item in resultList)
            {
                DataRow dr = dt.NewRow();

                string saveData = "";
                string key = "";

                //Dictionary<string, int> combine = new Dictionary<string, int>();
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

            // 필요한 Column들 추가
            {
                dt.Columns.Add("BackDate");
                dt.Columns.Add("Forward");
                dt.Columns.Add("기간요약");
                dt.Columns.Add("customerRRP", typeof(long));
                dt.Columns.Add("proposalRRP", typeof(string));
                dt.Columns.Add("finalPrice", typeof(string));
                dt.Columns.Add("number", typeof(int));

                foreach (DataRow row in dt.Rows)
                {
                    // "Expired Date" Column
                    try
                    {
                        string expdate = row["new_dt_end"].ToString();
                        string[] splitDate = expdate.Split('-');
                        row["new_dt_end"] = splitDate[0] + "/" + splitDate[1];

                        // Convert.ToInt32(row["단가"])
                    }
                    catch
                    {
                        row["new_dt_end"] = "";
                    }

                    // CLicenseOffer 클래스로 처리 
                    string _companyName = companyName.Text;
                    //string _siteName = row["new_txt_site"].ToString();
                    string _productCate = row["new_l_product_category"].ToString();
                    string _productName = row["new_l_products"].ToString();
                    string _quantity = "1";  // 실제수량은 나중에 grid에서 재처리 
                    string _endDate = row["new_dt_end"].ToString();
                    long _price = GetPriceByProductName(priceByProductName, _productName);
                    long _dollar = GetDollarByProductName(dollarByProductName, _productName);
                    CLicenseOffer newOffer = new CLicenseOffer(_companyName, _productCate, _productName, _endDate, _quantity, _price, _dollar);

                    // back dating 계산
                    row["BackDate"] = newOffer.GetBackdatingAsMonth(mOfferDate);

                    // 새 견적종료일
                    DateTime newExpireDate = DateTime.Now;
                    {
                        string[] newexpdate = newExpire.Text.Split('/');
                        string newdate = newExpire.Text + "/" + DateTime.DaysInMonth(Convert.ToInt32(newexpdate[0]), Convert.ToInt32(newexpdate[1]));
                        newExpireDate = DateTime.ParseExact(newdate, "yyyy/M/dd", null);
                    }

                    // forward dating계산 
                    row["Forward"] = newOffer.GetForwardAsMonth(mOfferDate, newExpireDate);

                }
            }
        }
        return combine;
    }


    #region "CRM API"

    // Old Query Function
    /*
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
                //Debug.WriteLine(string.Format("column name = {0}", aName));
            }

            // row list to datatable
            foreach (var item in resultList)
            {
                DataRow dr = dt.NewRow();

                string saveData = "";
                string key = "";

                Dictionary<string, int> combine = new Dictionary<string, int>();
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

            {
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    string expdate = row["new_dt_expired"].ToString();
                    string[] splitDate = expdate.Split('-');
                    row["new_dt_expired"] = splitDate[0] + "/" + splitDate[1];

                    // Convert.ToInt32(row["단가"])
                    }
                catch
                {
                    row["new_dt_expired"] = "";
                }

                try
                {
                    string category = row["new_l_product_category"].ToString();
                    string product = row["new_l_products"].ToString();
                    row["new_l_product_category"] = category + " " + product;
                    //GridView2.Columns.Insert(2, );
                }
                catch
                {
                    row["new_l_product_category"] = "";
                }
            }


            dt.Columns.Add("BackDate");
            foreach (DataRow row in dt.Rows)
            {
                //String endDate = ((string) row["new_dt_expired"]) + "/31";
                //row["Backdating"] = CalcBackDate(DateTime.ParseExact(endDate, "yyyy/MM/dd", null));
                //row["Backdating"] = "";

                String[] datesplit = (row["new_dt_expired"].ToString()).Split('/');

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

                String[] datesplit = (row["new_dt_expired"].ToString()).Split('/');

                try
                {
                    string date = datesplit[0] + "/" + datesplit[1] + "/" + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
                    DateTime endDate = DateTime.ParseExact(date, "yyyy/M/dd", null);
                    row["Forward"] = CalcForwardDate(endDate);
                }
                catch
                {
                    row["Forward"] = "";
                }

            }

            dt.Columns.Add("기간요약");
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    String[] datesplit = (row["new_dt_expired"].ToString()).Split('/');
                    String date = datesplit[0] + "." + datesplit[1] + "." + DateTime.DaysInMonth(Int32.Parse(datesplit[0]), Int32.Parse(datesplit[1]));
                    string forwarddata;

                    if (Convert.ToInt32(row["Forward"]) == 12)
                    {
                        DateTime now = DateTime.Now;
                        forwarddata = "Forward (" + now.Year + "." + (now.Month+1) + ".1 ~ " + (now.Year+1) + "." + now.Month + "." + DateTime.DaysInMonth(now.Year+1, now.Month) + ") = " + row["Forward"];
                    }
                    else
                    {
                        forwarddata = "Forward (" + DateTime.Now.Year + "." + DateTime.Now.Month + ".1 ~ " + date + ") = " + row["Forward"];
                    }
                    string backdata = "BackDate (" + date + " ~ " + DateTime.Now.Year + "." + DateTime.Now.Month + "." +
                                DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + ") = " + row["BackDate"];

                    string final = forwarddata + "<br>" + backdata;
                    row["기간요약"] = final;
                }
                catch
                {
                    row["기간요약"] = "";
                }
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
                // Convert.ToInt32(row["단가"])
            }


            dt.Columns.Add("proposalRRP", typeof(int));
            foreach (DataRow row in dt.Rows)
            {
                row["proposalRRP"] = Convert.ToInt32(row["customerRRP"]) * Convert.ToInt32(row["new_i_qty"]);
                // Convert.ToInt32(row["단가"])
            }


            dt.Columns.Add("finalPrice");
            foreach (DataRow row in dt.Rows)
            {
                double fwdPrice = Convert.ToInt32(row["proposalRRP"]) * (1 - Convert.ToDouble(forwardDiscount.Text) * 0.01) / 12 * Convert.ToInt32(row["Forward"]);
                double backPrice = Convert.ToInt32(row["proposalRRP"]) * (1 - Convert.ToInt32(backDiscount.Text) * 0.01) / 12 * Convert.ToInt32(row["BackDate"]);

                string final = "Forward Price: " + fwdPrice.ToString() + "<br>" + "BackDate Price: " + backPrice.ToString() + "<br>" + "Total: " + (fwdPrice + backPrice);
                row["finalPrice"] = final;
            }
            }
        }

        //return ds;
    }
    */

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


    protected int CalcBackDate(DateTime expDate, DateTime offerDate)
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
    }

    protected int CalcForwardDate(DateTime newExpDate, DateTime expDate)
    {
        return (int)((newExpDate.Year - expDate.Year) * 12) + newExpDate.Month - expDate.Month;
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
        GridView2.PageIndex = e.NewPageIndex;
        if (Session[_pageSortedKey] != null)
        {
            GridView2.DataSource = Session[_pageSortedKey];
            GridView2.DataBind();
        }
        else
        {
            //String ids = "";
            //String[] id_list;

            //if (Request.QueryString["ID"] != null)
            //    ids = Request.QueryString["ID"];
            //id_list = ids.Split(' ');

            String id_list = (string)GetValue_Session("sendIDList");

            //SqlCommand sqlCmd = CreateSqlCommand(id_list);
            //BindGridView1(id_list);
            BindGridView_CRM(id_list);
            GridView2.DataSource = dt;
            GridView2.DataBind();
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

    protected void calcbtn_Click(object sender, EventArgs e)
    {
        DataTable dts = (DataTable)Session["GridData"];

        string org_id_list = (string)Session["BK_sendIDList"];
        string org_companyName = (string)Session["BK_companyName"];

        Session["sendIDList"] = org_id_list;
        Session["companyName"] = org_companyName;

        try
        {
            ReloadDataAndPrintOutPage(org_id_list, org_companyName);
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('숫자를 입력해 주세요')", true);
        }
        return;
    }

    protected void calendarbtn_Click(object sender, EventArgs e)
    {
        if (!expireCalendar.Visible)
        {
            expireCalendar.Visible = true;
        }
        else
        {
            expireCalendar.Visible = false;
        }
    }

    protected void expireCalendar_SelectionChanged(object sender, EventArgs e)
    {
        DateTime select = expireCalendar.SelectedDate;
        string selectedDate = select.Year.ToString() + "/" + select.Month.ToString();
        newExpire.Text = selectedDate;
        expireCalendar.Visible = false;
    }

    protected void estimatebtn_Click(object sender, EventArgs e)
    {
        DataTable dts = (DataTable)Session["GridData"];
        string compName = (string)Session["BK_companyName"];
        DataSet ds = new DataSet();
        ds.Tables.Add(dts);
        string filePath = string.Format("C:/VS/Offer_{0}_{1}.xlsx", compName, DateTime.Now.ToShortDateString());

        int totalCols = GridView2.Columns.Count;
        int totalRows = GridView2.Rows.Count;

        Debug.WriteLine("totalcols" + totalCols + "totalrows" + totalRows);


        using (ExcelPackage pck = new ExcelPackage())
        {
            ExcelWorksheet workSheet = pck.Workbook.Worksheets.Add(dts.TableName);

            Debug.WriteLine("totalcols" + totalCols + "totalrows" + totalRows);
            GridViewRow headerRow = GridView2.HeaderRow;


            // 날짜 넣기
            workSheet.Cells[2, 7].Value = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");
            EPP_CellStyle(workSheet.Cells[2, 7], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, verBottom, horCenter, false, true, 10);

            // Solko Logo 넣기
            System.Drawing.Image logo = System.Drawing.Image.FromFile("C:\\Users\\solko\\Desktop\\solko_logo.jpg");
            var solkoLogo = workSheet.Drawings.AddPicture("solkoLogo", logo);
            solkoLogo.SetPosition(2, 0, 1, 0);
            solkoLogo.SetSize(252, 46);

            // Sales Quotation
            workSheet.Cells[3, 1].Value = "Sales Quotation";
            EPP_CellStyle(workSheet.Cells[3, 1], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, verBottom, horCenter, false, true, 28);
            workSheet.Cells[3, 1].Style.Font.UnderLine = true;
            workSheet.Cells[3, 1, 3, 8].Merge = true;

            // Solko 정보
            workSheet.Cells[7, 2].Value = "VAR: ";
            workSheet.Cells[7, 3].Value = "(주)솔코";
            workSheet.Cells[8, 2].Value = "Rep.: ";
            workSheet.Cells[8, 3].Value = Context.User.Identity.GetUserName();
            workSheet.Cells[9, 2].Value = "Tel: ";

            //SqlParameter param = new SqlParameter("@userName", Context.User.Identity.GetUserName());
            string sqlcmd = "SELECT PhoneNumber, Email FROM dbo.AspNetUsers;";
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlCommand cmd = new SqlCommand(sqlcmd, dbConn);
            //cmd.Parameters.Add(param);
            //cmd.Parameters["@userName"].Value = Context.User.Identity.GetUserName();
            SqlDataReader dr = cmd.ExecuteReader();

            string test11 = "";
            string test2 = "";
            int j = 0;
            if (dr.Read())
            {
                //for (int i = 0; i < 8; i++)
                //{
                //    System.Diagnostics.Debug.WriteLine("asdfasdf: ",j, "  " + dr[i].ToString());
                //}
                test11 = dr[0].ToString();
                Debug.WriteLine("test11: " + test11);
                test2 = dr[1].ToString();
                Debug.WriteLine("test2: " + test2);
                j++;
            }
            else
            {
                Debug.WriteLine("i: " + j + "XXX");
                j++;
            }
                
            //foreach (object drrr in dr)
            //{
            //    System.Diagnostics.Debug.WriteLine("asdfasdf: ", drrr);
            //}

            Debug.WriteLine(cmd.CommandText);

            string userNumber = dr[0].ToString();
            //userNumber.Split(2, 5);
            string userEmail = dr[1].ToString();
            dbConn.Close();
            //UserManager.FindAsync(Context.User.Identity., Context.User.
            workSheet.Cells[9, 3].Value = "031-8069-8306 || " + userNumber;
            workSheet.Cells[10, 2].Value = "Fax: ";
            workSheet.Cells[10, 3].Value = "031-8069-8301";
            workSheet.Cells[11, 2].Value = "Email: ";
            workSheet.Cells[11, 3].Value = userEmail;

            // 고객 정보
            workSheet.Cells[7, 6].Value = "Customer: ";
            workSheet.Cells[7, 7].Value = (string)Session["BK_companyName"];
            workSheet.Cells[8, 6].Value = "Rep.: ";
            workSheet.Cells[9, 6].Value = "Tel: ";
            workSheet.Cells[10, 6].Value = "Email: ";

            workSheet.Cells["B7:G11"].Style.Font.Size = 10;
            workSheet.Cells["B7:B11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells["C7:C11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["F7:F11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells["G7:G11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            //workSheet.Cells[1, 6].Value = "Forwarding D/C";
            //workSheet.Cells[1, 7].Value = forwardDiscount.Text + "%";

            //workSheet.Cells[2, 6].Value = "Backdating D/C";
            //workSheet.Cells[2, 7].Value = backDiscount.Text + "%";


            // 견적서 Table
            workSheet.Cells[15, 2].Value = "1. Solution";
            workSheet.Cells[15, 2].Style.Font.Size = 10;
            workSheet.Cells[15, 2].Style.Font.Bold = true;

            workSheet.Cells[15, 7].Value = "(Unit: WON)";
            workSheet.Cells[15, 7].Style.Font.Size = 8;
            workSheet.Cells[15, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells[16, 2].Value = "No.";
            workSheet.Cells[16, 3].Value = "Description";
            workSheet.Cells[16, 4].Value = "Seats";
            workSheet.Cells[16, 5].Value = "Customer RRP\n(소비자 단가)";
            //workSheet.Cells[16, 5].RichText.Clear();
            //workSheet.Cells[16, 5].RichText.Add("Customer RRP\r\n(소비자 단가)");

            workSheet.Cells[16, 6].Value = "Proposed Price\n(제안 단가)";
            workSheet.Cells[16, 7].Value = "Proposed Price\n(최종 견적가)";
            workSheet.Cells[16, 5].Style.WrapText = true;
            workSheet.Cells[16, 6].Style.WrapText = true;
            workSheet.Cells[16, 7].Style.WrapText = true;

            workSheet.Row(17).Height = (double)7.00;

            using (var range = workSheet.Cells[16, 2, 16, 7])
            {
                EPP_CellStyle(range, lineThin, lineThin, lineThin, lineThin, verCenter, horCenter, true, true, 10);
                EPP_CellStyle(workSheet.Cells[16, 2], lineThin, ExcelBorderStyle.None, lineThin, lineThin, verCenter, horCenter, true, true, 10);
                EPP_CellStyle(workSheet.Cells[16, 7], lineThin, lineThin, lineThin, ExcelBorderStyle.None, verCenter, horCenter, true, true, 10);
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9BC2E6"));
                workSheet.Row(16).Height = (double)50.0;
            }

            // 견적서 출력되는 본문의 엑셀 시작위치(17 = 컬럼헤더 바로 아랫줄)
            int tableStartRowIndex = 17;

            // 라이센스별 소계의 row index ==> 제일 아래의 합계에 목록으로 주기 위함 
            List<int> listSubSum = new List<int>();

            // 엑셀출력용 row index 
            int excelRowIndex = 1;

            // Cell에 값 넣기
            for (var readRowIndex = 1; readRowIndex <= totalRows - 2; readRowIndex++)
            {
                // backdating에 원가/견적 금액이 없는 경우 ==> 줄 출력하지 않기 
                if (readRowIndex % 3 == 0)
                {
                    string sBackDating = GridView2.Rows[readRowIndex - 1].Cells[1].Text;
                    if (sBackDating.Trim().Equals("BackDating = 0", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // NO
                {
                    int colIndex = 1; // NO

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex + 1].Value = " ";
                    }
                    else
                    {
                        int iVal = int.Parse(NormalizeToDecimal(product));
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex + 1].Value = iVal;


                        // 소계 row index 저장 
                        listSubSum.Add(tableStartRowIndex + excelRowIndex);

                    }
                }


                // 기간요약
                {
                    int colIndex = 2;

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;
                    //Debug.WriteLine("product: " + product);

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        product = " ";
                    }

                    workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex + 1].Value = product;

                }

                // 종료일자 --> 엑셀에 출력하지 않음으로 뺌.
                {
                }


                // 수량
                {
                    int colIndex = 4;

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = " ";
                    }
                    else
                    {
                        int iVal = int.Parse(NormalizeToDecimal(product));
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = iVal;
                    }

                }

                // 소비자 단가
                {
                    int colIndex = 5;

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = " ";
                    }
                    else
                    {
                        long iVal = long.Parse(NormalizeToDecimal(product));
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = iVal;
                    }
                }


                // 견적단가
                {
                    int colIndex = 6;

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = " ";
                    }
                    else
                    {
                        long iVal = long.Parse(NormalizeToDecimal(product));
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = iVal;
                    }
                }

                // 최종 견적가
                {
                    int colIndex = 7;

                    string product = GridView2.Rows[readRowIndex - 1].Cells[colIndex - 1].Text;

                    // Blank Cell 일 경우
                    if (IsNull(product))
                    {
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = " ";
                    }
                    else
                    {
                        string form_final_RRP = string.Format("D{0} * F{0}", excelRowIndex + tableStartRowIndex);
                        workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Formula = form_final_RRP;

                        //long iVal = long.Parse(NormalizeToDecimal(product));
                        //workSheet.Cells[excelRowIndex + tableStartRowIndex, colIndex].Value = iVal;
                    }
                }

                excelRowIndex++;
            }

            // Table의 마지막 줄
            int last_table_row = listSubSum[listSubSum.Count - 1];

            // Total Columns
            workSheet.Cells[last_table_row + 7, 2].Value = "Sub Total";
            workSheet.Cells[last_table_row + 7, 2, last_table_row + 7, 4].Merge = true;
            workSheet.Cells[last_table_row + 7, 2, last_table_row + 7, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[last_table_row + 7, 2, last_table_row + 7, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
            workSheet.Cells[last_table_row + 8, 2].Value = "VAT";
            workSheet.Cells[last_table_row + 8, 2, last_table_row + 8, 4].Merge = true;
            workSheet.Cells[last_table_row + 8, 2, last_table_row + 8, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[last_table_row + 8, 2, last_table_row + 8, 4].Style.Font.Bold = true;
            workSheet.Cells[last_table_row + 9, 2].Value = "Total Price";
            workSheet.Cells[last_table_row + 9, 2, last_table_row + 9, 4].Style.Font.Bold = true;
            workSheet.Cells[last_table_row + 9, 2, last_table_row + 9, 4].Merge = true;
            workSheet.Cells[last_table_row + 9, 3, last_table_row + 9, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            EPP_CellStyle(workSheet.Cells[last_table_row + 7, 2, last_table_row + 9, 4], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, true, 10);

            // Customer RRP
            // 제품 개수
            int num_of_prod = (totalRows - 2) / 3;
            string form = "D18 * E18";

            StringBuilder sbRows = new StringBuilder();
            for (int i = 0; i < listSubSum.Count; i++)
            {
                if (sbRows.Length > 0)
                    sbRows.Append(", ");

                sbRows.Append(string.Format("D{0} * E{0}", listSubSum[i]));
                workSheet.Cells[listSubSum[i], 3].Style.Font.Bold = true;
                workSheet.Cells[listSubSum[i], 5].Style.Font.Bold = true;
            }
            form = sbRows.ToString();

            // Sub Total
            workSheet.Cells[last_table_row + 7, 5].Formula = "SUM(" + form + ")";

            // VAT
            workSheet.Cells[last_table_row + 8, 5].Formula = "ROUND(E" + (last_table_row + 7) + "* 0.1, 0)";

            // Total Price 
            workSheet.Cells[last_table_row + 9, 5].Formula = "ROUND(SUM(E" + (last_table_row + 7) + ", E" + (last_table_row + 8) + "), 0)";


            // 최종 견적가
            string form_final = "";
            StringBuilder sbRows_final = new StringBuilder();
            for (int i = 0; i < listSubSum.Count; i++)
            {
                if (sbRows_final.Length > 0)
                    sbRows_final.Append(", ");

                sbRows_final.Append(string.Format("G{0}", listSubSum[i]));
            }
            form_final = sbRows_final.ToString();

            // Sub Total
            workSheet.Cells[last_table_row + 7, 7].Formula = "SUM(" + form_final + ")";

            // VAT
            workSheet.Cells[last_table_row + 8, 7].Formula = "ROUND(G" + (last_table_row + 7) + "* 0.1, 0)";

            // Total Price 
            workSheet.Cells[last_table_row + 9, 7].Formula = "ROUND(SUM(G" + (last_table_row + 7) + ", G" + (last_table_row + 8) + "), 0)";

            workSheet.Cells[18, 5, totalRows + 23, 7].Style.Numberformat.Format = "#,###";
            using (var range = workSheet.Cells[last_table_row + 7, 5, last_table_row + 9, 7])
            {
                EPP_CellStyle(range, lineThin, lineThin, lineThin, lineThin, verCenter, horRight, false, true, 10);
            }

            // VAT Row는 볼드체 쓰지 않음
            workSheet.Cells[last_table_row + 8, 5, last_table_row + 8, 7].Style.Font.Bold = false;

            workSheet.Cells[18, 7, last_table_row, 7].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FF0000"));

            // Proposed Price
            workSheet.Cells[13, 2].Value = "▶ Proposed Price  :  ";
            string form_proposed_price = "CONCATENATE(\"  ▶ Proposed Price   :   일금\",NUMBERSTRING(G" + (last_table_row + 7) + ", 1),\"원정 (VAT 별도)\") ";
            workSheet.Cells[13, 2].Formula = form_proposed_price;
            workSheet.Cells[13, 2].Style.Font.Size = 14;
            workSheet.Cells[13, 2].Style.Font.Bold = true;

            // Table Style
            EPP_CellStyle(workSheet.Cells[17, 2, last_table_row + 6, 2], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 3, last_table_row + 6, 3], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horLeft, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 4, last_table_row + 6, 4], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 5, last_table_row + 6, 5], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 6, last_table_row + 6, 6], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 7, last_table_row + 6, 7], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, ExcelBorderStyle.None, verCenter, horRight, false, false, 10);
            workSheet.Cells[last_table_row + 7, 2, last_table_row + 7, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[last_table_row + 8, 2, last_table_row + 8, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[last_table_row + 9, 2, last_table_row + 9, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[last_table_row + 9, 2, last_table_row + 9, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //workSheet.Cells[16, 2, totalRows + 23, 7].Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#808080"));
            workSheet.Cells[17, 2, last_table_row + 3, 7].Style.Font.Name = "Calibri";

            // Set style
            workSheet.Cells.Style.Font.Name = "맑은 고딕";
            workSheet.Cells[1, 1, last_table_row + 26, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[1, 1, last_table_row + 26, 8].Style.Fill.BackgroundColor.SetColor(Color.White);
            workSheet.Cells[16, 2, 16, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
            workSheet.Cells[last_table_row + 7, 2, last_table_row + 7, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
            workSheet.Cells[last_table_row + 4, 3, last_table_row + 5, 3].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#8EA9DB"));
            workSheet.Cells[last_table_row + 4, 3].Value = "  ∙ Note ";
            workSheet.Cells[last_table_row + 4, 3, last_table_row + 4, 4].Style.Font.Bold = true;

            for (int i = 0; i < listSubSum.Count; i++)
            {
                workSheet.Cells[listSubSum[i], 3].Style.Font.Bold = true;
                workSheet.Cells[listSubSum[i], 5].Style.Font.Bold = true;
            }

            // 발주 확인
            workSheet.Cells[last_table_row + 11, 6].Value = "발주 확인 (회사명판)";
            //workSheet.Cells[totalRows + 25, 6].Style.Font.Size = 10;
            //workSheet.Cells[totalRows + 25, 6].Style.Font.Bold = true;
            workSheet.Cells[last_table_row + 11, 6, last_table_row + 11, 7].Merge = true;
            EPP_CellStyle(workSheet.Cells[last_table_row + 11, 6, last_table_row + 11, 7], lineThin, ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, verCenter, horCenter, false, true, 10);
            //workSheet.Cells[totalRows + 25, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[last_table_row + 11, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[last_table_row + 11, 6].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));

            workSheet.Cells[last_table_row + 12, 6].Value = "결제 예정일";
            EPP_CellStyle(workSheet.Cells[last_table_row + 12, 6], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 9);

            workSheet.Cells[last_table_row + 13, 6, last_table_row + 19, 7].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            // 2. Detail Condition
            workSheet.Cells[last_table_row + 11, 2].Value = "2. Detail Condition";
            workSheet.Cells[last_table_row + 11, 2].Style.Font.Bold = true;
            workSheet.Cells[last_table_row + 11, 2].Style.Font.Size = 10;
            workSheet.Cells[last_table_row + 12, 2].Value = "   1) 본 견적(구매) 건은 정상 구매 조건이며, 불법 사용에 관련된 구매 건과는 별 건임을 알려드립니다.";
            var cell = workSheet.Cells[last_table_row + 13, 2];
            cell.IsRichText = true;
            var part1 = cell.RichText.Add("   2) Subscription 가입은 필수 계약 사항입니다.  ");
            part1.Bold = false;
            part1.Size = 9;
            var part2 = cell.RichText.Add("숙 지 하 였 음 (서명) ");
            part2.Bold = true;
            part2.Size = 9;
            var part3 = cell.RichText.Add("(고객께서 \"숙지하였음\" 필사와  \"서명\" 부탁드립니다.)");
            part3.Bold = false;
            part3.Size = 9;
            workSheet.Cells[last_table_row + 13, 2].Style.Font.Size = 9;
            //workSheet.Cells[totalRows + 27, 2].Value = "   2) Subscription 가입은 필수 계약 사항입니다.  숙 지 하 였 음 (서명) (고객께서 \"숙지하였음\" 필사와  \"서명\" 부탁 드립니다.)";
            workSheet.Cells[last_table_row + 14, 2].Value = "     : 제품 보완, 최신버전, 기술지원을 위한 필수적인 사항이며, 미 갱신으로 인한 제품 사용에 대한 불편이 발생 시 지원이 불가 합니다.";
            workSheet.Cells[last_table_row + 15, 2].Value = "   3) 납품 후 30일 이내 현금 지불 조건입니다.";
            workSheet.Cells[last_table_row + 16, 2].Value = "   4) 상기 가격은 공급사인 다쏘시스템 솔리드웍스의 가격 정책에 따라 예고 없이 변경될 수 있습니다.";
            workSheet.Cells[last_table_row + 17, 2].Value = "   5) 상기 견적은 발행일로부터 14일간 유효합니다.";
            workSheet.Cells[last_table_row + 12, 2, last_table_row + 17, 2].Style.Font.Size = 9;

            // 공인리셀러 정보
            workSheet.Cells[last_table_row + 20, 2].Style.Font.Size = 10;
            workSheet.Cells[last_table_row + 20, 2].Style.Font.UnderLine = true;
            workSheet.Cells[last_table_row + 20, 2].Value = "공인리셀러 정보";
            workSheet.Cells[last_table_row + 20, 2, last_table_row + 20, 7].Merge = true;
            workSheet.Cells[last_table_row + 20, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[last_table_row + 20, 2].Style.Font.Bold = true;


            for (var i = 0; i < 4; i++)
            {
                workSheet.Cells[last_table_row + 22 + i, 2].Value = "•";
                workSheet.Cells[last_table_row + 22 + i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            workSheet.Cells[last_table_row + 22, 3].Value = "사업자 등록번호 : 135-86-01726";
            workSheet.Cells[last_table_row + 23, 3].Value = "상      호  :  ㈜솔코";
            workSheet.Cells[last_table_row + 24, 3].Value = "업      태  :  서비스(사업관련)업 도소매";
            workSheet.Cells[last_table_row + 25, 3].Value = "주      소  :  경기도 의왕시 이미로 40, C-414 (포일동, 인덕원아이티밸리)";

            workSheet.Cells[last_table_row + 22, 4].Value = "• 법인등록번호  :  135811-0163732";
            workSheet.Cells[last_table_row + 23, 4].Value = "• 대표이사  :  박 광 수                        (인)";
            workSheet.Cells[last_table_row + 24, 4].Value = "• 종      목  :  기술, 설계용역, 컴퓨터 및 주변장치, 소프트웨어자문, 개발 및 공급";
            workSheet.Cells[last_table_row + 22, 2, last_table_row + 35, 4].Style.Font.Size = 9;

            // 직인 넣기
            System.Drawing.Image stamp = System.Drawing.Image.FromFile("C:\\Users\\solko\\Desktop\\solko_stamp.png");
            var solkoStamp = workSheet.Drawings.AddPicture("solkoStamp", stamp);
            solkoStamp.SetPosition(last_table_row + 21, 0, 5, 0);
            solkoStamp.SetSize(75, 68);


            // 컬럼 폭 조정
            workSheet.Column(1).Width = (double)3.75;
            workSheet.Column(2).Width = (double)12.63;
            workSheet.Column(3).Width = (double)70.13;
            workSheet.Column(4).Width = (double)6.50;
            workSheet.Column(5).Width = (double)20.13;
            workSheet.Column(6).Width = (double)20.13;
            workSheet.Column(7).Width = (double)20.13;
            workSheet.Column(8).Width = (double)3.75;

            // 행 폭 조정
            workSheet.Row(1).Height = (double)21.75;
            workSheet.Row(2).Height = (double)21.75;
            workSheet.Row(3).Height = (double)75;
            workSheet.Row(last_table_row + 7).Height = (double)25.5;
            workSheet.Row(last_table_row + 8).Height = (double)20;
            workSheet.Row(last_table_row + 9).Height = (double)20;

            //pck.SaveAs(new FileInfo(filePath));
            //Debug.WriteLine("filepath:" + filePath);

            //Write it back to the client
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment;  filename=ProductDetails.xlsx");
            //Response.BinaryWrite(pck.GetAsByteArray());

            string excelName = string.Format("Offer_{0}_{1}", compName, DateTime.Now.ToLongDateString());
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + excelName + ".xlsx");
                pck.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
    }

    protected void estimatebtn_Template(object sender, EventArgs e)
    {
        var excelToExport = new FileInfo("C:\\Users\\solko\\Desktop\\template.xlsx");
        using (var excelPackage = new ExcelPackage(excelToExport))
        {
            DataTable dts = (DataTable)Session["GridData"];
            string compName = (string)Session["BK_companyName"];
            DataSet ds = new DataSet();
            ds.Tables.Add(dts);
            string filePath = string.Format("C:/VS/Offer_{0}_{1}.xlsx", compName, DateTime.Now.ToShortDateString());


            excelPackage.Save();
        }
    }

    protected void estimatebtn_Click2(object sender, EventArgs e)
    {
        DataTable dts = (DataTable)Session["GridData"];
        string compName = (string)Session["BK_companyName"];
        DataSet ds = new DataSet();
        ds.Tables.Add(dts);
        string filePath = string.Format("C:/VS/Offer_{0}_{1}.xlsx", compName, DateTime.Now.ToShortDateString());

        int totalCols = GridView2.Columns.Count;
        int totalRows = GridView2.Rows.Count;
        //Debug.WriteLine("totalcols" + totalCols + "totalrows" + totalRows);

        using (ExcelPackage pck = new ExcelPackage())
        {
            ExcelWorksheet workSheet = pck.Workbook.Worksheets.Add(dts.TableName);

            Debug.WriteLine("totalcols" + totalCols + "totalrows" + totalRows);
            GridViewRow headerRow = GridView2.HeaderRow;

            //Set style
            workSheet.Cells.Style.Font.Name = "맑은 고딕";
            workSheet.Cells[1, 1, totalRows + 40, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[1, 1, totalRows + 40, 8].Style.Fill.BackgroundColor.SetColor(Color.White);

            //날짜 넣기
            workSheet.Cells[2, 7].Value = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");
            EPP_CellStyle(workSheet.Cells[2, 7], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, verBottom, horCenter, false, true, 10);

            //Solko Logo 넣기
            System.Drawing.Image logo = System.Drawing.Image.FromFile("C:\\Users\\solko\\Desktop\\solko_logo.jpg");
            var solkoLogo = workSheet.Drawings.AddPicture("solkoLogo", logo);
            solkoLogo.SetPosition(2, 0, 1, 0);
            solkoLogo.SetSize(252, 46);

            //Sales Quotation
            workSheet.Cells[3, 1].Value = "Sales Quotation";
            EPP_CellStyle(workSheet.Cells[3, 1], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, verBottom, horCenter, false, true, 28);
            workSheet.Cells[3, 1].Style.Font.UnderLine = true;
            workSheet.Cells[3, 1, 3, 8].Merge = true;

            //Solko 정보
            workSheet.Cells[7, 2].Value = "VAR: ";
            workSheet.Cells[7, 3].Value = "(주)솔코";
            workSheet.Cells[8, 2].Value = "Rep: ";
            workSheet.Cells[9, 2].Value = "Tel: ";
            workSheet.Cells[9, 3].Value = "031-8069-8306 || ";
            workSheet.Cells[10, 2].Value = "Fax: ";
            workSheet.Cells[10, 3].Value = "031-8069-8301";
            workSheet.Cells[11, 2].Value = "Email: ";

            //고객 정보
            workSheet.Cells[7, 6].Value = "Customer: ";
            workSheet.Cells[7, 7].Value = (string)Session["BK_companyName"];
            workSheet.Cells[8, 6].Value = "Rep: ";
            workSheet.Cells[9, 6].Value = "Tel: ";
            workSheet.Cells[10, 6].Value = "Email: ";

            workSheet.Cells["B7:G11"].Style.Font.Size = 10;
            workSheet.Cells["B7:B11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells["C7:C11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["F7:F11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells["G7:G11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells[1, 6].Value = "Forwarding D/C";
            workSheet.Cells[1, 7].Value = forwardDiscount.Text + "%";

            workSheet.Cells[2, 6].Value = "Backdating D/C";
            workSheet.Cells[2, 7].Value = backDiscount.Text + "%";

            //Proposed Price
            workSheet.Cells[13, 2].Value = "▶ Proposed Price  :  ";
            workSheet.Cells[13, 2].Style.Font.Size = 14;
            workSheet.Cells[13, 2].Style.Font.Bold = true;

            //견적서 Table
            workSheet.Cells[15, 2].Value = "1. Solution";
            workSheet.Cells[15, 2].Style.Font.Size = 10;
            workSheet.Cells[15, 2].Style.Font.Bold = true;

            workSheet.Cells[15, 7].Value = "(Unit: WON)";
            workSheet.Cells[15, 7].Style.Font.Size = 8;
            workSheet.Cells[15, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells[16, 2].Value = "No.";
            workSheet.Cells[16, 3].Value = "Description";
            workSheet.Cells[16, 4].Value = "Seats";
            workSheet.Cells[16, 5].Value = "Customer RRP" + ((char)10).ToString() + "(소비자 단가)";
            workSheet.Cells[16, 6].Value = "Proposed Price" + ((char)10).ToString() + "(제안 단가)";
            workSheet.Cells[16, 7].Value = "Proposed Price" + ((char)10).ToString() + "(최종 견적가)";

            workSheet.Row(17).Height = (double)7.00;

            using (var range = workSheet.Cells[16, 2, 16, 7])
            {
                EPP_CellStyle(range, lineThin, lineThin, lineThin, lineThin, verCenter, horCenter, false, true, 10);
                EPP_CellStyle(workSheet.Cells[16, 2], lineThin, ExcelBorderStyle.None, lineThin, lineThin, verCenter, horCenter, false, true, 10);
                EPP_CellStyle(workSheet.Cells[16, 7], lineThin, lineThin, lineThin, ExcelBorderStyle.None, verCenter, horCenter, false, true, 10);
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9BC2E6"));
                workSheet.Row(16).Height = (double)50.0;
            }

            //Total Columns
            workSheet.Cells[totalRows + 21, 2].Value = "Sub Total";
            workSheet.Cells[totalRows + 21, 2, totalRows + 21, 4].Merge = true;
            workSheet.Cells[totalRows + 21, 2, totalRows + 21, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //workSheet.Cells[totalRows + 21, 2, totalRows + 21, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
            workSheet.Cells[totalRows + 22, 2].Value = "VAT";
            workSheet.Cells[totalRows + 22, 2, totalRows + 22, 4].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 22, 2, totalRows + 22, 4].Merge = true;
            workSheet.Cells[totalRows + 22, 2, totalRows + 22, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[totalRows + 23, 2].Value = "Total Price";
            workSheet.Cells[totalRows + 23, 2, totalRows + 23, 4].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 23, 2, totalRows + 23, 4].Merge = true;
            workSheet.Cells[totalRows + 23, 3, totalRows + 23, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            EPP_CellStyle(workSheet.Cells[totalRows + 21, 2, totalRows + 23, 4], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, true, 10);

            //Cell에 값 넣기
            for (var j = 1; j <= totalRows - 2; j++)
            {
                for (var i = 1; i <= 2; i++)
                {

                    string product = GridView2.Rows[j - 1].Cells[i - 1].Text;
                    Debug.WriteLine("product: " + product);

                    //Blank Cell 일 경우
                    if (product == "&nbsp;")
                    {
                        product = " ";
                    }
                    workSheet.Cells[j + 17, i + 1].Value = product;
                }

                for (var i = 4; i <= 7; i++)
                {
                    string product = GridView2.Rows[j - 1].Cells[i - 1].Text;

                    //Blank Cell 일 경우
                    if (product == "&nbsp;")
                    {
                        product = " ";
                    }
                    workSheet.Cells[j + 17, i].Value = product;

                    if (i == 7)
                    {
                        workSheet.Cells[j + 17, i].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FF0000"));
                    }
                }

            }

            //Customer RRP 계산하기
            //제품 개수
            int num_of_prod = (totalRows - 2) / 3;

            string form = "D18 * E18";
            for (var i = 1; i < num_of_prod + 1; i++)
            {
                form += ", D" + (18 + 3 * i) + "* E" + (18 + 3 * i);
            }
            workSheet.Cells[totalRows + 21, 5].Formula = "ROUND(SUM(" + form + "), -3)";
            workSheet.Cells[totalRows + 22, 5].Formula = "ROUND(E" + (totalRows + 21) + "* 0.1, -3)";
            workSheet.Cells[totalRows + 23, 5].Formula = "ROUND(SUM(E" + (totalRows + 21) + ", E" + (totalRows + 22) + "), -3)";

            //최종 견적가 계산하기
            string final_form = "D18 * E18";
            for (var i = 1; i < num_of_prod + 1; i++)
            {
                final_form += ", D" + (18 + 3 * i) + "* E" + (18 + 3 * i);
            }
            workSheet.Cells[totalRows + 21, 7].Formula = "ROUND(SUM(" + final_form + "), -3)";
            workSheet.Cells[totalRows + 22, 7].Formula = "ROUND(E" + (totalRows + 21) + "* 0.1, -3)";
            workSheet.Cells[totalRows + 23, 7].Formula = "ROUND(SUM(E" + (totalRows + 21) + ", E" + (totalRows + 22) + "), -3)";


            int total = Int32.Parse(GridView2.Rows[totalRows - 1].Cells[5].Text, System.Globalization.NumberStyles.AllowThousands);
            double vat = total * 0.1;
            workSheet.Cells[totalRows + 21, 5].Value = total.ToString("N0");
            //workSheet.Cells[totalRows + 21, 5].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 22, 5].Value = vat.ToString("N0"); ;
            workSheet.Cells[totalRows + 23, 5].Value = (total + vat).ToString("N0"); ;
            workSheet.Cells[totalRows + 23, 5].Style.Font.Bold = true;
            using (var range = workSheet.Cells[totalRows + 21, 5, totalRows + 23, 7])
            {
                EPP_CellStyle(range, lineThin, lineThin, lineThin, lineThin, verCenter, horRight, false, true, 10);
            }

            workSheet.Cells[totalRows + 22, 5, totalRows + 22, 7].Style.Font.Bold = false;

            //Table Style
            EPP_CellStyle(workSheet.Cells[17, 2, totalRows + 20, 2], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 3, totalRows + 20, 3], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horLeft, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 4, totalRows + 20, 4], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 5, totalRows + 20, 5], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 6, totalRows + 20, 6], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, lineThin, verCenter, horRight, false, false, 10);
            EPP_CellStyle(workSheet.Cells[17, 7, totalRows + 20, 7], ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, ExcelBorderStyle.None, verCenter, horRight, false, false, 10);
            workSheet.Cells[totalRows + 21, 2, totalRows + 21, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[totalRows + 22, 2, totalRows + 22, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[totalRows + 23, 2, totalRows + 23, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[totalRows + 23, 2, totalRows + 23, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[16, 2, totalRows + 23, 7].Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#808080"));
            workSheet.Cells[17, 2, totalRows + 17, 7].Style.Font.Name = "Calibri";
            workSheet.Cells[16, 2, 16, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
            workSheet.Cells[totalRows + 21, 2, totalRows + 21, 7].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));

            //발주 확인
            workSheet.Cells[totalRows + 25, 6].Value = "발주 확인 (회사명판)";
            workSheet.Cells[totalRows + 25, 6].Style.Font.Size = 10;
            workSheet.Cells[totalRows + 25, 6].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 25, 6, totalRows + 25, 7].Merge = true;
            EPP_CellStyle(workSheet.Cells[totalRows + 25, 6, totalRows + 25, 7], lineThin, ExcelBorderStyle.None, lineThin, ExcelBorderStyle.None, verCenter, horCenter, false, true, 10);
            workSheet.Cells[totalRows + 25, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[totalRows + 25, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[totalRows + 25, 6].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));

            workSheet.Cells[totalRows + 26, 6].Value = "결제 예정일";
            EPP_CellStyle(workSheet.Cells[totalRows + 26, 6], ExcelBorderStyle.None, ExcelBorderStyle.None, ExcelBorderStyle.None, lineThin, verCenter, horCenter, false, false, 9);

            workSheet.Cells[totalRows + 27, 6, totalRows + 33, 7].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            //2.Detail Condition
            workSheet.Cells[totalRows + 25, 2].Value = "2. Detail Condition";
            workSheet.Cells[totalRows + 25, 2].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 25, 2].Style.Font.Size = 10;
            workSheet.Cells[totalRows + 26, 2].Value = "   1) 본 견적(구매)건은 정상 구매 조건이며, 불법 사용에 관련된 구매 건과는 별 건임을 알려드립니다.";
            var cell = workSheet.Cells[totalRows + 27, 2];
            cell.IsRichText = true;
            var part1 = cell.RichText.Add("   2) Subscription 가입은 필수 계약 사항입니다.  ");
            part1.Bold = false;
            part1.Size = 9;
            var part2 = cell.RichText.Add("숙 지 하 였 음 (서명) ");
            part2.Bold = true;
            part2.Size = 9;
            var part3 = cell.RichText.Add("(고객께서 \"숙지하였음\" 필사와  \"서명\" 부탁 드립니다.)");
            part3.Bold = false;
            part3.Size = 9;
            workSheet.Cells[totalRows + 27, 2].Style.Font.Size = 9;
            workSheet.Cells[totalRows + 27, 2].Value = "   2) Subscription 가입은 필수 계약 사항입니다.  숙 지 하 였 음 (서명) (고객께서 \"숙지하였음\" 필사와  \"서명\" 부탁 드립니다.)";
            workSheet.Cells[totalRows + 28, 2].Value = "     : 제품 보완, 최신버전, 기술지원을 위한 필수적인 사항이며, 미 갱신으로 인한 제품 사용에 대한 불편이 발생 시 지원이 불가 합니다.";
            workSheet.Cells[totalRows + 29, 2].Value = "   3) 납품후 30일 이내 현금 지불 조건입니다.";
            workSheet.Cells[totalRows + 30, 2].Value = "   4) 상기 가격은 공급사인 다쏘시스템 솔리드웍스의 가격 정책에 따라 예고 없이 변경될 수 있습니다.";
            workSheet.Cells[totalRows + 31, 2].Value = "   5) 상기 견적은 발행일로부터 14일 간 유효 합니다.";
            workSheet.Cells[totalRows + 26, 2, totalRows + 31, 2].Style.Font.Size = 9;

            //공인리셀러 정보
            workSheet.Cells[totalRows + 34, 2].Value = "공인리셀러 정보";
            workSheet.Cells[totalRows + 34, 2].Style.Font.Size = 10;
            workSheet.Cells[totalRows + 34, 2].Style.Font.Bold = true;
            workSheet.Cells[totalRows + 34, 2].Style.Font.UnderLine = true;
            workSheet.Cells[totalRows + 34, 2, totalRows + 34, 7].Merge = true;
            workSheet.Cells[totalRows + 34, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            for (var i = 0; i < 4; i++)
            {
                workSheet.Cells[totalRows + 36 + i, 2].Value = "•";
                workSheet.Cells[totalRows + 36 + i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            workSheet.Cells[totalRows + 36, 3].Value = "사업자 등록번호 : 135-86-01726";
            workSheet.Cells[totalRows + 37, 3].Value = "상      호  :  ㈜솔코";
            workSheet.Cells[totalRows + 38, 3].Value = "업      태  :  서비스(사업관련)업 도소매";
            workSheet.Cells[totalRows + 39, 3].Value = "주      소  :  경기도 의왕시 이미로 40, C-414 (포일동, 인덕원아이티밸리)";

            workSheet.Cells[totalRows + 36, 4].Value = "• 법인등록번호  :  135811-0163732";
            workSheet.Cells[totalRows + 37, 4].Value = "• 대표이사  :  박 광 수                        (인)";
            workSheet.Cells[totalRows + 38, 4].Value = "• 종      목  :  기술, 설계용역, 컴퓨터 및 주변장치, 소프트웨어자문, 개발 및 공급";
            workSheet.Cells[totalRows + 36, 2, totalRows + 39, 4].Style.Font.Size = 9;

            //직인 넣기
            System.Drawing.Image stamp = System.Drawing.Image.FromFile("C:\\Users\\solko\\Desktop\\solko_stamp.png");
            var solkoStamp = workSheet.Drawings.AddPicture("solkoStamp", stamp);
            solkoStamp.SetPosition(totalRows + 34, 0, 5, 0);
            solkoStamp.SetSize(100, 92);


            //컬럼 폭 조정
            workSheet.Column(1).Width = (double)3.75;
            workSheet.Column(2).Width = (double)12.63;
            workSheet.Column(3).Width = (double)70.13;
            workSheet.Column(4).Width = (double)6.50;
            workSheet.Column(5).Width = (double)20.13;
            workSheet.Column(6).Width = (double)20.13;
            workSheet.Column(7).Width = (double)20.13;
            workSheet.Column(8).Width = (double)3.75;

            //행 폭 조정
            workSheet.Row(1).Height = (double)21.75;
            workSheet.Row(2).Height = (double)21.75;
            workSheet.Row(3).Height = (double)75;
            workSheet.Row(totalRows + 21).Height = (double)25.5;
            workSheet.Row(totalRows + 22).Height = (double)20;
            workSheet.Row(totalRows + 23).Height = (double)20;

            //소비자 단가, 제안 단가, 최종 금액 Column
            foreach (int i in new int[] { 5, 6, 7 })
            {
                for (int j = 5; j <= totalRows; j += 3)
                {
                    EPP_CellStyle(workSheet.Cells[j, i, j + 2, i], lineThin, lineThin, lineThin, lineThin, verBottom, horRight, true, false);
                }
            }

            string usd = workSheet.Cells[totalRows + 4, 5, totalRows + 4, 5].Value.ToString();
            string krw = workSheet.Cells[totalRows + 4, 6, totalRows + 4, 6].Value.ToString();
            string percentage = workSheet.Cells[totalRows + 4, 7, totalRows + 4, 7].Value.ToString();

            workSheet.Cells[totalRows + 4, 5, totalRows + 4, 5].Value = "USD: " + usd;
            workSheet.Cells[totalRows + 4, 6, totalRows + 4, 6].Value = "KRW: " + krw;
            workSheet.Cells[totalRows + 4, 7, totalRows + 4, 7].Value = percentage + "%";

            pck.SaveAs(new FileInfo(filePath));
            Debug.WriteLine("filepath:" + filePath);

            //Write it back to the client
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=ProductDetails.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());

            string excelName = string.Format("Offer_{0}_{1}", compName, DateTime.Now.ToLongDateString());
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + excelName + ".xlsx");
                pck.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

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

    protected string NormalizeToDecimal(string val)
    {
        if (IsNull(val)) return "";

        return val.Replace(",", "").Replace("$", "").Replace("\\", "");
    }

    protected long NormalizeToLong(string val)
    {
        if (IsNull(val)) return 0;

        string normalized = val.Replace(",", "").Replace("$", "").Replace("\\", "");

        return long.Parse(normalized);
    }


    private static void EPP_CellStyle(ExcelRange range, ExcelBorderStyle top, ExcelBorderStyle left, ExcelBorderStyle bottom, ExcelBorderStyle right, ExcelVerticalAlignment verAlign, ExcelHorizontalAlignment horAlign, bool wrapText = false, bool fontBold = false, int fontSize = 10)
    {
        range.Style.Font.Size = fontSize;
        range.Style.Font.Bold = fontBold;

        //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //range.Style.Fill.BackgroundColor.SetColor(Color.White);
        //range.Style.Font.Color.SetColor(Color.Black);

        range.Style.Border.Top.Style = top;
        range.Style.Border.Left.Style = left;
        range.Style.Border.Right.Style = right;
        range.Style.Border.Bottom.Style = bottom;

        range.Style.VerticalAlignment = verAlign;
        range.Style.HorizontalAlignment = horAlign;

        range.Style.WrapText = wrapText;
    }

    // 이 버튼 안써
    protected void applybtn_Click(object sender, EventArgs e)
    {
        string org_id_list = (string)Session["BK_sendIDList"];
        string org_companyName = (string)Session["BK_companyName"];


        Session["sendIDList"] = org_id_list;
        Session["companyName"] = org_companyName;

        ReloadDataAndPrintOutPage(org_id_list, org_companyName);

        return;
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

    protected long rounding(double price)
    {
        //double v1 = double.Parse(price);

        double v2 = price * 0.001;

        double v3 = Math.Round(v2);

        long v4 = (long)v3;

        long v5 = v4 * 1000;

        return v5;
    }

    #region "안써"
    protected void GrantReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        //txtComment.Text = GridView1.Rows(Int32.Parse(arg[0])).col            ;
        GridViewRow row = GridView2.Rows[Int32.Parse(arg[0])];
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
        GridViewRow row = GridView2.Rows[Int32.Parse(arg[0])];
        txtRowID.Text = arg[1];
        ClientScriptManager sm = Page.ClientScript;
        string script = "<script>$('#modalDel').modal('show'); </script>";
        sm.RegisterStartupScript(this.GetType(), "sm", script);
        hidemodalDel.Checked = true;
    }

    protected void RejectReq(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        GridViewRow row = GridView2.Rows[Int32.Parse(arg[0])];
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

}

