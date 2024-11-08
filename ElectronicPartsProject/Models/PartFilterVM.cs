using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicPartsProject.Models
{
    public class PartFilterVM
    {
        public string PartNumber { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ComponentValue { get; set; }
        public string Footprint { get; set; }
        public bool? IsActive { get; set; }
    }

}