using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ASP0000

namespace RtpAnnouncer.Bots
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) return;
            var serviceProvider = services.BuildServiceProvider();
            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}