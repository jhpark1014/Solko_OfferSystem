<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OS_CampaignMailList.aspx.cs" Inherits="OS_CampaignMailList" %>

<%--<%@ Register Assembly="FarPoint.Web.Spread, Version=9.40.20161.0, Culture=neutral, PublicKeyToken=327c3516b1b18457" Namespace="FarPoint.Web.Spread" TagPrefix="FarPoint" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">

    <div class="page-header">
        <h2><b>라이센스 재계약 요청메일 발송관리</b></h2>
    </div>

    <div class="row">
        <div>
            <asp:TextBox ID="textCAID" runat="server" Visible="false">none</asp:TextBox>
            <asp:TextBox ID="textSUBJ" runat="server" Visible="false">none</asp:TextBox>
            <asp:Button ID="btnSendMailNow" runat="server" Style="text-align:center; float:right; margin-left:10px;" class="w-50 btn btn-primary btn-small" onClick="btnSendMailNow2_Click" OnClientClick="return CheckConfirm();" text="발송예정건 발송하기(고객에게 발송)" />
            <asp:Button ID="btnSendMailInternal" runat="server" Style="text-align:center; float:right; margin-left:10px;" class="w-50 btn btn-primary btn-small" onClick="btnSendMailInternal_Click" OnClientClick="return CheckConfirm();" text="발송예정건 내부발송(SOLKO)" />
            <asp:Button ID="btnSendMailInternalAll" runat="server" Style="text-align:center; float:right; margin-left:40px;" class="w-50 btn btn-primary btn-small" onClick="btnSendMailInternalAll_Click" OnClientClick="return CheckConfirm();" text="전체 내부발송(SOLKO)" />

            <asp:Button ID="btnExcelExport" runat="server" Style="text-align:center; float:left; margin-left:10px;" class="w-50 btn btn-primary btn-small" onClick="btnExcelExport_Click" text="Excel 저장" />
            <asp:Button ID="btnExcelImport" runat="server" Style="text-align:center; float:left; margin-left:40px;" class="w-50 btn btn-primary btn-small" onClick="btnExcelImport_Click" OnClientClick="return CheckFile();" text="Excel 읽기" />
            <asp:FileUpload ID="fileUpload" runat="server" Style="text-align:center; float:left; margin-left:10px;" class="w-30 btn btn-primary btn-small" />

            <br />
            <br />
        </div>
    </div>
            <br />



    <div class="row">
        <div class="col-md-20">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:UpdatePanel ID="updateGridPanel" runat="server" >
                        <ContentTemplate>
                            <asp:GridView ID="GridView1" runat="server" class="table table-striped" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" OnPageIndexChanging="OnPaging" Width="100%" OnRowCommand="GridView1_OnRowCommand" OnRowDataBound="GridView1_RowDataBound" rowstyle-cssclass="DataRow" AlternatingRowStyle-CssClass="DataRow DataRowAlt">
                                <Columns>
                                    <asp:BoundField ItemStyle-Width="3%" DataField="no" HeaderText="번호" />
                                    <asp:BoundField ItemStyle-Width="0%" DataField="guid" HeaderText="관리번호" Visible="false" />
                                    <asp:BoundField ItemStyle-Width="10%" DataField="거래처" HeaderText="거래처" />
                                    <asp:BoundField ItemStyle-Width="10%" DataField="사이트" HeaderText="사이트" />
                                    <asp:HyperLinkField ItemStyle-Width="25%" DataTextField="제목" HeaderText="제목" DataNavigateUrlFields="CRM보기" DataNavigateUrlFormatString="{0}" Target="_blank" />
                                    <asp:BoundField ItemStyle-Width="5%" DataField="기술지원갯수" HeaderText="기술지원갯수" />
                                    <asp:BoundField ItemStyle-Width="5%" DataField="RN담당" HeaderText="RN담당" />
                                    <asp:BoundField ItemStyle-Width="8%" DataField="예약발송일" HeaderText="예약발송일" />
                                    <asp:BoundField ItemStyle-Width="0%" DataField="진행상태" HeaderText="진행상태" Visible="false" />

                                    <asp:TemplateField ItemStyle-Width="15%" ShowHeader="true">
                                        <HeaderTemplate>
                                            검토결과
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:Button ID="buttonChangeWill" runat="server" CausesValidation="false" commandName="ChangeWill" Text='<%# Eval("진행상태") %>' CommandArgument='<%# Eval("guid") %>' OnClientClick="LoadingWithMask();" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField ItemStyle-Width="10%" ShowHeader="true">
                                        <HeaderTemplate>
                                            내부 발송하기
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:Button ID="buttonSendNowOneInternal" runat="server" CausesValidation="false" commandName="SendNowOneInternal" Text='내부 발송하기' CommandArgument='<%# Eval("guid") %>' OnClientClick="return CheckConfirm();" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField ItemStyle-Width="10%" ShowHeader="true">
                                        <HeaderTemplate>
                                            고객에게 발송하기
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:Button ID="buttonSendNowOne" runat="server" CausesValidation="false" commandName="SendNowOne" Text='고객에 지금 발송하기' CommandArgument='<%# Eval("guid") %>' OnClientClick="return CheckConfirm();" />
                                        </ItemTemplate>
                                    </asp:TemplateField>


                                    <asp:BoundField ItemStyle-Width="350px" DataField="간략보기" HeaderText="간략보기" HtmlEncode="False" ItemStyle-CssClass="minWidth300"  />
                                </Columns>

                                <PagerSettings NextPageText="&gt;" PreviousPageText="&lt;" />
                                <PagerStyle CssClass="none" HorizontalAlign="Center" VerticalAlign="Middle" BorderStyle="None" />
                            </asp:GridView>
                        </ContentTemplate>
                    </asp:UpdatePanel>


                </div>
            </div>

        </div>
    </div>


    <!-- Modal -->
    <div class="modal fade" id="modalAdd" tabindex="-1" role="dialog" aria-labelledby="modalAddLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
              
                <div class="modal-body">
                    <div class=".form-horizontal container">
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        function CheckConfirm() {
            let result = confirm('지금 메일을 발송하시겠습니까?');
            if (result) {
                LoadingWithMask();
            }
            return result;
        }
    </script>


    <script>
        $('#modalAdd').on('hidden.bs.modal', function () { $("#MainContent_hidemodalAdd").prop("checked", false); });
        $('#modalDel').on('hidden.bs.modal', function () { $("#MainContent_hidemodalDel").prop("checked", false); });
        $('#modalRej').on('hidden.bs.modal', function () { $("#MainContent_hidemodalRej").prop("checked", false); });
        if ($('#MainContent_hidemodalAdd').prop('checked')) { $('#modalAdd').modal('show'); };
        if ($('#MainContent_hidemodalDel').prop('checked')) { $('#modalDel').modal('show'); };
        if ($('#MainContent_hidemodalRej').prop('checked')) { $('#modalRej').modal('show'); };
    </script>

    <script type="text/javascript">  
        // for check all checkbox  
        function CheckAll(Checkbox) {
            var gridview1 = GridView1
            var checkboxes = GridView1.getElementsByID("CheckBox1")

            if (Checkbox.checked) {
                for (i = 0; i < checkboxes.length; i++) {

                    checkboxes[i].checked = true;
                }
            }
            else {
                checkboxes[i].checked = false;
            }

        }
    </script>  

    
    <script type="text/javascript">  
        function CheckFile() {
            if ($('#fileUpload').Files.length > 0) {
                return true;
            }
            else {
                alert('파일을 선택 후 실행해 주세요');
                return false;
            }
        }
    </script>  

</asp:Content>
