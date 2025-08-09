using HR.Config;
using HR.MAUI.Shared.Services;
using HR.MAUI.Web.Components;
using HR.MAUI.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddIdentityAndDb(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationHandler>();

builder.Services.AddHttpClient("Default", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7073/");
})
.AddHttpMessageHandler<AuthenticationHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));


builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

builder.Services.AddEndpoints();

// Add device-specific services used by the HR.MAUI.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(HR.MAUI.Shared._Imports).Assembly);

app.MapEndpoints();

app.Run();
