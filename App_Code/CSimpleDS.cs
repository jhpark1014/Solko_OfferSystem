using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CSimpleDS
/// </summary>
public class CSimpleDS
{
    DataSet mDS = null;

    public CSimpleDS()
    {
        this.mDS = new DataSet();
    }

    public void SetDataSet(DataSet ds)
    {
        //this.mDS = ds.Clone();
        this.mDS = ds.Copy();
    }

    public DataSet GetDataSet()
    {
        if (this.mDS != null)
        {
            DataSet ds = this.mDS.Copy();
            return ds;
        }

        return new DataSet();
    }


    public DataTable GetTable()
    {
        if (this.mDS.Tables.Count <= 0)
            this.mDS.Tables.Add();

        return this.mDS.Tables[0];
    }

    private DataRow GetRow(int row)
    {
        DataTable dt = GetTable();
        if (dt.Rows.Count <= row)
        {
            do
            {
                DataRow newrow = dt.NewRow();
                dt.Rows.Add(newrow);
            } while (dt.Rows.Count <= row);
        }

        return dt.Rows[row];
    }

    private void AddColumn(string colName)
    {
        colName = colName.ToUpper();

        DataTable dt = GetTable();
        if (dt.Columns.Contains(colName)) return;
        dt.Columns.Add(colName);
    }

    private void AddColumn(DataTable dt, string colName)
    {
        colName = colName.ToUpper();

        if (dt.Columns.Contains(colName)) return;
        dt.Columns.Add(colName);
    }



    public void SetValue(int row, string colName, string Value)
    {
        if (row < 0) throw new Exception("minimum row index is 0");
        if (string.IsNullOrEmpty(colName)) throw new Exception("column name is mandatory");

        colName = colName.ToUpper();

        DataTable dt = GetTable();
        AddColumn(dt, colName);

        DataRow aRow = GetRow(row);
        aRow[colName] = Value;
    }

    public int GetRowCount()
    {
        DataTable dt = GetTable();
        return dt.Rows.Count;
    }

    public string GetValueAsString(int row, string colName)
    {
        colName = colName.ToUpper();
        DataTable dt = GetTable();

        if (row >= dt.Rows.Count || row < 0) return "";
        if (!dt.Columns.Contains(colName)) return "";

        Object returnStr = null;
        try
        {
            returnStr = dt.Rows[row][colName];
            if (returnStr == null || returnStr == DBNull.Value) return "";
            return Convert.ToString(returnStr);
        }
        catch
        {
            return "";
        }
    }

}