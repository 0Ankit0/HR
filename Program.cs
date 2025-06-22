using System.Configuration;
using HR.Api;
using HR.Components;
using HR.Config;
using HR.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddIdentityAndDb(builder.Configuration);

// Add this service
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationHandler>();

// Update your HttpClient registration
builder.Services.AddHttpClient("Default", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7073/");
})
.AddHttpMessageHandler<AuthenticationHandler>();

// Keep this line to make the client injectable
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
//builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

//app.MapRazorPages();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAccountApi();

app.Run();
public class TwoFactorLoginRequest
{

    public string TwoFactorCode { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
    public string? ReturnUrl { get; set; }
}
