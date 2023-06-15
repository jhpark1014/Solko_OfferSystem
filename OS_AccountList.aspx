<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OS_AccountList.aspx.cs" Inherits="RequestList" %>

<%--<%@ Register Assembly="FarPoint.Web.Spread, Version=9.40.20161.0, Culture=neutral, PublicKeyToken=327c3516b1b18457" Namespace="FarPoint.Web.Spread" TagPrefix="FarPoint" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">

    <style type="text/css">
        .CenteredGrid {
            width: 100%;
            margin-left: auto;
            margin-right: auto;
        }
    </style>

    <style type="text/css">
        .hideGridColumn
        {
            display: none;
        }
    </style>

    <script language="javascript" type="text/javascript">
        function goEdit() {
            document.edit.submit();
        }
    </script>


    <div class="page-header">
        <h1>업체 조회<small></small></h1>

    </div>

    <div class="row">
        <div>
        업체명: 
        <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
        &nbsp
        Serial Number:  
        <asp:TextBox ID="serialSearch" runat="server"></asp:TextBox>
        &nbsp
        견적기준 년월 (Y/M) :
        <asp:TextBox ID="startDateSearch" runat="server" Width="8%" ></asp:TextBox>
        ~
        <asp:TextBox ID="finDateSearch" runat="server" Width="8%" ></asp:TextBox>

        &nbsp &nbsp &nbsp
        <asp:Button ID="searchbtn" runat="server" Style="text-align:center; background-color:deepskyblue; border-color:deepskyblue;" class="w-50 btn btn-primary btn-small" onClick="searchbtn_Click" text="검색" />
        &nbsp
        <asp:Button ID="resetbtn" runat="server" Style="text-align:center;  background-color:deepskyblue; border-color:deepskyblue;" class="w-50 btn btn-primary btn-small" onClick="resetbtn_Click" text="RESET" />

        <asp:Button ID="calculatebtn" runat="server" Style="text-align:center; float:right; font-weight:bold;" class="w-50 btn btn-primary btn-small" onClick="Calculatebtn_Click" text="견적 계산하기" />

        </div>
    </div>
    <br />

    <div class="row">
        <div class="col-md-20">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:UpdatePanel ID="updateGridPanel" runat="server" >
                        <ContentTemplate>
                            <asp:GridView ID="GridView1" runat="server" class="table table-striped" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" OnPageIndexChanging="OnPaging" Width="100%">
                                <Columns>
                                    <asp:BoundField DataField="new_l_account" HeaderText="고객명" />
                                    <%--<asp:BoundField DataField="Location" HeaderText="사업장 위치" />--%>
