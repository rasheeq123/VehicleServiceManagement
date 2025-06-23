

public enum ServiceStatus
{
    Scheduled,
    InProgress,
    Completed
}

namespace VehicleServiceManagemnt.Models
{
    public class ServiceRecordModel
    {
        public int ServiceRecordID { get; set; }
        public DateTime ServiceDate { get; set; }
        public ServiceStatus Status { get; set; }
        public decimal TotalCost { get; set; }

        // Foreign Keys
        public int VehicleID { get; set; }
        public VehicleModel Vehicle { get; set; }

        public int ServiceRepID { get; set; }
        public ServiceRepresentativeModel ServiceRepresentative { get; set; }

        public ICollection<BillOfMaterialModel> BillOfMaterials { get; set; }
    }
}