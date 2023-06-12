using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using System;
using xPMWorksWeb;

namespace xPMWorksWeb
{
    // ����� Ŭ������ �� ���� �Ӽ��� �߰��Ͽ� ����ڿ� ���� ����� �����͸� �߰��� �� �ֽ��ϴ�. �ڼ��� �˾ƺ����� http://go.microsoft.com/fwlink/?LinkID=317594�� �湮�Ͻʽÿ�.
    public class ApplicationUser : IdentityUser
    {
        // �߰��Ѱ�!!!!
        //public string user_Name { get; set; }
        //public string userPhone { get; set; }
        //public string userEmail { get; set; }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
    }

    #region �����
    public class UserManager : UserManager<ApplicationUser>
    {
        public UserManager()
            : base(new UserStore<ApplicationUser>(new ApplicationDbContext()))
        {
        }
    }
}

namespace xPMWorksWeb
{
    public static class IdentityHelper
    {
        // �ܺ� �α����� ������ �� XSRF�� ����
        public const string XsrfKey = "XsrfId";

        public static void SignIn(UserManager manager, ApplicationUser user, bool isPersistent)
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = manager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request[ProviderNameKey];
        }

        public static string GetExternalLoginRedirectUrl(string accountProvider)
        {
            return "/Account/RegisterExternalLogin?" + ProviderNameKey + "=" + accountProvider;
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void SignOut(UserManager manager, ApplicationUser user, bool isPersistent)
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl, false);
            }
            else
            {
                response.Redirect("~/", false);
            }
        }
    }
    #endregion
}