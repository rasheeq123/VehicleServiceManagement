

namespace VehicleServiceManagemnt.Models
{
    public class BillOfMaterialModel
    {
        public int BOMID { get; set; }
        public int Quantity { get; set; }

        // Foreign Keys
        public int ServiceRecordID { get; set; }
        public ServiceRecordModel? ServiceRecord { get; set; }

        public int ServiceItemID { get; set; }
        public ServiceItemModel? ServiceItem { get; set; }
    }
}