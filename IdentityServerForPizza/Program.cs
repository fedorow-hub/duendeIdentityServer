using Duende.IdentityServer.Services;
using duendeIdentityServer;
using duendeIdentityServer.Data;
using duendeIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

string env = builder.Environment.ContentRootPath;

var connectionString = builder.Configuration.GetValue<string>("DbConnection");

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

//����������� Identity � ��������������� ��� AppUser, IdentityRole
builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
{
    //��������� ���������� � ������
    config.Password.RequiredLength = 4;//������������ ����� ���. 4 �������
    config.Password.RequireDigit = false;//�������� ���������� � ������������� ����
    config.Password.RequireNonAlphanumeric = false;//�������� ���������� � ������������� �� ��������-�������� ��������
    config.Password.RequireUppercase = false;//�������� ���������� � ������������� ��������� ����
})
    .AddEntityFrameworkStores<AuthDbContext>()//��������� DbContext ��� ��������� � Identity
    .AddDefaultTokenProviders();//��������� ��������� ����� ���������� ��� ��������� � ���������� ������� �������

//����������� ������� ������������ ��� Identity server4
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;
}).AddAspNetIdentity<AppUser>()//����������� AppUser ��� AspNetIdentity
    .AddInMemoryApiResources(Configuration.ApiResources)//InMemory ��������� ������ ��� �������� � �������������
    .AddInMemoryIdentityResources(Configuration.IdentityResources)
    .AddInMemoryApiScopes(Configuration.ApiScopes)
    .AddInMemoryClients(Configuration.Clients)//InMemory ��������� ������ ��� ��������
    .AddDeveloperSigningCredential()//������������ ���������������� ���������� �������
    .AddProfileService<ProfileService>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<IProfileService, ProfileService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

SeedDatabase();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}