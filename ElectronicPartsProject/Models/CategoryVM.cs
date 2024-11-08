using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ElectronicPartsProject.Models.DBElectronicPartsModel
{
    [MetadataType(typeof(CategoryVM))]
    public partial class category
    {
        // La clase principal está autogenerada por Entity Framework
    }
    public class CategoryVM
    {
        [Display(Name = "Category Name")]
        public string category_name { get; set; }

        [Display(Name = "Global Ref")]
        public string global_ref_des { get; set; }
    }
}