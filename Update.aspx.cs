using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
//using INI;
//using System.Windows.Forms;

public partial class Update : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        makeTree();
        if (inputNewFolderPath.Items.Count == 0)
        {
            initCombo();
        }
        String args = Request.QueryString["Path"];
        if (args != null)
        {
            TreeViewProject.FindNode(args).Select();
            TreeViewProject_SelectedNodeChanged(this, EventArgs.Empty);
        }

    }


    protected void initCombo()
    {
        Dictionary<string, string> comboList = new Dictionary<string, string>();
        comboList.Add("%AppPath%", @"설치폴더");
        //comboList.Add("%ApplicationData%", @"ApplicationData (C:\Users\사용자\AppData\Roaming)");
        //comboList.Add("%LocalApplicationData%", @"LocalApplicationData (C:\Users\사용자\AppData\Local)");
        comboList.Add("%System%", @"System (C:\Windows)");
        comboList.Add("%MyDocuments%", @"내 문서");
        comboList.Add("%MyMusic%", @"내 음악");
        comboList.Add("%MyPictures%", @"내 사진");
        comboList.Add("%MyVideos%", @"내 비디오");
        comboList.Add("%StartMenu%", @"시작메뉴");
        inputNewFolderPath.AppendDataBoundItems = true;
        inputNewFolderPath.DataSource = comboList;
        inputNewFolderPath.DataValueField = "Key";
        inputNewFolderPath.DataTextField = "Value";
        inputNewFolderPath.DataBind();

        inputEditFolderPath.AppendDataBoundItems = true;
        inputEditFolderPath.DataSource = comboList;
        inputEditFolderPath.DataValueField = "Key";
        inputEditFolderPath.DataTextField = "Value";
        inputEditFolderPath.DataBind();
        comboList.Clear();
    }

    #region Tree View
    protected void TreeViewProject_SelectedNodeChanged(object sender, EventArgs e)
    {
        TreeChange();
    }
    protected void TreeChange()
    {
        // 폴더 경로 표시 변경
        int nodeDepth = TreeViewProject.SelectedNode.Depth;
        TreeNode tempNode = TreeViewProject.SelectedNode;
        string pathHtml = "";
        string folderPath = "";
        for (int i = nodeDepth; i >= 0; i--)
        {
            pathHtml = "<li>" + tempNode.Text + "</li>" + pathHtml;
            folderPath = tempNode.Text + "\\" + folderPath;
            //else {
            //    pathHtml = "<li><a href = \"#\">" + tempNode.Text + "</a></li>" + pathHtml;
            //}
            tempNode = tempNode.Parent;
        }
        switch (nodeDepth)
        {
            case 0:
                InfoFolder.Enabled = true;
                AddFolder.Enabled = true;
                EditFolder.Enabled = false;
                RemoveFolder.Enabled = false;
                AddMasterFile.Enabled = false;

                inputNewFolderName.Enabled = false;
                inputNewFolderPath.Enabled = true;
                inputEditFolderName.Enabled = false;
                inputEditFolderPath.Enabled = true;

                GridView1.DataSource = null;
                GridView1.DataBind();
                break;
            case 1:
                InfoFolder.Enabled = true;
                AddFolder.Enabled = true;
                EditFolder.Enabled = true;
                RemoveFolder.Enabled = true;
                AddMasterFile.Enabled = true;

                inputNewFolderName.Enabled = true;
                inputNewFolderPath.Enabled = false;
                inputEditFolderName.Enabled = false;
                inputEditFolderPath.Enabled = true;

                GridView1.DataSource = null;
                GridView1.DataBind();
                break;
            default:
                InfoFolder.Enabled = true;
                AddFolder.Enabled = true;
                EditFolder.Enabled = true;
                RemoveFolder.Enabled = true;
                AddMasterFile.Enabled = true;
                inputNewFolderName.Enabled = true;
                inputNewFolderPath.Enabled = false;
                inputEditFolderName.Enabled = true;
                inputEditFolderPath.Enabled = false;
                break;
        }
        FolderPath.InnerHtml = pathHtml;

        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM UpFolder WHERE Id=" + folder(TreeViewProject.SelectedNode.Value));
        while (dbReader.Read())
        {
            lblFolderDescription.Text = "<pre>" + dbReader["Description"].ToString() + "</pre>";
        }
        lblFolderPath.Text = folderPath;
        lblFolderName.Text = TreeViewProject.SelectedNode.Text;
        // 파일 리스트 표시
        if (nodeDepth >= 1)
        {
            createMasterFileListTable(folder(TreeViewProject.SelectedNode.Value));
        }
    }
    public void makeTree()
    {
        if (TreeViewProject.Nodes.Count < 1)
        {
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM UpProject");
            while (dbReader.Read())
            {
                TreeNode rootNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("Name")), dbReader["Id"].ToString());
                rootNode.ChildNodes.Add(addTreeRootNode(dbReader.GetInt32(dbReader.GetOrdinal("Id")), rootNode));
                TreeViewProject.Nodes.Add(rootNode);
            }
            DB.DbClose(dbConn);
        }
    }
    public TreeNode addTreeRootNode(int porjectId, TreeNode nodeFolder)
    {
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_Folders where Root=1 and ProjectId = " + porjectId);
        while (dbReader.Read())
        {
            TreeNode subNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("FolderName")), dbReader["CurrentFolderId"].ToString() + "," + dbReader["Id"].ToString());
            subNode = addTreeChildNode(dbReader.GetInt32(dbReader.GetOrdinal("Id")), subNode);
            nodeFolder.ChildNodes.Add(subNode);
        }
        DB.DbClose(dbConn);
        return nodeFolder;
    }

    public TreeNode addTreeChildNode(int childId, TreeNode nodeFolder)
    {
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_Folders where Root=0 and ParentFolderId = " + childId);
        while (dbReader.Read())
        {
            TreeNode subNode = new TreeNode(dbReader.GetString(dbReader.GetOrdinal("FolderName")), dbReader["CurrentFolderId"].ToString() + "," + dbReader["Id"].ToString());
            subNode = addTreeChildNode(dbReader.GetInt32(dbReader.GetOrdinal("Id")), subNode);
            nodeFolder.ChildNodes.Add(subNode);
        }
        DB.DbClose(dbConn);
        return nodeFolder;
    }
    #endregion

    #region Master File List
    public void createMasterFileListTable(int id = 0)
    {
        if (id > 0)
        {
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT MasterId, FileFullName,size, Version, UploadDate FROM View_FileInFolder WHERE FolderId = " + id);
            GridView1.DataSource = dbReader;
            GridView1.DataBind();
            dbReader.Close();
            dbReader = DB.QueryReader(dbConn, "SELECT * FROM UpFolder WHERE Id = " + id);
            while (dbReader.Read())
            {
                inputEditFolderName.Text = dbReader["FolderName"].ToString();
                if (TreeViewProject.SelectedNode.Depth < 1)
                {
                    inputEditFolderPath.Text = dbReader["SystemPath"].ToString();
                }
                inputEditFolderDescription.Text = dbReader["Description"].ToString();
            }
            dbReader.Close();
            dbConn.Close();
            AddStoredFile.Enabled = false;
            GridView2.DataSource = null;
            GridView2.DataBind();
            FileName.InnerHtml = "<li class=\"active\">No Selected</li>";
        }
    }
    #endregion

    #region Modal Event

    protected void btnAddMasterFile_Click(object sender, EventArgs e)
    {
        if (fup.HasFile)
        {
            // DB 연결
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            try
            {
                // 파일 업로드
                string fileName = @Path.GetFileName(fup.FileName);
                string fileNameNameWithouExtension = @Path.GetFileNameWithoutExtension(fup.FileName);
                string fileNameExtension = @Path.GetExtension(fup.FileName);
                
                string uploadedName = @Server.MapPath("~/temp/") + fileName + ".uploaded";
                fup.PostedFile.SaveAs(uploadedName);
                string hash = GetChecksum(uploadedName);
                string SavedName = hash + ".Uploaded";
                SqlDataReader dbReader = DB.QueryReader(dbConn, "select hash as Count from UpStoredFile where hash = '" + hash + "'");

                Boolean emptyRows = !dbReader.HasRows;
                dbReader.Close();
                //dbCmd.Connection = dbConn;
                dbReader.Close();
                if (emptyRows)
                {
                    // 업로드 파일명 변경 후 업로드 폴더로 이동
                    FileInfo fileRename = new FileInfo(uploadedName);
                    if (fileRename.Exists)
                    {
                        string path = ConfigurationManager.AppSettings["uploadLocation"].ToString();

                        fileRename.MoveTo(path + "/" + SavedName); //이미있으면 에러
                    }
                    // 업데이트 파일 정보 DB업데이트
                    int folderId = folder(TreeViewProject.SelectedNode.Value);
                    //DateTime myDateTime = DateTime.Now;
                    string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    DB.QueryInsert(dbConn, "EXEC UploadFile " + folderId + ",'" + fileNameNameWithouExtension + "','" + fileNameExtension + "','" + SavedName + "','" + hash + "',1,'" + fup.PostedFile.ContentLength.ToString() + "', '" + sqlFormattedDate + "'");
                }
                createMasterFileListTable(folder(TreeViewProject.SelectedNode.Value));
            }
            catch (Exception ex)
            {
                Response.Write(ex);
                //throw;
            }
            dbConn.Close();
        }
    }
    public static string GetChecksum(string sPathFile)
    {
        if (!File.Exists(sPathFile))
            return null;

        using (FileStream stream = File.OpenRead(sPathFile))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] byteChecksum = md5.ComputeHash(stream);
            return BitConverter.ToString(byteChecksum).Replace("-", String.Empty);
        }
    }
    protected void btnAddFolder_Click(object sender, EventArgs e)
    {
        int responseID;
        int ProjectID = 1;
        int FolderID = 0;
        Boolean root = true;
        string folderName = null;
        string systemFolder = null;

        if (TreeViewProject.SelectedNode.Depth > 0)
        {
            folderName = inputNewFolderName.Text;
            FolderID = link(TreeViewProject.SelectedNode.Value);
            systemFolder = null;
            root = false;
            if (TreeViewProject.SelectedNode.Depth > 1)
            {
                FolderID = link(TreeViewProject.SelectedNode.Value);
                systemFolder = null;
                root = false;
            }
        }
        else
        {
            folderName = inputNewFolderPath.SelectedItem.Text;
            systemFolder = inputNewFolderPath.SelectedItem.Value;
            ProjectID = link(TreeViewProject.SelectedNode.Value);
        }
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "EXEC NewFolder " + ProjectID + "," + FolderID + "," + root + ",'" + folderName + "','" + systemFolder + "','" + inputNewFolderDescription.Text + "'");
        while (dbReader.Read())
        {
            responseID = int.Parse(dbReader[0].ToString());
        }
        dbConn.Close();
        //string nodePath = TreeViewProject.SelectedNode.ValuePath;
        //Response.Redirect(Request.Path + "?Path=" + nodePath);
        Response.Redirect(Request.Path);
        //TreeViewProject.

    }
    protected void btnEditFolder_Click(object sender, EventArgs e)
    {
        string folderName = inputEditFolderName.Text;
        string systemFolder = null;

        if (TreeViewProject.SelectedNode.Depth == 1)
        {
            folderName = inputEditFolderPath.SelectedItem.Text;
            systemFolder = inputEditFolderPath.SelectedItem.Value;
        }
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "UPDATE UpFolder SET FolderName='" + folderName + "', SystemPath='" + systemFolder + "', Description='" + inputEditFolderDescription.Text + "' WHERE id =" + folder(TreeViewProject.SelectedNode.Value));
        dbConn.Close();
        Response.Redirect(Request.Path);

    }
    protected void btnRemoveFolder_Click(object sender, EventArgs e)
    {
        DB DB = new DB();
        SqlConnection dbConn = DB.DbOpen();
        SqlDataReader dbReader = DB.QueryReader(dbConn, "DELETE FROM UpFolderInFolder WHERE CurrentFolderId =" + link(TreeViewProject.SelectedNode.Value));
        dbConn.Close();
        //string nodePath = TreeViewProject.SelectedNode.Parent.ValuePath;
        //Response.Redirect(Request.Path + "?Path=" + nodePath);
        Response.Redirect(Request.Path);
    }
    #endregion

    #region Master File Event



    protected void DeleteMasterFile(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        int id = Int32.Parse(arg[0]);
        if (id > 0)
        {
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlDataReader dbReader = DB.QueryReader(dbConn, "EXEC DeleteFile " + id);
            while (dbReader.Read())
            {
                string uploadedName = ConfigurationManager.AppSettings["uploadLocation"].ToString() + @"\" + dbReader["SaveName"].ToString();
                FileInfo fileDelete = new FileInfo(uploadedName);

                if (fileDelete.Exists)
                {
                    fileDelete.Delete();
                }
            }
            dbReader.Close();
            dbConn.Close();
            TreeChange();
        }
    }

    protected void SelectMasterFile(object sender, EventArgs e)
    {
        string[] arg = (sender as LinkButton).CommandArgument.ToString().Split(new char[] { ',' });
        int id = Int32.Parse(arg[0]);
        FileName.InnerHtml = "<li>" + arg[1] + "</li>";
        if (id > 0)
        {
            DB DB = new DB();
            SqlConnection dbConn = DB.DbOpen();
            SqlDataReader dbReader = DB.QueryReader(dbConn, "SELECT * FROM View_StoredFile WHERE MasterId = " + id);
            GridView2.DataSource = dbReader;
            GridView2.DataBind();
            AddStoredFile.Enabled = true;
        }
    }
    #endregion   

    #region Stored File Event
    protected void DownloadFile(object sender, EventArgs e)
    {

    }
    protected void ActiveFile(object sender, EventArgs e)
    {
    }
    protected void DeleteFile(object sender, EventArgs e)
    {

    }

    #endregion

    #region Etc Functions
    public static int folder(string args)
    {
        string[] arg = args.Split(new char[] { ',' });
        return int.Parse(arg[0]);
    }
    public static int link(string args)
    {
        string[] arg = args.Split(new char[] { ',' });
        if (arg.Length > 1)
        {
            return int.Parse(arg[1]);
        }
        else
        {
            return int.Parse(arg[0]);

        }
    }

    #endregion




}