using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ElectronicPartsProject.Models.DBElectronicPartsModel
{
    [MetadataType(typeof(SupplierVM))]
    public partial class supplier
    {
        // La clase principal está autogenerada por Entity Framework
    }
    public class SupplierVM
    {
        [Display(Name = "Supplier Name")]
        public string supplier_name { get; set; }
    }
}