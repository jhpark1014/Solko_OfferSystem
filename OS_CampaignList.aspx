<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OS_CampaignList.aspx.cs" Inherits="OS_CampaignList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">

    <div class="page-header">
        <h2><b>라이센스 재계약요청 캠페인</b></h2>
    </div>

    <div class="row">
        <div>
            검색조건 : 
            <asp:TextBox ID="textFindyyyymm" runat="server"></asp:TextBox>
            <asp:Button ID="btnStep1GetLicense" runat="server" Style="text-align:center;" class="w-50 btn btn-primary btn-small" onClick="btnStep1GetLicense_Click" OnClientClick="LoadingWithMask();" text="CRM 캠페인 조회" />
            <asp:Button ID="btnStep2CreateCampaign" runat="server" Style="text-align:center; float:right;" class="w-50 btn btn-primary btn-small" onClick="btnCreate_Click" OnClientClick="LoadingWithMask();"  text="캠페인 생성" />
        </div>
        <div><br /></div>
    </div>


    <div class="row">
        <div class="col-md-20">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:UpdatePanel ID="updateGridPanel" runat="server" >
                        <ContentTemplate>
                            <asp:GridView ID="GridView1" runat="server" class="table table-striped" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" OnPageIndexChanging="OnPaging" Width="100%" rowstyle-cssclass="DataRow" AlternatingRowStyle-CssClass="DataRow DataRowAlt">
                                <Columns>
                                    <asp:BoundField ItemStyle-Width="50px" DataField="no" HeaderText="번호" />
                                    <asp:BoundField ItemStyle-Width="0%" DataField="guid" HeaderText="관리번호" Visible="false" />
                                    <asp:HyperLinkField ItemStyle-Width="50%" DataTextField="subject" HeaderText="캠페인 활동 명칭" DataNavigateUrlFields="CRM보기" DataNavigateUrlFormatString="{0}" Target="_blank" />
                                    <asp:BoundField ItemStyle-Width="10%" DataField="statusCode" HeaderText="상태설명" />
                                    <asp:HyperLinkField ItemStyle-Width="10%" DataTextField="mailCount" HeaderText="관련 전자메일" DataNavigateUrlFields="MAIL보기" DataNavigateUrlFormatString="{0}" Target="_blank" />
                                    <asp:TemplateField ItemStyle-Width="80px" HeaderText="메일목록 보기">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkLiewDetail" Text='상세보기' CommandArgument='<%# Eval("guid") +","+ Eval("subject") %>' runat="server" OnClick="OnClick_DetailView" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
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

    <div>
    </div>

    
    <!-- Modal -->
    <div class="modal fade" id="modalAdd" tabindex="-1" role="dialog" aria-labelledby="modalAddLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="modalAddLabel">캠페인 활동 생성</h4>
                </div>
              
                <div class="modal-body">
                    <div class=".form-horizontal container">

                        <label for="textBox1" class="col-md-4 control-label">만료 년/월</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="textBox1" runat="server" class="form-control"></asp:TextBox>
                            <br />
                            <label ID="Label1" >(예, 2023/06 형태)</label>
                            <br /><br />
                            <%--<asp:Label ID="lblInfo" runat="server" class="form-control" BorderStyle="None">(예, 2023/06 형태)</asp:Label>--%>
                        </div>

                        <div hidden>
                            <asp:TextBox ID="textLicenseSearchStart" runat="server" class="form-control" ReadOnly="true"></asp:TextBox>
                            <asp:TextBox ID="textLicenseSearchEnd" runat="server" class="form-control" ReadOnly="true"></asp:TextBox>
                            <asp:TextBox ID="textMailWillDate" runat="server" class="form-control" ReadOnly="true"></asp:TextBox>
                            <asp:TextBox ID="textCRMKey" runat="server" class="form-control" ReadOnly="true"></asp:TextBox>
                        </div>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        <asp:Button ID="btnAdd" runat="server" Text="캠페인 활동 생성" class="btn btn-primary" OnClick="btnAdd_Click" OnClientClick="LoadingWithMask();" />
                    </div>
                </div>
            </div>
        </div>
    </div>


    
    <div class="hidden">
        <asp:CheckBox ID="hidemodalAdd" runat="server" Checked="False" />
        <asp:CheckBox ID="hidemodalDel" runat="server" Checked="False" />
        <asp:CheckBox ID="hidemodalRej" runat="server" Checked="False" />
        <asp:TextBox ID="selIDList" runat="server" />
    </div>

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

</asp:Content>
