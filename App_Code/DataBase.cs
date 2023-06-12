using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// DataBase의 요약 설명입니다.
/// </summary>
public class DB
{

    public SqlConnection DbOpen()
    {
        try
        {
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
            dbConnection.Open();
            return dbConnection;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public SqlDataReader QueryReader(SqlConnection dbConnection, string @Query)
    {
        try
        {
            SqlCommand dbCmd = new SqlCommand();
            dbCmd.Connection = dbConnection;
            dbCmd.CommandText = Query;
            dbCmd.CommandType = CommandType.Text;
            dbCmd.Connection = dbConnection;
            return dbCmd.ExecuteReader();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    //public SqlDataAdapter  QueryAdapter(SqlConnection dbConnection, string @Query)
    //{
    //    try
    //    {
    //        SqlCommand dbCmd = new SqlCommand();
    //        dbCmd.Connection = dbConnection;
    //        dbCmd.CommandText = Query;
    //        dbCmd.CommandType = CommandType.Text;
    //        dbCmd.Connection = dbConnection;
    //        SqlDataAdapter aa = dbCmd.ExecuteReader();
    //        return dbCmd.ExecuteReader();
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }
    //}
    public string convSql(string value, Boolean comma = false)
    {
        string com = "";
        if (comma)
        {
            com = ",";
        }
        value = value.Trim();
        return "'" + value + "'" + com;
    }
    public int QueryInsert(SqlConnection dbConnection, string @Query)
    {
        try
        {
            SqlCommand dbCmd = new SqlCommand();
            dbCmd.Connection = dbConnection;
            dbCmd.CommandText = Query;
            dbCmd.CommandType = CommandType.Text;
            dbCmd.Connection = dbConnection;
            return dbCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }
    public SqlConnection DbClose(SqlConnection dbConnection)
    {
        dbConnection.Close();
        return dbConnection;
    }
}