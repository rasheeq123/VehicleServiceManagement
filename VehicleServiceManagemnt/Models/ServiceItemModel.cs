

namespace VehicleServiceManagemnt.Models
{
    public class ServiceItemModel
    {
        public int ServiceItemID { get; set; }
        public string ItemName { get; set; }
        public decimal ItemCost { get; set; }
        public ICollection<BillOfMaterialModel>? BillOfMaterials { get; set; }
    }
}