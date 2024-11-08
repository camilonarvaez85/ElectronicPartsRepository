using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ElectronicPartsProject.Models.DBElectronicPartsModel
{

    [MetadataType(typeof(PartVM))]
    public partial class part
    {
        // La clase principal está autogenerada por Entity Framework
    }

    public class PartVM
    {
        [Display(Name = "Part Number")]
        public string part_number { get; set; }

        [Display(Name = "Description")]
        public string description { get; set; }

        [Display(Name = "Component Value")]
        public string component_value { get; set; }

        [Display(Name = "Footprint")]
        public string footprint { get; set; }

        [Display(Name = "Package Code")]
        public string pkg_code { get; set; }

        [Display(Name = "Schematic Symbol File")]
        public string schematic_symbol_file { get; set; }

        [Display(Name = "Value")]
        public string value { get; set; }

        [Display(Name = "Active")]
        public bool is_active { get; set; }
    }
}