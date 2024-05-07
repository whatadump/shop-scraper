using System;
using System.Net.Http;
using ShopScraper.Application;
using ShopScraper.Domain;
using ShopScraper.Infrastructure;
using ShopScraper.Infrastructure.Entities;
using ShopScraper.Web.Client;
using ShopScraper.Web.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.UseBusinessApplication(builder.Configuration);

builder.Services.UseInteractiveApplication(builder.Configuration);

await builder.Build().RunAsync();
