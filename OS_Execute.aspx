<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OS_Execute.aspx.cs" Inherits="RequestList" %>

<%--<%@ Register Assembly="FarPoint.Web.Spread, Version=9.40.20161.0, Culture=neutral, PublicKeyToken=327c3516b1b18457" Namespace="FarPoint.Web.Spread" TagPrefix="FarPoint" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="page-header">
        <h1>견적서 발행<small></small></h1>
    </div>

<%-- UpdatePanel 시작 --%>
    <asp:UpdatePanel ID="GridPanel" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <div>
                Forward 할인율 (%) : 
                <asp:TextBox ID="forwardDiscount" runat="server"></asp:TextBox>
                &nbsp
                Back 할인율 (%) : 
                <asp:TextBox ID="backDiscount" runat="server"></asp:TextBox>
                <asp:Button ID="applybtn" runat="server" Style="text-align:center;" class="w-50 btn btn-primary btn-small" onClick="applybtn_Click" text="일괄적용" />
            </div>

            <asp:Label ID="companyName" runat="server" Font-Size="25px" Font-Bold="true" ></asp:Label>
            <div class="row">
                <div class="col-md-12">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <asp:GridView ID="GridView3" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" OnPageIndexChanging="OnPaging" Width="100%">
                                <Columns>
                                <%--<asp:BoundField DataField="고객명" HeaderText="고객명" ItemStyle-Width="10%"/>--%>
                                <asp:BoundField DataField="number" HeaderText="NO." HtmlEncode="false" />
                                <asp:BoundField DataField="사업장 위치" HeaderText="사업장 위치" HtmlEncode="false" Visible="false" />
                                <asp:BoundField DataField="제품명" HeaderText="제품명" />
                                <%--<asp:TemplateField HeaderText="제품명">
                                    <ItemTemplate>
                                        <asp:Label id="ProductType" runat="server" Text='<%# Eval("new_l_product_category") + " " + Eval("new_l_products")%>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>--%>
                                <asp:BoundField DataField="Expired Date" HeaderText="Expired Date" DataFormatString="{0:yyyy/MM}" Visible="false" />


    <%--                        mssql setting 
                                <asp:BoundField DataField="CustomerName" HeaderText="고객명" />
                                <asp:BoundField DataField="Location" HeaderText="사업장 위치" />
                                <asp:TemplateField HeaderText="제품명">
                                    <ItemTemplate>
                                        <asp:Label id="ProductType" runat="server" Text='<%# Eval("ProductType") + " " + Eval("Product")%>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Qty" HeaderText="수량" />
                                <asp:BoundField DataField="ExpiredDate" HeaderText="Expired Date" DataFormatString="{0:yyyy/MM}" />
    --%>
                                <asp:BoundField DataField="BackDate" HeaderText="BackDate" Visible="false"/>
                                <asp:BoundField DataField="Forward" HeaderText="Forward" Visible="false"/>
                                <asp:BoundField DataField="수량" HeaderText="수량" />
                                <asp:BoundField DataField="기간요약" HeaderText="기간요약" HtmlEncode="false"/>
                                <asp:BoundField DataField="Customer RRP (소비자 단가)" HeaderText="Customer RRP (소비자 단가)" />
                                <asp:BoundField DataField="Proposal RRP (제안 단가)" HeaderText="Proposal RRP (제안 단가)" />
                                <asp:BoundField DataField="견적가격" HeaderText="최종 금액" Visible="false"/>
                                <%--<asp:TemplateField>
                                    <HeaderTemplate>
                                        할인율
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="changeForwardDiscount" runat="server" OnTextChanged="discountRate_TextChanged" AutoPostBack="true"></asp:TextBox>
                                        <br />
                                        <asp:TextBox ID="changeBackDiscount" runat="server" OnTextChanged="discountRate_TextChanged" AutoPostBack="true"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>--%>
                                <asp:BoundField DataField="견적가격" HeaderText="견적가격" HtmlEncode="false" />

                            </Columns>
                            <PagerSettings NextPageText="&gt;" PreviousPageText="&lt;" />
                             <PagerStyle CssClass="none" HorizontalAlign="Center" VerticalAlign="Middle" BorderStyle="None" />

                            </asp:GridView>
                        
                </div>
            </div>
        </div>
    </div>
    </ContentTemplate>
</asp:UpdatePanel>

    <div>
        <asp:Button ID="estimatebtn" runat="server" Style="text-align:center;" class="w-50 btn btn-primary btn-small" onClick="estimatebtn_Click" text="견적하기" />
    </div>

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
    </div>

    <script>
        $('#modalAdd').on('hidden.bs.modal', function () { $("#MainContent_hidemodalAdd").prop("checked", false); });
        $('#modalDel').on('hidden.bs.modal', function () { $("#MainContent_hidemodalDel").prop("checked", false); });
        $('#modalRej').on('hidden.bs.modal', function () { $("#MainContent_hidemodalRej").prop("checked", false); });
        if ($('#MainContent_hidemodalAdd').prop('checked')) { $('#modalAdd').modal('show'); };
        if ($('#MainContent_hidemodalDel').prop('checked')) { $('#modalDel').modal('show'); };
        if ($('#MainContent_hidemodalRej').prop('checked')) { $('#modalRej').modal('show'); };
    </script>

</asp:Content>
