using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShoppingCartList.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = "http://localhost:7071/";
if (builder.HostEnvironment.IsProduction())
{
    baseAddress = "https://shoppingcartlist-api.azurewebsites.net";
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

await builder.Build().RunAsync();
