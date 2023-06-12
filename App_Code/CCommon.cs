using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Summary description for CCommon
/// </summary>
public class CCommon
{

    public static string vbCr = "\r";
    public static string vbLf = "\n";
    public static string vbCrLf = "\r\n";

    public static char[] splitSpace = { ' ' };
    public static char[] splitComma = { ',' };
    public static char[] splitColon = { ':' };
    public static char[] splitSemiColon = { ';' };
    public static char[] splitDash = { '-' };
    public static char[] splitReverseSlash = { '\\' };
    public static char[] splitSlash = { '/' };

    public static char[] splitDiv1 = { '†' };
    public static char[] splitDiv2 = { '‡' };




    public static string GetComputerName()
    {
        return System.Environment.MachineName;
    }

    public static string GetTimeToCompare()
    {
        return DateTime.Now.ToString("yyyyMMdd.HHmm");
    }

    public static string GetTimeToCompare(DateTime dt)
    {
        return dt.ToString("yyyyMMdd.HHmm");
    }

    public static string GetTimeToReport(DateTime dt)
    {
        return dt.ToString("yyyy/MM/dd HH:mm");
    }

    public static string GetNowFullString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
    }

    public static string GetNowFullSimple()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
    }

    public static string GetNowTimeSimple()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }

    public static object checkNull(Object obj)
    {
        // 입력문자가 null인가 체크 후 string으로 리턴
        if (obj == null || obj == DBNull.Value) return "";
        return obj;
    }

    public static bool isNull(string str)
    {
        // 입력문자가 null인가 체크 후 string으로 리턴
        // if (obj == null || obj == DBNull.Value) return "";
        if (string.IsNullOrEmpty(str)) return true;
        return false;
    }

    public static bool isNotNull(string str)
    {
        return !isNull(str);
    }

    public static bool isNotDigit(string instr)
    {
        return !isDigit(instr);
    }

    public static bool isDigit(string instr)
    {
        // 입력문자가 숫자로만 구성되었는가 체크
        return Regex.IsMatch(instr, @"^\d+$");
    }

    public static string GetAscCode(string org)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char a in org)
        {
            sb.AppendLine(a + " ==> " + System.Convert.ToInt32(a).ToString());
        }

        return sb.ToString();
    }

    public static string GetNoNewLine(string org)
    {
        org = org.Replace("\r", "");
        org = org.Replace("\n", "");
        return org;
    }

    #region "Row 처리"
    public static int getDSCount(DataSet ds)
    {
        if (ds == null) return 0;
        if (ds.Tables.Count == 0) return 0;
        int returnVal = 0;
        try
        {
            returnVal = ds.Tables[0].Rows.Count;
        }
        catch (Exception ex)
        {
            returnVal = 0;
        }
        return returnVal;
    }
    public static int getDTCount(DataTable ds)
    {
        if (ds == null) return 0;
        int returnVal = 0;
        try
        {
            returnVal = ds.Rows.Count;
        }
        catch
        {
            returnVal = 0;
        }
        return returnVal;
    }

    public static Object getDSValue(DataSet ds, int row, int col)
    {
        if (ds.Tables.Count == 0) return "";
        if (row >= ds.Tables[0].Rows.Count) return "";

        Object returnStr = null;
        try
        {
            returnStr = ds.Tables[0].Rows[row][col];
        }
        catch (Exception ex)
        {
        }

        return checkNull(returnStr);
    }

    public static Object getDTValue(DataTable ds, int row, int col)
    {
        if (row >= ds.Rows.Count) return "";

        Object returnStr = null;
        try
        {
            returnStr = ds.Rows[row][col];
        }
        catch
        {
        }

        return checkNull(returnStr);
    }

    public static Object getDTValue(DataTable ds, int row, string col)
    {
        if (row >= ds.Rows.Count) return "";

        Object returnStr = null;
        try
        {
            returnStr = ds.Rows[row][col];
        }
        catch
        {
        }

        return checkNull(returnStr);
    }

    public static Object getDSValue(DataSet ds, int row, string colname)
    {
        if (ds.Tables.Count == 0) return "";
        if (row >= ds.Tables[0].Rows.Count) return "";

        Object returnStr = null;
        try
        {
            returnStr = ds.Tables[0].Rows[row][colname];
        }
        catch (Exception ex)
        {
        }

        return checkNull(returnStr);
    }

    public static string getDSValueAsString(DataSet ds, int row, string colname)
    {
        Object tempValue = getDSValue(ds, row, colname);
        return (string)tempValue.ToString();
    }

    public static string getDTValueAsString(DataTable ds, int row, string colname)
    {
        Object tempValue = getDTValue(ds, row, colname);
        return (string)tempValue.ToString();
    }

    public static string getDSValueAsString(DataSet ds, int row, int col)
    {
        Object tempValue = getDSValue(ds, row, col);
        return (string)tempValue.ToString();
    }

    public static long getDSValueAsLong(DataSet ds, int row, int col)
    {
        Object tempValue = getDSValue(ds, row, col);
        return long.Parse(tempValue.ToString());
    }

    public static long getDSValueAsLong(DataSet ds, int row, string col)
    {
        Object tempValue = getDSValue(ds, row, col);
        return long.Parse(tempValue.ToString());
    }

    public static Object getDSValue(DataRow dr, string colname)
    {
        Object returnStr = null;
        try
        {
            returnStr = dr[colname];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("컬럼명 없음 1");
        }

        return checkNull(returnStr);
    }

    public static Object getDSValue(DataRow dr, int col)
    {
        Object returnStr = null;
        try
        {
            returnStr = dr[col];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("컬럼명 없음 1");
        }

        return checkNull(returnStr);
    }

    public static string getDSValueAsString(DataRow dr, string colname)
    {
        Object tempValue = getDSValue(dr, colname);
        return tempValue.ToString();
    }

    public static string getDSValueAsString(DataRow dr, int col)
    {
        Object tempValue = getDSValue(dr, col);
        return tempValue.ToString();
    }



    public static void setDSValue(DataSet ds, int row, int col, string val)
    {
        if (ds.Tables.Count == 0) return;
        if (row >= ds.Tables[0].Rows.Count) return;

        try
        {
            ds.Tables[0].Rows[row][col] = val;
        }
        catch
        {
        }
    }


    public static void setDSValue(DataSet ds, int row, string colName, string val)
    {
        if (ds.Tables.Count == 0) return;
        if (row >= ds.Tables[0].Rows.Count) return;

        try
        {
            ds.Tables[0].Rows[row][colName] = val;
        }
        catch
        {
        }
    }


    public static void setDTValue(DataTable ds, int row, int col, string val)
    {
        if (row >= ds.Rows.Count) return;

        try
        {
            ds.Rows[row][col] = val;
        }
        catch
        {
        }
    }

    public static void setDTValue(DataTable ds, int row, string colName, string val)
    {
        if (row >= ds.Rows.Count) return;

        try
        {
            ds.Rows[row][colName] = val;
        }
        catch
        {
        }
    }


    #endregion

    #region "Col 처리"
    public static int getDSHeaderCount(DataSet ds)
    {
        if (ds == null) return 0;
        if (ds.Tables.Count == 0) return 0;
        int returnVal = 0;
        try
        {
            returnVal = ds.Tables[0].Columns.Count;
        }
        catch (Exception ex)
        {
            returnVal = 0;
        }
        return returnVal;
    }

    public static string getDSHeaderByIndex(DataSet ds, int index)
    {
        if (ds == null) return "";
        if (ds.Tables.Count == 0) return "";

        string returnStr = "";
        try
        {
            returnStr = ds.Tables[0].Columns[index].ToString();
        }
        catch (Exception ex)
        {
        }

        return returnStr;
    }


    public static int getDSHeaderIndexByName(DataSet ds, string name)
    {
        if (ds == null) return -1;
        if (ds.Tables.Count == 0) return -1;

        try
        {
            return ds.Tables[0].Columns.IndexOf(name);
        }
        catch
        {
            return -1;
        }
    }

    #endregion

    public static string compressDS(DataSet ds)
    {
        System.IO.MemoryStream memStream = new System.IO.MemoryStream();
        System.IO.Compression.DeflateStream compressedStream = new System.IO.Compression.DeflateStream(memStream, System.IO.Compression.CompressionMode.Compress, true);
        ds.WriteXml(compressedStream, XmlWriteMode.WriteSchema);
        compressedStream.Close();
        memStream.Seek(0, System.IO.SeekOrigin.Begin);

        return Convert.ToBase64String(memStream.GetBuffer(), Base64FormattingOptions.None);
    }

    //public static string compressString(string inStr)
    //{
    //    System.IO.MemoryStream memStream = new System.IO.MemoryStream();
    //    System.IO.Compression.DeflateStream compressedStream = new System.IO.Compression.DeflateStream(memStream, System.IO.Compression.CompressionMode.Compress, true);
    //    byte[] uncompressedBytes = System.Text.Encoding.Default.GetBytes(inStr);
    //    compressedStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
    //    compressedStream.Close();
    //    memStream.Seek(0, System.IO.SeekOrigin.Begin);

    //    return Convert.ToBase64String(memStream.GetBuffer(), Base64FormattingOptions.None);
    //}

    public static DataSet decompressDS(String base64EncodedData)
    {
        DataSet ds = new DataSet();
        if (string.IsNullOrEmpty(base64EncodedData))
            return ds;
        System.IO.MemoryStream memStream = new System.IO.MemoryStream(Convert.FromBase64String(base64EncodedData));
        System.IO.Compression.DeflateStream compressedStream = new System.IO.Compression.DeflateStream(memStream, System.IO.Compression.CompressionMode.Decompress, true);
        //ds.ReadXml(compressedStream);
        ds.ReadXml(compressedStream, XmlReadMode.ReadSchema);
        compressedStream.Close();
        memStream.Seek(0, System.IO.SeekOrigin.Begin);

        return ds;
    }


    public static byte[] ToCompressedByteArray(string source)
    {
        // convert the source string into a memory stream
        using (
            System.IO.MemoryStream inMemStream = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(source)),
            outMemStream = new System.IO.MemoryStream())
        {
            // create a compression stream with the output stream
            using (var zipStream = new System.IO.Compression.DeflateStream(outMemStream, System.IO.Compression.CompressionMode.Compress, true))
                // copy the source string into the compression stream
                inMemStream.WriteTo(zipStream);

            // return the compressed bytes in the output stream
            return outMemStream.ToArray();
        }
    }

    public static string ToUncompressedString(string source)
    {
        // get the byte array representation for the compressed string
        var compressedBytes = Convert.FromBase64String(source);

        // load the byte array into a memory stream
        using (var inMemStream = new System.IO.MemoryStream(compressedBytes))
        // and decompress the memory stream into the original string
        using (var decompressionStream = new System.IO.Compression.DeflateStream(inMemStream, System.IO.Compression.CompressionMode.Decompress))
        using (var streamReader = new System.IO.StreamReader(decompressionStream))
            return streamReader.ReadToEnd();
    }

    public static string compressString(string ds)
    {
        return Convert.ToBase64String(ToCompressedByteArray(ds));
    }

    public static string decompressString(String base64EncodedData)
    {
        return ToUncompressedString(base64EncodedData);

        //System.IO.MemoryStream memStream = new System.IO.MemoryStream(Convert.FromBase64String(base64EncodedData));
        //System.IO.Compression.DeflateStream compressedStream = new System.IO.Compression.DeflateStream(memStream, System.IO.Compression.CompressionMode.Decompress, true);
        //StreamReader sr = new StreamReader(compressedStream);
        //string all = sr.ReadToEnd();
        //compressedStream.Close();
        //sr.Close();
        //memStream.Seek(0, System.IO.SeekOrigin.Begin);

        //return all;
    }

    public static string GetFilenameTransitionStore(string vaultRootPath, string stateName, string docID)
    {
        return string.Format(@"{0}\{1}\{2}.{3}.txt", vaultRootPath, @"LOG\Transition", stateName, docID);
    }


}