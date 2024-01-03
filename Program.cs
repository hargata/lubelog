using CarCareTracker.External.Implementations;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IVehicleDataAccess, VehicleDataAccess>();
builder.Services.AddSingleton<INoteDataAccess, NoteDataAccess>();
builder.Services.AddSingleton<IServiceRecordDataAccess, ServiceRecordDataAccess>();
builder.Services.AddSingleton<IGasRecordDataAccess, GasRecordDataAccess>();
builder.Services.AddSingleton<ICollisionRecordDataAccess, CollisionRecordDataAccess>();
builder.Services.AddSingleton<ITaxRecordDataAccess, TaxRecordDataAccess>();
builder.Services.AddSingleton<IFileHelper, FileHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
