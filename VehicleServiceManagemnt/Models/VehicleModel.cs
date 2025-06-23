using VehicleServiceManagemnt.Models;

namespace VehicleServiceManagemnt.Models
{
    public class VehicleModel
    {
        public int VehicleID { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string VIN { get; set; }

        // Foreign Key
        public int CustomerID { get; set; }
        public CustomerModel? Customer { get; set; } // Navigation property

        public ICollection<ServiceRecordModel>? ServiceRecords { get; set; }
    }
}
