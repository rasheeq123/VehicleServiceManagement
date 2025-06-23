using System.ComponentModel.DataAnnotations;
namespace VehicleServiceManagemnt.Models
{
    public class ServiceRepresentativeModel
    {
        public int ServiceRepID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        public ICollection<ServiceRecordModel>? ServiceRecords { get; set; }
    }
}