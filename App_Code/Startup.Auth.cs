using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;

namespace xPMWorksWeb
{
    public partial class Startup {

        // 인증을 구성하는 방법에 대한 자세한 내용을 보려면 http://go.microsoft.com/fwlink/?LinkId=301883을 방문하십시오.
        public void ConfigureAuth(IAppBuilder app)
        {
            // 응용 프로그램이 쿠키를 사용하여 로그인된 사용자에 대한 정보를 저장할 수 있도록 합니다.
            // 또한 타사 로그인 공급자를 통해 사용자 로깅에 대한 정보를 저장할 수 있도록 합니다.
            // 사용자가 응용 프로그램에 로그인할 수 있는 경우 이 옵션은 필수입니다.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // 타사 로그인 공급자로 로그인할 수 있으려면 다음 줄의 주석 처리를 제거하십시오.
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}
