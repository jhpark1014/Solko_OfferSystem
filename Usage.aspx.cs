using System;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;
using System.Diagnostics;
using LicenseCheck;
using System.Text;

public partial class Usage : System.Web.UI.Page
{
    private DataTable dt;
    private string _sortDirection;
    Image sortImage = new Image();

    static string _pageSortedKey = "Usage_SortedView";  // 페이지의 정렬데이터 키값
    static string _pageFilterKey = "Usage_FilterText";  // 페이지의 필터링데이터 키값
    protected void Page_Load(object sender, EventArgs e)
    {
        if( !Page.IsPostBack )
        {
            BindGridView();
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        //DB DB = new DB();
        //SqlConnection dbConn = DB.DbOpen();
        //SqlDataAdapter da = new SqlDataAdapter(new SqlCommand("SELECT * FROM View_Usage ORDER BY RunDate DESC;", dbConn));
        //dt = new DataTable();
        //da.Fill(dt);
        //GridView1.DataSource = dt;
        //GridView1.DataBind();
        //dbConn.Close();

    }
    protected void BindGridView()
    {
        string sqlALL = "SELECT * FROM View_Usage ORDER BY RunDate DESC;";

        if(Session[_pageFilterKey] != null )
        {
            string tempFilter = (string)Session[_pageFilterKey];
            if( !string.IsNullOrEmpty(tempFilter) )
            {
                string sql1 = "SELECT * FROM View_Usage ";
                string sql2 = " WHERE ";
                string sql3 = " ORDER BY RunDate DESC;";

                // 조건절 생성
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < GridView1.Columns.Count; i++)
                {
                    if (GridView1.Columns[i].GetType().Equals(typeof(System.Web.UI.WebControls.BoundField)))
                    {
                        if (sb.Length > 0) sb.Append(" OR ");

                        System.Web.UI.WebControls.BoundField col = (System.Web.UI.WebControls.BoundField)GridView1.Columns[i];
                        sb.Append(col.DataField);
                        sb.Append(" like '%");
                        sb.Append(tempFilter);
                        sb.Append("%' ");
                    }
                }

                // 조건절 조합
                if (sb.Length > 0)
                    sql2 = sql2 + sb.ToString();
                else
                    sql2 = "";

                sqlALL = sql1 + sql2 + sql3;
            }
        }

        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(sqlALL, dbConn));
        DataTable tempDT = new DataTable();
        da.Fill(tempDT);
        dt = tempDT;
        dbConn.Close();
    }
    protected void OnPaging(object sender, GridViewPageEventArgs e)
    {
        GridView1.PageIndex = e.NewPageIndex;
        if( Session[_pageSortedKey] != null )
        {
            GridView1.DataSource = Session[_pageSortedKey];
            GridView1.DataBind();
        }
        else
        {
            BindGridView();
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }
    }

    protected void GridView1_Sorting(object sender, GridViewSortEventArgs e)
    {
        SetSortDirection(SortDireaction);

        BindGridView();
        DataView sortedView = new DataView(dt);
        sortedView.Sort = e.SortExpression + " " + _sortDirection;
        Session[_pageSortedKey] = sortedView;
        GridView1.DataSource = sortedView;
        GridView1.DataBind();

        SortDireaction = _sortDirection;
        int columnIndex = 0;
        foreach (DataControlFieldHeaderCell headerCell in GridView1.HeaderRow.Cells)
        {
            if (headerCell.ContainingField.SortExpression == e.SortExpression)
            {
                columnIndex = GridView1.HeaderRow.Cells.GetCellIndex(headerCell);
            }
        }

        if( sortImage != null )
            GridView1.HeaderRow.Cells[columnIndex].Controls.Add(sortImage);
    }
    protected void SetSortDirection(string sortDirection)
    {
        if (sortDirection == "ASC")
        {
            _sortDirection = "DESC";
            //sortImage.ImageUrl = "view_sort_ascending.png";
            sortImage.ImageUrl = "view_sort_descending.png";
        }
        else
        {
            _sortDirection = "ASC";
            //sortImage.ImageUrl = "view_sort_descending.png";
            sortImage.ImageUrl = "view_sort_ascending.png";
        }
    }
    public string SortDireaction {
        get {
            if (ViewState["SortDireaction"] == null)
                return string.Empty;
            else
                return ViewState["SortDireaction"].ToString();
        }
        set {
            ViewState["SortDireaction"] = value;
        }
    }
    protected void btnSearchClick(object sender, EventArgs e)
    {
        string sSearch = txtSearch.Text;
        sSearch = sSearch.Trim();

        if (string.IsNullOrEmpty(sSearch))
            Session[_pageFilterKey] = "";
        else
            Session[_pageFilterKey] = sSearch;

        Session[_pageSortedKey] = null;
        _sortDirection = "";

        BindGridView();
        GridView1.DataSource = dt;
        GridView1.DataBind();
    }
    protected void msgb(string MSG)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + MSG + "');", true);
    }
}

