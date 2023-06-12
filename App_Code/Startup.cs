using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(xPMWorksWeb.Startup))]
namespace xPMWorksWeb
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
