using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using ms.Data;
using WebPWrecover.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
//required for Apache reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.RequireHeaderSymmetry = false;
    options.ForwardLimit = 2;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1")); //reverse proxy, Kestrel defaults to port 5000 which is also set in apsettings.json
    options.KnownProxies.Add(IPAddress.Parse("162.205.232.100")); //server IP public
});
//configure listen protocals and assert SSL/TLS requirement
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.ConfigureHttpsDefaults(listenOptions =>
    {
        listenOptions.SslProtocols = SslProtocols.Tls13;
        listenOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;//requires certificate from client
    });
});

//configure connection string from environment variables thus hidding it from production
var environ = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var connectionString = "";
var MS_Email_Pass = "";
if (environ == "Production")
{
    //pulls connection string from environment variables
    connectionString = Environment.GetEnvironmentVariable("MariaDbConnectionStringLocal");
    MS_Email_Pass = Environment.GetEnvironmentVariable(MS_Email_Pass);
}
else
{
    //pulls connection string from development local version of secrets.json
    connectionString = builder.Configuration.GetConnectionString("MariaDbConnectionStringRemote");
    MS_Email_Pass = builder.Configuration.GetConnectionString("MS_Email_Pass");


}
Environment.SetEnvironmentVariable("DbConnectionString", connectionString);//this is used in services to access the string
Environment.SetEnvironmentVariable("GC_Email_Pass", MS_Email_Pass);

//adds the MySQL dbcontext
builder.Services.AddDbContext<ms.Data.ApplicationDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 11)), options => options.EnableRetryOnFailure()));

//requires users to sign in
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = false;
});

//builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

//addition of encryption methods for deployment on linux
builder.Services.AddDataProtection().UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

builder.Services.AddMvc();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    //app.UseHttpsRedirection();//appache handles.  Do not enable!
    //app.UseHsts();//not required SSL/TLS.  Untested configuration!
}
else
{
    //app.UseDeveloperExceptionPage();
}
var configuration = builder.Configuration;
var value = configuration.GetValue<string>("value");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); //All controllers including the API are mapped here.
});

app.UseHsts();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "portal",
    pattern: "{controller=PortalController}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "contactform",
    pattern: "/Contact",
    defaults: new { controller = "ContactController", action = "post" });
app.MapControllerRoute(
    name: "contactform-get",
    pattern: "/Contact",
    defaults: new { controller = "ContactController", action = "get" });
app.MapRazorPages();
app.Run();



