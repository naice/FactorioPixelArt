using FactorioPixelArt;
using FactorioPixelArt.Controls.Overlay;
using FactorioPixelArt.Navigation;
using FactorioPixelArt.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;
services.AddScoped<IClipboardService, ClipboardService>();
services.AddHttpClient("FactorioPixelArt.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
services.AddScoped(sp => {
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("FactorioPixelArt.ServerAPI");
    return client;
});

// 3rd party
services.AddBlazoredLocalStorage();
services.AddRadzenComponents();

// My
services.AddScoped(typeof(SimpleStorage<>));
services.AddScoped(typeof(SimpleStorage<,>));
services.AddScoped<IOverlayService, OverlayService>();
services.AddScoped<INavigationPanelService, NavigationPanelService>();
services.AddScoped<MyJsInterop>();

var host = builder.Build();

await host.RunAsync();
