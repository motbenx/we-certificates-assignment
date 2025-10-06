using InsuranceCertificates.Data;
using InsuranceCertificates.Domain;
using InsuranceCertificates.Models;
using InsuranceCertificates.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceCertificates.Controllers;

[ApiController]
[Route("[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    private readonly CertificateValidationService _validationService;

    public CertificatesController(AppDbContext appDbContext, CertificateValidationService validationService)
    {
        _appDbContext = appDbContext;
        _validationService = validationService;
    }

    [HttpGet]
    public async Task<IEnumerable<CertificateModel>> Get()
    {
        return await _appDbContext.Certificates.Select(c => new CertificateModel
        {
            Number = c.Number,
            CreationDate = c.CreationDate,
            ValidFrom = c.ValidFrom,
            ValidTo = c.ValidTo,
            CustomerName = c.Customer.Name,
            CustomerDateOfBirth = c.Customer.DateOfBirth,
            InsuredItem = c.InsuredItem,
            InsuredSum = c.InsuredSum,
            CertificateSum = c.CertificateSum
        }).ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request)
    {
        var creationDate = DateTime.UtcNow;

        // Validate customer age (must be 18+)
        var ageValidation = _validationService.ValidateCustomerAge(request.CustomerDateOfBirth, creationDate);
        if (!ageValidation.IsValid)
        {
            return BadRequest(new { error = ageValidation.ErrorMessage });
        }

        // Validate insured sum and calculate certificate sum
        var certificateSumValidation = _validationService.ValidateAndCalculateCertificateSum(request.InsuredSum);
        if (!certificateSumValidation.IsValid)
        {
            return BadRequest(new { error = certificateSumValidation.ErrorMessage });
        }

        // Calculate validity dates
        var (validFrom, validTo) = _validationService.CalculateValidityDates(creationDate);

        // Generate certificate number
        var lastCertificateNumber = await GetLastCertificateNumberAsync();
        var certificateNumber = _validationService.GenerateNextCertificateNumber(lastCertificateNumber);

        // Create customer
        var customer = new Customer
        {
            Name = request.CustomerName,
            DateOfBirth = request.CustomerDateOfBirth
        };

        // Create certificate
        var certificate = new Certificate
        {
            Number = certificateNumber,
            CreationDate = creationDate,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Customer = customer,
            InsuredItem = request.InsuredItem,
            InsuredSum = request.InsuredSum,
            CertificateSum = certificateSumValidation.Value!
        };

        _appDbContext.Certificates.Add(certificate);
        await _appDbContext.SaveChangesAsync();

        // Return the created certificate
        var certificateModel = new CertificateModel
        {
            Number = certificate.Number,
            CreationDate = certificate.CreationDate,
            ValidFrom = certificate.ValidFrom,
            ValidTo = certificate.ValidTo,
            CustomerName = certificate.Customer.Name,
            CustomerDateOfBirth = certificate.Customer.DateOfBirth,
            InsuredItem = certificate.InsuredItem,
            InsuredSum = certificate.InsuredSum,
            CertificateSum = certificate.CertificateSum
        };

        return CreatedAtAction(nameof(Get), new { id = certificate.Id }, certificateModel);
    }

    private async Task<int> GetLastCertificateNumberAsync()
    {
        var lastCertificate = await _appDbContext.Certificates
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (lastCertificate == null)
        {
            return 0; // Start from 0, so first certificate will be 00001
        }

        // Parse the certificate number to integer
        if (int.TryParse(lastCertificate.Number, out var number))
        {
            return number;
        }

        // If parsing fails, count all certificates as fallback
        return await _appDbContext.Certificates.CountAsync();
    }
}