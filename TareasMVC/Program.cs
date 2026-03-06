using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using TareasMVC;
using TareasMVC.Entidades;
using TareasMVC.Servicios;
using AutoMapper;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
}).AddJsonOptions(opciones =>
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AgregarAdmin", policy =>
        policy.RequireClaim("AgregarAdmin", "true"))
    .AddPolicy("EliminarAdmin", policy =>
        policy.RequireClaim("EliminarAdmin", "true"));


builder.Services.AddDbContext<ApplicationDbContext>(opciones 
    => opciones.UseSqlServer("name=DefaultConnection"));
builder.Services.AddAuthentication();
builder.Services.AddIdentity<User, IdentityRole>(opciones => 
{
    opciones.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/usuarios/Login";
    
    opciones.AccessDeniedPath = "/usuarios/login";
});

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
