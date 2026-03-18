using aspnetapp;
using aspnetapp.Hubs;
using aspnetapp.Repositories;
using aspnetapp.Services;
using MongoDB.Driver;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDbContext<CounterContext>();

// MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>()
    ?? new MongoDbSettings();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddSingleton<WorkOrderRepository>();
builder.Services.AddSingleton<PrintTemplateRepository>();

// MES API client
var mesApiSettings = builder.Configuration.GetSection("MesApi").Get<MesApiSettings>()
    ?? new MesApiSettings();
builder.Services.AddSingleton(mesApiSettings);
builder.Services.AddHttpClient<MesApiClient>();

// Print task service
var printTaskSettings = builder.Configuration.GetSection("PrintTask").Get<PrintTaskSettings>()
    ?? new PrintTaskSettings();
builder.Services.AddSingleton(printTaskSettings);
builder.Services.AddSingleton<PrintTaskService>();

// SignalR
builder.Services.AddSignalR();

// Quartz background job
builder.Services.AddQuartz(q =>
{
    var mesCron = builder.Configuration["Quartz:MesSyncCron"] ?? "0 * * * * ?";
    var jobKey = new JobKey("MesDataSyncJob");
    q.AddJob<MesDataSyncJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("MesDataSyncJob-trigger")
        .WithCronSchedule(mesCron));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<PrintHub>("/hubs/print");

app.Run();

