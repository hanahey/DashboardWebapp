using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DashboardWebapp.Startup))]
namespace DashboardWebapp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
