<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OS_FileContent.aspx.cs" Inherits="OS_FileContent" %>

<%--<%@ Register Assembly="FarPoint.Web.Spread, Version=9.40.20161.0, Culture=neutral, PublicKeyToken=327c3516b1b18457" Namespace="FarPoint.Web.Spread" TagPrefix="FarPoint" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="page-header">
        <h1>View license status <small>견적준비</small></h1>
    </div>

    <div class="row">

        <div class="col-md-9">
            <div class="row">

                <div class="panel panel-default">
                    <div class="panel-body">
                        <asp:GridView ID="PriceGridView" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" AllowPaging="True" OnPageIndexChanging="OnPaging" PageSize="30"> 

                            <Columns>

                                <asp:BoundField DataField="Category" HeaderText="분류" />
                                <asp:BoundField DataField="Product name" HeaderText="제품명" />
                                <asp:BoundField DataField="Price" HeaderText="가격" />
                                <asp:BoundField DataField="PLC" HeaderText="PLC" />
                                <asp:BoundField DataField="RLC" HeaderText="RLC" />

                            </Columns>
                            <PagerSettings NextPageText="&gt;" PreviousPageText="&lt;" />
                            <PagerStyle CssClass="none" HorizontalAlign="Center" VerticalAlign="Middle" BorderStyle="None" />

                        </asp:GridView>
                    </div>
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
