<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="FileList.aspx.cs" Inherits="FileList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>Update Management <small>업데이트 파일목록</small></h1>
    </div>
    <div class="row">
        <div class="col-md-12">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False">

                        <Columns>
                            <asp:BoundField DataField="ProjectName" HeaderText="Project" />
                            <asp:BoundField DataField="depth_fullname" HeaderText="Path" />
                            <asp:TemplateField HeaderText="File">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkFileDownload" Text='<%# Eval("FileFullName") %>' CommandArgument='<%# Eval("MasterId") %>' runat="server" OnClick="DownloadFile" ToolTip='<%# Eval("SaveName") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-HorizontalAlign="Right" />
                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" />
                            <asp:TemplateField HeaderText="Delete">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkMasterDelete" Text='<span class="glyphicon glyphicon-remove" aria-hidden="true"></span>' CommandArgument='<%# Eval("MasterId") %>' runat="server" OnClick="DeleteMasterFile" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
