<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Update.aspx.cs" Inherits="Update" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="page-header">
        <h1>Update Management <small>업데이트 관리</small></h1>
    </div>
    <div class="row">
        <div class="col-md-3">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:TreeView ID="TreeViewProject" runat="server" CssClass="treestyle" ShowLines="True" LineImagesFolder="~/TreeLineImages" OnSelectedNodeChanged="TreeViewProject_SelectedNodeChanged" ImageSet="XPFileExplorer" NodeIndent="15">
                        <HoverNodeStyle Font-Underline="True" ForeColor="#6666AA" />
                        <NodeStyle Font-Names="Tahoma" Font-Size="8pt" ForeColor="Black" HorizontalPadding="2px" NodeSpacing="0px" VerticalPadding="2px" />
                        <ParentNodeStyle Font-Bold="False" />
                        <SelectedNodeStyle BackColor="#B5B5B5" Font-Underline="False" HorizontalPadding="0px" VerticalPadding="0px" Font-Bold="True" />
                    </asp:TreeView>
                </div>
            </div>

        </div>
        <div class="col-md-9">
            <div class="row">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <ol class="breadcrumb" id="FolderPath" runat="server">
                            <li class="active">No Selected</li>
                        </ol>
                        <asp:GridView ID="GridView1" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" HorizontalAlign ="Center" >

                            <Columns>
                                <asp:TemplateField HeaderText="File Name">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkMasterSelect" ToolTip="Select File" Text='<%# Eval("FileFullName") %>' CommandArgument='<%# Eval("MasterId") + "," + Eval("FileFullName")  %>' runat="server" OnClick="SelectMasterFile"></asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-HorizontalAlign="Right" />
                                <asp:BoundField DataField="Version" HeaderText="Version" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" />
                                <asp:TemplateField HeaderText="Delete" ItemStyle-HorizontalAlign ="Center" >
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkMasterDelete" Text='<span class="glyphicon glyphicon-remove" aria-hidden="true"></span>' CommandArgument='<%# Eval("MasterId") %>' runat="server" OnClick="DeleteMasterFile" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        <div class="btn-group pull-right" role="group" aria-label="...">
                            <asp:Button ID="InfoFolder" runat="server" Text="폴더정보" CssClass="btn btn-info btn-xs" Enabled="False" />
                            <asp:Button ID="AddFolder" runat="server" Text="폴더추가" CssClass="btn btn-primary btn-xs" Enabled="False" />
                            <asp:Button ID="EditFolder" runat="server" Text="폴더수정" CssClass="btn btn-warning btn-xs" Enabled="False" />
                            <asp:Button ID="RemoveFolder" runat="server" Text="폴더삭제" CssClass="btn btn-danger btn-xs" Enabled="False" />
                            <asp:Button ID="AddMasterFile" runat="server" Text="파일추가" CssClass="btn btn-success btn-xs" Enabled="False" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="row" hidden="hidden">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <ol class="breadcrumb" id="FileName" runat="server">
                            <li class="active">No Selected</li>
                        </ol>
                        <div id="exTab2" class="container">
                            <ul class="nav nav-tabs">
                                <li class="active">
                                    <a href="#1" data-toggle="tab">Revision</a>
                                </li>
                                <li><a href="#2" data-toggle="tab">Information</a>
                                </li>
                            </ul>

                            <div class="tab-content ">
                                <div class="tab-pane active" id="1">
                                    <asp:GridView ID="GridView2" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False">

                                        <Columns>
                                            <asp:TemplateField HeaderText="File Name">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkDownload" ToolTip="Download File" Text='<%# Eval("FileFullName") %>' CommandArgument='<%# Eval("Id") %>' runat="server" OnClick="DownloadFile"></asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="Size" HeaderText="Size" />
                                            <asp:BoundField DataField="Version" HeaderText="Version" />
                                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" />
                                            <asp:TemplateField HeaderText="Active">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkActive" Text='<span class="glyphicon glyphicon-ok" aria-hidden="true"></span>' CommandArgument='<%# Eval("Id") %>' runat="server" OnClick="ActiveFile" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Delete">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkDelete" Text='<span class="glyphicon glyphicon-remove" aria-hidden="true"></span>' CommandArgument='<%# Eval("Id") %>' runat="server" OnClick="DeleteFile" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                    <div class="btn-group pull-right" role="group" aria-label="...">
                                        <asp:Button ID="AddStoredFile" runat="server" Text="파일추가" CssClass="btn btn-success btn-xs" Enabled="False" />
                                    </div>
                                </div>
                                <div class="tab-pane" id="2">
                                    <dl class="dl-horizontal">
                                        <dt>File Name</dt>
                                        <dd>
                                            <asp:Label ID="lblFileName" runat="server" Text="Label"></asp:Label></dd>
                                        <dt>File Name</dt>
                                        <dd>
                                            <asp:Label ID="lblFullPath" runat="server" Text="Label"></asp:Label></dd>
                                        <dt>File Name</dt>
                                        <dd>
                                            <asp:Label ID="lblDescription" runat="server" Text="Label"></asp:Label></dd>
                                    </dl>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
    <!-- Modal 파일 업로드 ------------------------------------------>
    <asp:Panel ID="modalAddMasterFile" runat="server" CssClass="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">파일 업로드</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label for="exampleInputFile">파일 업로드</label>
                    <asp:FileUpload ID="fup" runat="server" />
                    <p class="help-block">
                        파일추가 부분입니다.
                        <br />
                        RevisionUp은 아레 Revision창에서 수행하시기 바랍니다.(업데이트 버전관리 : 추가개발)
                    </p>
                </div>
            </div>
            <div class="modal-footer">

                <button type="button" class="btn btn-default" data-dismiss="modal" id="CancelAddMasterFile">Cancel</button>
                <asp:Button ID="btnAddMaster" runat="server" CssClass="btn btn-primary" Text="파일업로드하기" OnClick="btnAddMasterFile_Click" />
            </div>
        </div>
    </asp:Panel>
    <!-- Modal 폴더생성 ------------------------------------------>
    <asp:Panel ID="modalNewFolder" runat="server" CssClass="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">폴더생성</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label for="exampleInputFile">폴더이름</label>
                    <asp:TextBox CssClass="form-control" ID="inputNewFolderName" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="exampleInputFile">Root 경로</label><asp:DropDownList data-toggle="dropdown" aria-expanded="false" ID="inputNewFolderPath" runat="server" Enabled="False">
                    </asp:DropDownList>
                    &nbsp;<p class="help-block">
                        Root인 경우만 설정합니다.<br />
                    </p>

                </div>
                <div class="form-group">
                    <label for="exampleInputFile">폴더설명</label>
                    <asp:TextBox CssClass="form-control" ID="inputNewFolderDescription" runat="server" TextMode="MultiLine" Columns="20" Rows="4"></asp:TextBox>
                </div>
            </div>
            <div class="modal-footer">

                <button type="button" class="btn btn-default" data-dismiss="modal" id="CancelAddFolder">Cancel</button>
                <asp:Button ID="btnAddFolder" runat="server" CssClass="btn btn-primary" Text="새폴더" OnClick="btnAddFolder_Click" />
            </div>
        </div>
    </asp:Panel>
    <!-- Modal 폴더수정 ------------------------------------------>
    <asp:Panel ID="modalEditFolder" runat="server" CssClass="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">폴더 수정</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label for="exampleInputFile">폴더이름</label>
                    <asp:TextBox CssClass="form-control" ID="inputEditFolderName" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="exampleInputFile">Root 경로</label><asp:DropDownList class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-expanded="false" ID="inputEditFolderPath" runat="server" Enabled="False">
                    </asp:DropDownList>
                    <p class="help-block">
                        Root인 경우만 설정합니다.
                    </p>
                </div>
                <div class="form-group">
                    <label for="exampleInputFile">폴더설명</label>
                    <asp:TextBox CssClass="form-control" ID="inputEditFolderDescription" runat="server" TextMode="MultiLine" Columns="20" Rows="4"></asp:TextBox>
                </div>
            </div>
            <div class="modal-footer">

                <button type="button" class="btn btn-default" data-dismiss="modal" id="CancelEditFolder">Cancel</button>
                <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary" Text="폴더수정" OnClick="btnEditFolder_Click" />
            </div>
        </div>
    </asp:Panel>
    <!-- Modal 폴더정보 ------------------------------------------>
    <asp:Panel ID="modalInfoFolder" runat="server" CssClass="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">폴더 정보</h4>
            </div>
            <div class="modal-body">
                <dl class="dl-horizontal">
                    <dt>Folder Name</dt>
                    <dd>
                        <asp:Label ID="lblFolderName" runat="server" Text="Label"></asp:Label></dd>
                    <dt>Path</dt>
                    <dd>
                        <asp:Label ID="lblFolderPath" runat="server" Text="Label"></asp:Label></dd>
                    <dt>Description</dt>
                    <dd>
                        <asp:Label ID="lblFolderDescription" runat="server" Text="Label"></asp:Label></dd>
                </dl>
            </div>
            <div class="modal-footer">

                <button type="button" class="btn btn-default" data-dismiss="modal" id="CancelInfoFolder">Close</button>
            </div>
        </div>
    </asp:Panel>
    <!-- Modal 폴더 삭제 ------------------------------------------>
    <asp:Panel ID="modalRemoveFolder" runat="server" CssClass="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">폴더 삭제</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label for="exampleInputFile">폴더가 삭제됩니다.</label>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal" id="CancelRemoveFolder">Cancel</button>
                <asp:Button ID="btnRemoveFolder" runat="server" CssClass="btn btn-primary" Text="폴더삭제" OnClick="btnRemoveFolder_Click" />
            </div>
        </div>
    </asp:Panel>

    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender1" runat="server" TargetControlID="AddMasterFile" PopupControlID="modalAddMasterFile" CancelControlID="CancelAddMasterFile" Drag="False"></ajaxToolkit:ModalPopupExtender>
    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender2" runat="server" TargetControlID="InfoFolder" PopupControlID="modalInfoFolder" CancelControlID="CancelInfoFolder" Drag="False"></ajaxToolkit:ModalPopupExtender>
    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender3" runat="server" TargetControlID="AddFolder" PopupControlID="modalNewFolder" CancelControlID="CancelAddFolder" Drag="False"></ajaxToolkit:ModalPopupExtender>
    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender4" runat="server" TargetControlID="EditFolder" PopupControlID="modalEditFolder" CancelControlID="CancelEditFolder" Drag="False"></ajaxToolkit:ModalPopupExtender>
    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender5" runat="server" TargetControlID="RemoveFolder" PopupControlID="modalRemoveFolder" CancelControlID="CancelRemoveFolder" Drag="False"></ajaxToolkit:ModalPopupExtender>
</asp:Content>

