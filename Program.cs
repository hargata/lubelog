using CarCareTracker.External.Implementations;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//Additional JsonFile
builder.Configuration.AddJsonFile(StaticHelper.UserConfigPath, optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile(StaticHelper.ServerConfigPath, optional: true, reloadOnChange: true);

if (!string.IsNullOrWhiteSpace(builder.Configuration["LUBELOGGER_LOCALE_OVERRIDE"]))
{
    var overrideCulture = new CultureInfo(builder.Configuration["LUBELOGGER_LOCALE_OVERRIDE"] ?? string.Empty);
    if (!string.IsNullOrWhiteSpace(builder.Configuration["LUBELOGGER_LOCALE_DT_OVERRIDE"]))
    {
        var overrideDTFormat = new CultureInfo(builder.Configuration["LUBELOGGER_LOCALE_DT_OVERRIDE"] ?? string.Empty);
        overrideCulture.DateTimeFormat = overrideDTFormat.DateTimeFormat;
    }
    CultureInfo.DefaultThreadCurrentCulture = overrideCulture;
    CultureInfo.DefaultThreadCurrentUICulture = overrideCulture;
}

//Print Messages
StaticHelper.InitMessage(builder.Configuration);
//Check Migration
StaticHelper.CheckMigration(builder.Environment.WebRootPath, builder.Environment.ContentRootPath);

// Add services to the container.
builder.Services.AddControllersWithViews();

//LiteDB is always injected even if user uses Postgres.
builder.Services.AddSingleton<ILiteDBHelper, LiteDBHelper>();

//data access method
if (!string.IsNullOrWhiteSpace(builder.Configuration["POSTGRES_CONNECTION"])){
    builder.Services.AddSingleton<IVehicleDataAccess, PGVehicleDataAccess>();
    builder.Services.AddSingleton<INoteDataAccess, PGNoteDataAccess>();
    builder.Services.AddSingleton<IServiceRecordDataAccess, PGServiceRecordDataAccess>();
    builder.Services.AddSingleton<IGasRecordDataAccess, PGGasRecordDataAccess>();
    builder.Services.AddSingleton<ICollisionRecordDataAccess, PGCollisionRecordDataAccess>();
    builder.Services.AddSingleton<ITaxRecordDataAccess, PGTaxRecordDataAccess>();
    builder.Services.AddSingleton<IReminderRecordDataAccess, PGReminderRecordDataAccess>();
    builder.Services.AddSingleton<IUpgradeRecordDataAccess, PGUpgradeRecordDataAccess>();
    builder.Services.AddSingleton<IOdometerRecordDataAccess, PGOdometerRecordDataAccess>();
    builder.Services.AddSingleton<ISupplyRecordDataAccess, PGSupplyRecordDataAccess>();
    builder.Services.AddSingleton<IPlanRecordDataAccess, PGPlanRecordDataAccess>();
    builder.Services.AddSingleton<IPlanRecordTemplateDataAccess, PGPlanRecordTemplateDataAccess>();
    builder.Services.AddSingleton<IUserConfigDataAccess, PGUserConfigDataAccess>();
    builder.Services.AddSingleton<IUserRecordDataAccess, PGUserRecordDataAccess>();
    builder.Services.AddSingleton<ITokenRecordDataAccess, PGTokenRecordDataAccess>();
    builder.Services.AddSingleton<IUserAccessDataAccess, PGUserAccessDataAccess>();
    builder.Services.AddSingleton<IExtraFieldDataAccess, PGExtraFieldDataAccess>();
    builder.Services.AddSingleton<IInspectionRecordDataAccess, PGInspectionRecordDataAccess>();
    builder.Services.AddSingleton<IInspectionRecordTemplateDataAccess, PGInspectionRecordTemplateDataAccess>();
    builder.Services.AddSingleton<IEquipmentRecordDataAccess, PGEquipmentRecordDataAccess>();
    builder.Services.AddSingleton<IUserHouseholdDataAccess, PGUserHouseholdDataAccess>();
    builder.Services.AddSingleton<IApiKeyRecordDataAccess, PGApiKeyRecordDataAccess>();
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
    builder.Services.AddSingleton<IUpgradeRecordDataAccess, UpgradeRecordDataAccess>();
    builder.Services.AddSingleton<IOdometerRecordDataAccess, OdometerRecordDataAccess>();
    builder.Services.AddSingleton<ISupplyRecordDataAccess, SupplyRecordDataAccess>();
    builder.Services.AddSingleton<IPlanRecordDataAccess, PlanRecordDataAccess>();
    builder.Services.AddSingleton<IPlanRecordTemplateDataAccess, PlanRecordTemplateDataAccess>();
    builder.Services.AddSingleton<IUserConfigDataAccess, UserConfigDataAccess>();
    builder.Services.AddSingleton<IUserRecordDataAccess, UserRecordDataAccess>();
    builder.Services.AddSingleton<ITokenRecordDataAccess, TokenRecordDataAccess>();
    builder.Services.AddSingleton<IUserAccessDataAccess, UserAccessDataAccess>();
    builder.Services.AddSingleton<IExtraFieldDataAccess, ExtraFieldDataAccess>();
    builder.Services.AddSingleton<IInspectionRecordDataAccess, InspectionRecordDataAccess>();
    builder.Services.AddSingleton<IInspectionRecordTemplateDataAccess, InspectionRecordTemplateDataAccess>();
    builder.Services.AddSingleton<IEquipmentRecordDataAccess, EquipmentRecordDataAccess>();
    builder.Services.AddSingleton<IUserHouseholdDataAccess, UserHouseholdDataAccess>();
    builder.Services.AddSingleton<IApiKeyRecordDataAccess, ApiKeyRecordDataAccess>();
}

//configure helpers
builder.Services.AddSingleton<IFileHelper, FileHelper>();
builder.Services.AddSingleton<IGasHelper, GasHelper>();
builder.Services.AddSingleton<IEquipmentHelper, EquipmentHelper>();
builder.Services.AddSingleton<IReminderHelper, ReminderHelper>();
builder.Services.AddSingleton<IReportHelper, ReportHelper>();
builder.Services.AddSingleton<IConfigHelper, ConfigHelper>();
builder.Services.AddSingleton<ITranslationHelper, TranslationHelper>();
builder.Services.AddSingleton<IMailHelper, MailHelper>();

//configure logic
builder.Services.AddSingleton<ILoginLogic, LoginLogic>();
builder.Services.AddSingleton<IUserLogic, UserLogic>();
builder.Services.AddSingleton<IOdometerLogic, OdometerLogic>();
builder.Services.AddSingleton<IVehicleLogic, VehicleLogic>();

//Configure Auth
builder.Services.AddHttpClient();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("AuthN").AddScheme<AuthenticationSchemeOptions, Authen>("AuthN", opts => { });
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes("AuthN").RequireAuthenticatedUser().Build();
});
builder.Services.AddHealthChecks();
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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "data", "images")),
    RequestPath = "/images",
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/images"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-store");
            var userIsAuthenticated = ctx.Context.User.Identity?.IsAuthenticated ?? false;
            if (!userIsAuthenticated)
            {
                ctx.Context.Response.Redirect("/Login");
            }
        }
    }
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "data", "documents")),
    RequestPath = "/documents",
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/documents"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-store");
            var userIsAuthenticated = ctx.Context.User.Identity?.IsAuthenticated ?? false;
            if (!userIsAuthenticated)
            {
                ctx.Context.Response.Redirect("/Login");
            }
        }
    }
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "data", "translations")),
    RequestPath = "/translations"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "data", "temp")),
    RequestPath = "/temp",
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/temp"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-store");
            var userIsAuthenticated = ctx.Context.User.Identity?.IsAuthenticated ?? false;
            if (!userIsAuthenticated)
            {
                ctx.Context.Response.Redirect("/Login");
            }
        }
    }
});

app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api") && ctx.Request.ContentType == "application/json",
    ab => ab.UseMiddleware<BufferBody>()
);

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
