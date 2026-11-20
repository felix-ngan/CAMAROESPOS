using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CamaroesPOS.Web;
using CamaroesPOS.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<PosStoreState>();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddSingleton<SessionState>();
builder.Services.AddSingleton<LocalizationService>();

await builder.Build().RunAsync();
