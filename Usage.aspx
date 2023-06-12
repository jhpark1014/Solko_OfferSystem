<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Usage.aspx.cs" Inherits="Usage" %>

<%--<%@ Register Assembly="FarPoint.Web.Spread, Version=9.40.20161.0, Culture=neutral, PublicKeyToken=327c3516b1b18457" Namespace="FarPoint.Web.Spread" TagPrefix="FarPoint" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="page-header">
        <h1>License Management <small>사용율</small></h1>
        <div class="row" Style="text-align: right">
            <asp:Label ID="labelSearch" runat="server" Text="검색어 : " />
            <asp:TextBox ID="txtSearch" runat="server" Style="width:150px" />
            <asp:Button  ID="btnSearch" runat="server" Style="width:80px" OnClick="btnSearchClick" Text="검색" />
        </div>
    </div>
    <div class="row">
        <div class="col-md-12">
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-striped" GridLines="None" AutoGenerateColumns="False" AllowPaging="True" OnPageIndexChanging="OnPaging" AllowSorting="true" OnSorting="GridView1_Sorting" PageSize="30">

                        <Columns>
                            <asp:BoundField DataField="Company" HeaderText="회사" SortExpression="Company"/>
                            <asp:BoundField DataField="Name" HeaderText="이름" SortExpression="Name" />
                            <asp:BoundField DataField="SwKey" HeaderText="S/N" SortExpression="SwKey" />
                            <asp:BoundField DataField="SwVer" HeaderText="Version" SortExpression="SwVer" />
                            <asp:BoundField DataField="SwSP" HeaderText="Service Pack" SortExpression="SwSP" />
                            <asp:BoundField DataField="OsVer" HeaderText="OS" SortExpression="OsVer" />
                            <asp:BoundField DataField="Hostname" HeaderText="Host Name" SortExpression="Hostname" />
                            <asp:BoundField DataField="UserName" HeaderText="Login Name" SortExpression="UserName" />
                            <asp:BoundField DataField="RunDate" HeaderText="사용일" DataFormatString="{0:yyyy/MM/dd}" SortExpression="RunDate" />
                            <asp:BoundField DataField="ProgramVersion" HeaderText="xPMWorks Version" SortExpression="ProgramVersion" />
                            <asp:BoundField DataField="CountEditor" HeaderText="Editor" SortExpression="CountEditor" />
                            <asp:BoundField DataField="CountPrint" HeaderText="Print" SortExpression="CountPrint" />
                            <asp:BoundField DataField="CountRename" HeaderText="Rename" SortExpression="CountRename" />

                        </Columns>
                        <PagerSettings NextPageText="&gt;" PreviousPageText="&lt;" />
                        <PagerStyle CssClass="none" HorizontalAlign="Center" VerticalAlign="Middle" BorderStyle="None" />

                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