<%--                                    <asp:BoundField DataField="new_p_status" HeaderText="등급" />--%>
                                    <asp:BoundField DataField="new_txt_account_eng" HeaderText="고객명(영문)" />
                                    <asp:BoundField DataField="new_txt_site" HeaderText="Site" />

                                    <asp:BoundField DataField="new_name" HeaderText="Serial Number" />

                                    <%--<asp:TemplateField HeaderText="Serial Number">
                                        <ItemTemplate>
                                             <%# Eval("new_name") + " " + Eval("new_l_products")%>
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>

                                    <%-- mssql column set
                                        <asp:BoundField DataField="SerialNumber" HeaderText="Serial Number" />
                                    <asp:BoundField DataField="CustomerName" HeaderText="고객명" />
                                    <asp:BoundField DataField="Location" HeaderText="사업장 위치" />
                                    <asp:BoundField DataField="Grade" HeaderText="등급" />
                                    <asp:BoundField DataField="Site" HeaderText="Site" />--%>

                                    <%--<asp:BoundField DataField="ProductType" HeaderText="제품분류" />--%>
                                    <%--<asp:BoundField DataField="Product" HeaderText="제품" />--%>
                                    <asp:TemplateField HeaderText="제품명">
                                        <ItemTemplate>
                                            <%# Eval("new_l_product_category") + " " + Eval("new_l_products")%>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="new_p_status" HeaderText="상태" />
                                    <asp:BoundField DataField="new_p_type" HeaderText="타입" />


                                    <asp:BoundField DataField="new_i_qty" HeaderText="수량" />
                                    <%--<asp:BoundField DataField="new_dt_install" HeaderText="Install Date" DataFormatString="{0:yyyy/MM}" />--%>
                                    <asp:BoundField DataField="new_dt_end" HeaderText="End Date" DataFormatString="{0:yyyy/MM}" />
                                    <asp:BoundField DataField="new_dt_expired" HeaderText="Expired Date" DataFormatString="{0:yyyy/MM}" />
                                    <%--<asp:BoundField DataField="statuscode" HeaderText="Status" />--%>
                                    <%--<asp:TemplateField Visible="false">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="idid" runat="server" Value='<%# Eval("new_customer_productsid") %>'></asp:HiddenField>
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>
                                    <asp:BoundField DataField="new_customer_productsid" HeaderText="ID" HeaderStyle-CssClass="hideGridColumn" ItemStyle-CssClass="hideGridColumn" />
                            
        <%--   mssql column set
                                    <asp:BoundField DataField="Qty" HeaderText="수량" />
                                    <asp:BoundField DataField="InstallDate" HeaderText="Install Date" DataFormatString="{0:yyyy/MM}" />
                                    <asp:BoundField DataField="EndDate" HeaderText="End Date" DataFormatString="{0:yyyy/MM}" />
                                    <asp:BoundField DataField="ExpiredDate" HeaderText="Expired Date" DataFormatString="{0:yyyy/MM}" />
                                    <asp:BoundField DataField="Status" HeaderText="Status" />
                                    <asp:BoundField DataField="ID" HeaderText="ID" />
        --%>

                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            선택 <asp:CheckBox ID="CheckBoxAll" runat="server" OnCheckedChanged="CheckBoxAll_CheckedChanged" AutoPostBack="true" />
                                        </HeaderTemplate>
                                        <ItemTemplate>  
		                                    <asp:CheckBox ID="CheckBox1" runat="server" />  
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


    <asp:Button ID="reloadExcel" runat="server" Style="text-align:center; float:right" class="w-50 btn btn-primary btn-small" onClick="reloadExcel_Click" text="단가표 다시 읽기" Visible="false"/>


    <%--<form name="edit" action="OS_Calculate.aspx" method="POST" runat="server">--%>
        <div>
            
        </div>
        <%--<input type="text" name="test" />--%>
		<%--<input id="calculatebtn" type="button" value="견적 계산하기" onclick="goEdit()" />--%>
	<%--</form>--%>


    <!-- Modal -->
    <div class="modal fade" id="modalAdd" tabindex="-1" role="dialog" aria-labelledby="modalAddLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="modalAddLabel">라이선스 요청 승인</h4>
                </div>
              
                <div class="modal-body">
                    <div class=".form-horizontal container">
                        <asp:TextBox ID="txtRowID" runat="server" Visible="False"></asp:TextBox>
                        <label for="txtCompany" class="col-sm-4 control-label">회사명</label>
                        <div class="col-sm-8">
                            <asp:TextBox ID="txtCompany" runat="server" class="form-control"></asp:TextBox>
                        </div>
                        <label for="txtBuso" class="col-md-4 control-label">부서</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtBuso" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtName" class="col-md-4 control-label">이름</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtName" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtMail" class="col-md-4 control-label">메일주소</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtMail" runat="server" TextMode="Email" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtOffice" class="col-md-4 control-label">회사전화</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtOffice" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtPhone" class="col-md-4 control-label">휴대폰</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtPhone" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtComment" class="col-md-4 control-label">요청사항</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtComment" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtswKey" class="col-md-4 control-label">Key</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtswKey" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        
                        <label for="txtprgName" class="col-md-4 control-label">프로그램 명칭</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtprgName" runat="server" class="form-control"></asp:TextBox>
                        </div>

                        <label for="txtprgVer" class="col-md-4 control-label">프로그램 버전</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtprgVer" runat="server" class="form-control"></asp:TextBox>
                        </div>


                        <label for="txtswKey" class="col-md-4 control-label">라이선스 종류</label>
                        <div class="col-md-8">
                            <asp:RadioButton ID="RadioButton1" runat="server" Text="SOLKO 정식" Checked="True" GroupName="LicType" AutoPostBack="True" OnCheckedChanged="changRadio" ToolTip="기본 1년라이선스/수정수량 제한없음" />
                            <asp:RadioButton ID="RadioButton2" runat="server" Text="Evaluation" Checked="False" GroupName="LicType" AutoPostBack="True" OnCheckedChanged="changRadio" ToolTip="개본 1개월 라이선스/수정수량 적용" />
                            <asp:RadioButton ID="RadioButton3" runat="server" Text="Test" Checked="False" GroupName="LicType" AutoPostBack="True" OnCheckedChanged="changRadio" />
                        </div>

                        <label for="txtType" class="col-md-4 control-label">라이선스 시작일</label>
                        <div class="col-md-8">
                            <div class="input-group">
                                <asp:TextBox ID="txtLicStart" runat="server" class="form-control" OnTextChanged="changeStartDate"></asp:TextBox>
                                <%--<div class="input-group-addon">
                                    <a data-toggle="collapse" href="#collapseStart" aria-expanded="false" aria-controls="collapseStart">▽
                                    </a>
                                </div>--%>
                            </div>
                            <div class="collapse" id="collapseStart">
                                <asp:Calendar ID="dpStart" runat="server" OnSelectionChanged="dpStart_SelectionChanged"></asp:Calendar>
                            </div>
                        </div>

                        <label for="txtType" class="col-md-4 control-label">라이선스 종료일</label>
                        <div class="col-md-8">
                            <div class="input-group">
                                <asp:TextBox ID="txtLicEnd" runat="server" class="form-control"></asp:TextBox>
                                <%--<div class="input-group-addon">
                                    <a data-toggle="collapse" href="#collapseEnd" aria-expanded="false" aria-controls="collapseEnd">▽
                                    </a>
                                </div>--%>
                            </div>
                            <div class="collapse" id="collapseEnd">
                                <asp:Calendar ID="dpEnd" runat="server" OnSelectionChanged="dpEnd_SelectionChanged"></asp:Calendar>
                            </div>
                        </div>

