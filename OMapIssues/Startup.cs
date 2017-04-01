using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OMapIssues.Startup))]
namespace OMapIssues
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
