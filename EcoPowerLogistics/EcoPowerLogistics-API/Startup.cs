using EcoPowerLogistics_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoPowerLogistics_API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ecopowerlogisticsdevContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")
            ));
        }
    }
}