<%--                        <label for="txtType" class="col-md-4 control-label">작업가능 모델수량</label>
                        <div class="col-md-8">
                            <asp:TextBox ID="txtLicMax" runat="server" class="form-control" Visible="False">0</asp:TextBox>
                        </div>--%>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        <asp:Button ID="btnAdd" runat="server" Text="라이선스 발급" class="btn btn-primary" OnClick="btnAdd_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>


    <!-- Modal -->
    <div class="modal fade" id="modalDel" tabindex="-1" role="dialog" aria-labelledby="modalDelLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">x</span></button>
                    <h4 class="modal-title" id="modalDelLabel">요청 삭제</h4>
                </div>
                <div class="modal-body">
                    라이선스 발급요청을 취소합니다.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    <asp:Button ID="btnDel" runat="server" Text="요청취소" class="btn btn-primary" OnClick="btnDel_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="modalRej" tabindex="-1" role="dialog" aria-labelledby="modalRejLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">x</span></button>
                    <h4 class="modal-title" id="modalRejLabel">요청 반려</h4>
                </div>
                <div class="modal-body">
                    <div class=".form-horizontal container">
                    라이선스 발급요청을 반려합니다(유지보수 계약상태 경우만 발급 가능함의 메일을 발송합니다.)
                    </div>
                    <label for="txtMail" class="col-md-4 control-label">메일주소</label>
                    <div class="col-md-8">
                        <asp:TextBox ID="txtMailReject" runat="server" TextMode="Email" class="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    <asp:Button ID="Button1" runat="server" Text="요청반려" class="btn btn-primary" OnClick="btnRej_Click" />
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
