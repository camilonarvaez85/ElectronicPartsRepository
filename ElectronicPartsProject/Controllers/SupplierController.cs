using ElectronicPartsProject.Models.DBElectronicPartsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicPartsProject.Controllers
{
    public class SupplierController : Controller
    {

        DB_ElectronicPartsEntities dbElectroParts = new DB_ElectronicPartsEntities();

        // GET: Supplier
        public ActionResult Index()
        {
            var suppliersList = dbElectroParts.suppliers.ToList();
            return View(suppliersList);
        }

        public ActionResult FilterSuppliers(string supplierName = null)
        {
            var suppliersQuery = dbElectroParts.suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(supplierName))
            {
                suppliersQuery = suppliersQuery.Where(s => s.supplier_name.Contains(supplierName));
            }

            return PartialView("_SuppliersTable", suppliersQuery.ToList());
        }

        // Crear un nuevo proveedor (GET)
        public ActionResult Create()
        {
            return View();
        }

        // Crear un nuevo proveedor (POST)
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(supplier newSupplier)
        {
            if (ModelState.IsValid)
            {
                dbElectroParts.suppliers.Add(newSupplier);
                dbElectroParts.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(newSupplier);
        }

        // Editar un proveedor (GET)
        public ActionResult Edit(int id)
        {
            var supplier = dbElectroParts.suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // Editar un proveedor (POST)
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit(supplier editedSupplier)
        {
            if (ModelState.IsValid)
            {
                dbElectroParts.Entry(editedSupplier).State = System.Data.Entity.EntityState.Modified;
                dbElectroParts.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(editedSupplier);
        }


        // Eliminar un proveedor (GET)
        public ActionResult Delete(int id)
        {
            var supplierToDelete = dbElectroParts.suppliers.FirstOrDefault(s => s.supplier_id == id);

            if (supplierToDelete == null)
            {
                return HttpNotFound();
            }

            return View(supplierToDelete);
        }

        // Confirmar eliminación de un proveedor (POST)
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var supplierToDelete = dbElectroParts.suppliers.FirstOrDefault(s => s.supplier_id == id);

            if (supplierToDelete == null)
            {
                return HttpNotFound();
            }

            // Verificar si tiene registros asociados en supplier_parts
            if (supplierToDelete.supplier_parts.Any())
            {
                TempData["ErrorMessage"] = "The supplier cannot be deleted because it has associated parts.";
                return RedirectToAction("Delete", new { id = id });
            }

            // Eliminar el proveedor si no tiene partes asociadas
            dbElectroParts.suppliers.Remove(supplierToDelete);
            dbElectroParts.SaveChanges();

            return RedirectToAction("Index", "Supplier");
        }

        // Método para obtener nombres de proveedores
        public JsonResult GetSuppliers(string term)
        {
            var suppliers = dbElectroParts.suppliers
                .Where(s => s.supplier_name.Contains(term))
                .Select(s => s.supplier_name)
                .Distinct()
                .ToList();

            return Json(suppliers, JsonRequestBehavior.AllowGet);
        }

        // Método para obtener números de parte del fabricante
        public JsonResult GetManufacturerPartNumbers(string term)
        {
            var partNumbers = dbElectroParts.supplier_parts
                .Where(sp => sp.manufacturer_part_number.Contains(term))
                .Select(sp => sp.manufacturer_part_number)
                .Distinct()
                .ToList();

            return Json(partNumbers, JsonRequestBehavior.AllowGet);
        }

    }
}