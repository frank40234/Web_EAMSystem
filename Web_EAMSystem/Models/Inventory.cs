using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Web_EAMSystem.Models
{
    public class Inventory
    {
        [Key]
        public Guid INVENTORY_ID { get; set;}

        [ForeignKey("ASSET_ID")]
        public Guid ASSET_ID {  get; set;}

        [ForeignKey("BIN_ID")]
        public Guid BIN_ID {  get; set;}

        public int QTY { get; set;}
        public DateTime ModifiedDate { get; set; }
    }
}
