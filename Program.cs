using InsuranceCertificates.Data;
using InsuranceCertificates.Domain;
using InsuranceCertificates.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("database"));

// Register validation service
builder.Services.AddScoped<CertificateValidationService>();

var app = builder.Build();

FeedCertificates(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();


void FeedCertificates(IServiceProvider provider)
{
    using var scope = provider.CreateScope();
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var validationService = scope.ServiceProvider.GetRequiredService<CertificateValidationService>();

    // Clear existing certificates to ensure fresh mock data
    appDbContext.Certificates.RemoveRange(appDbContext.Certificates);
    
    var creationDate = DateTime.UtcNow;
    var (validFrom, validTo) = validationService.CalculateValidityDates(creationDate);

    appDbContext.Certificates.Add(new Certificate()
    {
        Number = "00001",
        CreationDate = creationDate,
        ValidFrom = validFrom,
        ValidTo = validTo,
        CertificateSum = 25,
        InsuredItem = "Apple iPhone 14 PRO",
        InsuredSum = 150,
        Customer = new Customer()
        {
            Name = "Customer 1",
            DateOfBirth = new DateTime(1990, 1, 1)
        }
    });

    appDbContext.SaveChanges();
}