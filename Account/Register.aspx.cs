using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.UI;
using xPMWorksWeb;

public partial class Account_Register : Page
{
    protected void CreateUser_Click(object sender, EventArgs e)
    {
        var manager = new UserManager();
        //var user = new ApplicationUser() { UserName = UserName.Text };
        var user = new ApplicationUser() { UserName = UserId.Text };
        IdentityResult result = manager.Create(user, Password.Text);
        manager.SetPhoneNumber(user.Id, UserName.Text + "*" + UserPhone.Text + "*" + UserMobilePhone.Text + "*" +  UserCRMID.Text + "*" + UserCRMPass.Text);
        manager.SetEmail(user.Id, UserEmail.Text);
        //manager.SetPhoneNumber(user.Id, UserPhone.Text);
        //manager.SetEmail(user.Id, UserEmail.Text);
        if (result.Succeeded)
        {
            IdentityHelper.SignIn(manager, user, isPersistent: false);
            IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
        }
        else
        {
            ErrorMessage.Text = result.Errors.FirstOrDefault();
        }
    }
}