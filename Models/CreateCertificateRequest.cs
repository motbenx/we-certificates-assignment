using System.ComponentModel.DataAnnotations;

namespace InsuranceCertificates.Models;

public class CreateCertificateRequest
{
    [Required]
    public required string CustomerName { get; set; }

    [Required]
    public required DateTime CustomerDateOfBirth { get; set; }

    [Required]
    public required string InsuredItem { get; set; }

    [Required]
    [Range(20, 200, ErrorMessage = "Insured sum must be between $20 and $200")]
    public required decimal InsuredSum { get; set; }
}