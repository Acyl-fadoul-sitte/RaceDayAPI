



namespace RaceDayMVC1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC services.
            builder.Services.AddControllersWithViews();

            // Configure connection to RaceDayAPI.
            builder.Services.AddHttpClient("RaceDayAPI", client =>
            {
                var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? "http://racedayapi:8080/";

                client.BaseAddress = new Uri(apiUrl);
            });

            // Enable Session.
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Temporarily disabled while testing the connection.
            // app.UseHttpsRedirection();

            app.UseRouting();

            // Enable Session before authorization.
            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
