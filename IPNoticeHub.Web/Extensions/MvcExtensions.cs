namespace IPNoticeHub.Web.Extensions
{
    public static class MvcExtensions
    {
        public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSwaggerGen();

            return services;
        }
    }
}
