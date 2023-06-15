<%@ Page Title="등록" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Account_Register" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>
    <p class="text-danger">
        <asp:Literal runat="server" ID="ErrorMessage" />
    </p>

    <div class="form-horizontal">
        <h4>새 계정을 만듭니다.</h4>
        <hr />
        <asp:ValidationSummary runat="server" CssClass="text-danger" />
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserId" CssClass="col-md-2 control-label">아이디</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserId" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserId"
                    CssClass="text-danger" ErrorMessage="아이디 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserName" CssClass="col-md-2 control-label">이름 + 직함</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserName" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName"
                    CssClass="text-danger" ErrorMessage="이름 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserEmail" CssClass="col-md-2 control-label">이메일</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserEmail" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserEmail"
                    CssClass="text-danger" ErrorMessage="이메일 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserPhone" CssClass="col-md-2 control-label">유선 번호 <br /> (000-0000-0000 형식으로 작성해주세요.) </asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserPhone" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserPhone"
                    CssClass="text-danger" ErrorMessage="유선 번호 필드는 필수입니다." />
            </div>
        </div>
                <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserMobilePhone" CssClass="col-md-2 control-label">핸드폰 번호 <br /> (000-0000-0000 형식으로 작성해주세요.) </asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserMobilePhone" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserMobilePhone"
                    CssClass="text-danger" ErrorMessage="핸드폰 번호 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserCRMID" CssClass="col-md-2 control-label">CRM 계정 아이디 </asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserCRMID" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserCRMID"
                    CssClass="text-danger" ErrorMessage="CRM 계정 아이디 필드는 필수입니다." />
            </div>
        </div>
                <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="UserCRMPass" CssClass="col-md-2 control-label">CRM 계정 비밀번호</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="UserCRMPass" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserCRMPass"
                    CssClass="text-danger" ErrorMessage="CRM 계정 비밀번호 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="Password" CssClass="col-md-2 control-label">비밀번호</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Password"
                    CssClass="text-danger" ErrorMessage="비밀번호 필드는 필수입니다." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="ConfirmPassword" CssClass="col-md-2 control-label">비밀번호 확인</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="ConfirmPassword" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ConfirmPassword"
                    CssClass="text-danger" Display="Dynamic" ErrorMessage="비밀번호 확인 필드는 필수입니다." />
                <asp:CompareValidator runat="server" ControlToCompare="Password" ControlToValidate="ConfirmPassword"
                    CssClass="text-danger" Display="Dynamic" ErrorMessage="비밀번호와 일치하지 않습니다." />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" OnClick="CreateUser_Click" Text="등록" CssClass="btn btn-default" />
            </div>
        </div>
    </div>
</asp:Content>

