using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.UI;
using xPMWorksWeb;

public partial class Account_Login : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        RegisterHyperLink.NavigateUrl = "Register";
        OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];
        var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
        if (!String.IsNullOrEmpty(returnUrl))
        {
            RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
        }
    }

    protected void LogIn(object sender, EventArgs e)
    {
        try
        {
            if (IsValid)
            {
                // Validate the user password
                var manager = new UserManager();

                //// reset user password force
                //var userStore = new UserStore<IdentityUser>();
                //var userManager = new UserManager<IdentityUser>(userStore);
                //string userName = UserName.Text;
                //var userObject = userManager.FindByName(userName);
                //if (userObject != null)
                //{
                //    userObject.PasswordHash = userManager.PasswordHasher.HashPassword(Password.Text);
                //    var res = userManager.Update(userObject);
                //}

                // find user
                ApplicationUser user = manager.Find(UserName.Text, Password.Text);
                if (user != null)
                {

                    IdentityHelper.SignIn(manager, user, RememberMe.Checked);

                    try
                    {
                        IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                    }
                    catch
                    {
                        IdentityHelper.SignOut(manager, user, RememberMe.Checked);

                        //Response.Redirect("~/", false);
                        //IdentityHelper.RedirectToReturnUrl(".", Response);
                    }

                }
                else
                {
                    FailureText.Text = "Invalid username or password.";
                    ErrorMessage.Visible = true;
                }
            }
        }
        catch (Exception ex)
        {
            FailureText.Text = ex.Message;
            ErrorMessage.Visible = true;
        }
    }
}