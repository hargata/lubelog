using CarCareTracker.External.Implementations;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//data access method
if (!string.IsNullOrWhiteSpace(builder.Configuration["POSTGRES_CONNECTION"])){
    builder.Services.AddSingleton<IVehicleDataAccess, PGVehicleDataAccess>();
    builder.Services.AddSingleton<INoteDataAccess, PGNoteDataAccess>();
    builder.Services.AddSingleton<IServiceRecordDataAccess, PGServiceRecordDataAccess>();
    builder.Services.AddSingleton<IGasRecordDataAccess, PGGasRecordDataAccess>();
    builder.Services.AddSingleton<ICollisionRecordDataAccess, PGCollisionRecordDataAccess>();
    builder.Services.AddSingleton<ITaxRecordDataAccess, PGTaxRecordDataAccess>();
    builder.Services.AddSingleton<IReminderRecordDataAccess, PGReminderRecordDataAccess>();
}
else
{
    builder.Services.AddSingleton<IVehicleDataAccess, VehicleDataAccess>();
    builder.Services.AddSingleton<INoteDataAccess, NoteDataAccess>();
    builder.Services.AddSingleton<IServiceRecordDataAccess, ServiceRecordDataAccess>();
    builder.Services.AddSingleton<IGasRecordDataAccess, GasRecordDataAccess>();
    builder.Services.AddSingleton<ICollisionRecordDataAccess, CollisionRecordDataAccess>();
    builder.Services.AddSingleton<ITaxRecordDataAccess, TaxRecordDataAccess>();
    builder.Services.AddSingleton<IReminderRecordDataAccess, ReminderRecordDataAccess>();
}

builder.Services.AddSingleton<IUpgradeRecordDataAccess, UpgradeRecordDataAccess>();
builder.Services.AddSingleton<IUserRecordDataAccess, UserRecordDataAccess>();
builder.Services.AddSingleton<ITokenRecordDataAccess, TokenRecordDataAccess>();
builder.Services.AddSingleton<IUserAccessDataAccess, UserAccessDataAccess>();
builder.Services.AddSingleton<IUserConfigDataAccess, UserConfigDataAccess>();
builder.Services.AddSingleton<ISupplyRecordDataAccess, SupplyRecordDataAccess>();
builder.Services.AddSingleton<IPlanRecordDataAccess, PlanRecordDataAccess>();
builder.Services.AddSingleton<IPlanRecordTemplateDataAccess, PlanRecordTemplateDataAccess>();
builder.Services.AddSingleton<IOdometerRecordDataAccess, OdometerRecordDataAccess>();

//configure helpers
builder.Services.AddSingleton<IFileHelper, FileHelper>();
builder.Services.AddSingleton<IGasHelper, GasHelper>();
builder.Services.AddSingleton<IReminderHelper, ReminderHelper>();
builder.Services.AddSingleton<IReportHelper, ReportHelper>();
builder.Services.AddSingleton<IMailHelper, MailHelper>();
builder.Services.AddSingleton<IConfigHelper, ConfigHelper>();
builder.Services.AddSingleton<ITranslationHelper, TranslationHelper>();

//configure logic
builder.Services.AddSingleton<ILoginLogic, LoginLogic>();
builder.Services.AddSingleton<IUserLogic, UserLogic>();

if (!Directory.Exists("data"))
{
    Directory.CreateDirectory("data");
}

//Additional JsonFile
builder.Configuration.AddJsonFile(StaticHelper.UserConfigPath, optional: true, reloadOnChange: true);

//Configure Auth
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("AuthN").AddScheme<AuthenticationSchemeOptions, Authen>("AuthN", opts => { });
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes("AuthN").RequireAuthenticatedUser().Build();
});
//Configure max file upload size
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
