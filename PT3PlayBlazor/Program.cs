using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using KristofferStrube.Blazor.MediaCaptureStreams;
using PT3PlayBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(serviceProvider => (IJSInProcessRuntime)serviceProvider.GetRequiredService<IJSRuntime>());

builder.Services.AddMediaDevicesService();

await builder.Build().RunAsync();
