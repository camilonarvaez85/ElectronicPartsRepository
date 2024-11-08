using ElectronicPartsProject.Models.DBElectronicPartsModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PagedList;
using ElectronicPartsProject.Models;

namespace ElectronicPartsProject.Controllers
{
    public class PartController : Controller
    {
        DB_ElectronicPartsEntities dbElectroParts = new DB_ElectronicPartsEntities();
        // GET: Part
        public ActionResult Index(PartFilterVM filters, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 14;

            // Verifica y aplica los filtros al query de partes
            var partsQuery = dbElectroParts.parts.Include("category").AsQueryable();

            if (!string.IsNullOrEmpty(filters.PartNumber))
                partsQuery = partsQuery.Where(p => p.part_number.Contains(filters.PartNumber));
            if (!string.IsNullOrEmpty(filters.Category))
                partsQuery = partsQuery.Where(p => p.category.category_name.Contains(filters.Category));
            if (!string.IsNullOrEmpty(filters.Description))
                partsQuery = partsQuery.Where(p => p.description.Contains(filters.Description));
            if (!string.IsNullOrEmpty(filters.ComponentValue))
                partsQuery = partsQuery.Where(p => p.component_value.Contains(filters.ComponentValue));
            if (!string.IsNullOrEmpty(filters.Footprint))
                partsQuery = partsQuery.Where(p => p.footprint.Contains(filters.Footprint));
            if (filters.IsActive.HasValue)
                partsQuery = partsQuery.Where(p => p.is_active == filters.IsActive.Value);

            // Cargar la paginación con los datos filtrados
            var pagedList = partsQuery.OrderBy(p => p.part_number).ToPagedList(pageNumber, pageSize);

            // Guardar filtros y página actuales en ViewBag
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = pageNumber;

            return View(pagedList);
        }



        public ActionResult FilterParts(string partNumber = null, string category = null, string description = null, string componentValue = null, string footprint = null, bool? isActive = null, int? page = 1)
        {
            int pageNumber = page ?? 1;
            int pageSize = 14;

            var partsQuery = dbElectroParts.parts.Include("category").AsQueryable();

            // Filtros por cada columna
            if (!string.IsNullOrEmpty(partNumber))
            {
                partsQuery = partsQuery.Where(p => p.part_number.Contains(partNumber));
            }

            if (!string.IsNullOrEmpty(category))
            {
                partsQuery = partsQuery.Where(p => p.category.category_name.Contains(category));
            }

            if (!string.IsNullOrEmpty(description))
            {
                partsQuery = partsQuery.Where(p => p.description.Contains(description));
            }

            if (!string.IsNullOrEmpty(componentValue))
            {
                partsQuery = partsQuery.Where(p => p.component_value.Contains(componentValue));
            }

            if (!string.IsNullOrEmpty(footprint))
            {
                partsQuery = partsQuery.Where(p => p.footprint.Contains(footprint));
            }

            if (isActive.HasValue)
            {
                partsQuery = partsQuery.Where(p => p.is_active == isActive.Value);
            }

            // Paginación
            var pagedList = partsQuery.OrderBy(p => p.part_number).ToPagedList(pageNumber, pageSize);

            return PartialView("_PartsTable", pagedList);
        }


        public ActionResult Create()
        {
            // Obtener todas las categorías para el DropDownList
            ViewBag.category_id = new SelectList(dbElectroParts.categories, "category_id", "category_name");
            return View();
        }

        [HttpPost]
        public ActionResult Create(part aPart, string supplierParts, IEnumerable<HttpPostedFileBase> files)
        {
            ModelState.Remove("files");

            if (!ModelState.IsValid)
            {
                ViewBag.category_id = new SelectList(dbElectroParts.categories, "category_id", "category_name", aPart.category_id);
                return View(aPart);
            }

            var serializer = new JavaScriptSerializer();
            var suppliersList = serializer.Deserialize<List<supplier_parts>>(supplierParts);

            using (var transaction = dbElectroParts.Database.BeginTransaction())
            {
                try
                {
                    // Agregar la parte y guardar para obtener el `part_id`
                    dbElectroParts.parts.Add(aPart);
                    dbElectroParts.SaveChanges();

                    // Obtener todos los nombres de proveedores en suppliersList
                    var supplierNames = suppliersList.Select(sp => sp.supplier.supplier_name).Distinct().ToList();

                    // Buscar proveedores existentes en la base de datos
                    var existingSuppliers = dbElectroParts.suppliers
                        .Where(s => supplierNames.Contains(s.supplier_name))
                        .ToDictionary(s => s.supplier_name, s => s.supplier_id);

                    foreach (var supplierPart in suppliersList)
                    {
                        // Usar el supplier_id si el proveedor ya existe, o crearlo si no existe
                        if (!existingSuppliers.TryGetValue(supplierPart.supplier.supplier_name, out int supplierId))
                        {
                            var newSupplier = new supplier
                            {
                                supplier_name = supplierPart.supplier.supplier_name
                            };
                            dbElectroParts.suppliers.Add(newSupplier);
                            dbElectroParts.SaveChanges();

                            supplierId = newSupplier.supplier_id;
                            existingSuppliers[supplierPart.supplier.supplier_name] = supplierId;
                        }

                        // Asignar el supplier_id al supplierPart y agregarlo al contexto
                        supplierPart.supplier_id = supplierId;
                        supplierPart.part_id = aPart.part_id;

                        supplierPart.supplier = null;
                        dbElectroParts.supplier_parts.Add(supplierPart);
                    }

                    // Guardar archivos asociados a la parte
                    if (files != null && files.Any())
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                byte[] fileData;
                                using (var binaryReader = new BinaryReader(file.InputStream))
                                {
                                    fileData = binaryReader.ReadBytes(file.ContentLength);
                                }

                                var fileEntity = new file
                                {
                                    file_name = file.FileName,
                                    file_type = file.ContentType,
                                    file_data = fileData,
                                    upload_date = DateTime.Now,
                                    part_id = aPart.part_id
                                };

                                dbElectroParts.files.Add(fileEntity);
                            }
                        }
                    }

                    dbElectroParts.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("Detail", new { id = aPart.part_id });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "An error occurred while saving data: " + ex.Message);
                    ViewBag.category_id = new SelectList(dbElectroParts.categories, "category_id", "category_name", aPart.category_id);
                    return View(aPart);
                }
            }
        }

        public ActionResult Edit(int id)
        {
            // Cargar la parte con sus archivos y suppliers asociados
            var part = dbElectroParts.parts
                .Include("files")
                .Include("supplier_parts.supplier") // Incluye los suppliers relacionados
                .FirstOrDefault(p => p.part_id == id);

            if (part == null)
            {
                return HttpNotFound();
            }

            // Cargar categorías para el DropDownList en la vista
            ViewBag.category_id = new SelectList(dbElectroParts.categories, "category_id", "category_name", part.category_id);

            // Serializar los suppliers asociados en JSON
            ViewBag.ExistingSupplierParts = JsonConvert.SerializeObject(part.supplier_parts.Select(sp => new
            {
                supplier = new { supplier_name = sp.supplier.supplier_name },
                manufacturer_part_number = sp.manufacturer_part_number
            }));


            return View(part);
        }



        [HttpPost]
        public ActionResult Edit(part aPart, IEnumerable<HttpPostedFileBase> newFiles, string suppliersToAdd, string suppliersToDelete)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.category_id = new SelectList(dbElectroParts.categories, "category_id", "category_name", aPart.category_id);
                return View(aPart);
            }

            var existingPart = dbElectroParts.parts
                .Include("files")
                .Include("supplier_parts") // Incluye supplier_parts para poder modificar los registros
                .FirstOrDefault(p => p.part_id == aPart.part_id);

            if (existingPart != null)
            {
                dbElectroParts.Entry(existingPart).CurrentValues.SetValues(aPart);

                // Procesar nuevos archivos
                if (newFiles != null)
                {
                    foreach (var file in newFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            byte[] fileData;
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                fileData = binaryReader.ReadBytes(file.ContentLength);
                            }

                            var fileEntity = new file
                            {
                                file_name = file.FileName,
                                file_type = file.ContentType,
                                file_data = fileData,
                                upload_date = DateTime.Now,
                                part_id = aPart.part_id
                            };

                            dbElectroParts.files.Add(fileEntity);
                        }
                    }
                }

                var suppliersToAddList = !string.IsNullOrEmpty(suppliersToAdd) ? JsonConvert.DeserializeObject<List<supplier_parts>>(suppliersToAdd) : new List<supplier_parts>();
                var suppliersToDeleteList = !string.IsNullOrEmpty(suppliersToDelete) ? JsonConvert.DeserializeObject<List<supplier_parts>>(suppliersToDelete) : new List<supplier_parts>();

                // Procesar eliminaciones de supplier_parts
                foreach (var supplierPartToDelete in suppliersToDeleteList)
                {
                    var supplierPart = existingPart.supplier_parts.FirstOrDefault(sp =>
                        sp.supplier.supplier_name == supplierPartToDelete.supplier.supplier_name &&
                        sp.manufacturer_part_number == supplierPartToDelete.manufacturer_part_number);

                    if (supplierPart != null)
                    {
                        dbElectroParts.supplier_parts.Remove(supplierPart);

                        // Verificar si el proveedor tiene otros registros supplier_parts
                        var supplierId = supplierPart.supplier_id;
                        bool hasOtherParts = dbElectroParts.supplier_parts.Any(sp => sp.supplier_id == supplierId);

                        // Si no tiene otros registros, eliminar el proveedor
                        if (!hasOtherParts)
                        {
                            var supplierToDelete = dbElectroParts.suppliers.FirstOrDefault(s => s.supplier_id == supplierId);
                            if (supplierToDelete != null)
                            {
                                dbElectroParts.suppliers.Remove(supplierToDelete);
                            }
                        }
                    }
                }

                // Agregar nuevos registros en supplier_parts
                foreach (var supplierPart in suppliersToAddList)
                {
                    var existingSupplier = dbElectroParts.suppliers.FirstOrDefault(s => s.supplier_id == supplierPart.supplier_id);

                    if (existingSupplier == null)
                    {
                        var newSupplier = new supplier
                        {
                            supplier_name = supplierPart.supplier.supplier_name
                        };
                        dbElectroParts.suppliers.Add(newSupplier);
                        dbElectroParts.SaveChanges();
                        supplierPart.supplier_id = newSupplier.supplier_id;
                    }

                    var newSupplierPart = new supplier_parts
                    {
                        part_id = aPart.part_id,
                        supplier_id = supplierPart.supplier_id,
                        manufacturer_part_number = supplierPart.manufacturer_part_number
                    };
                    dbElectroParts.supplier_parts.Add(newSupplierPart);
                }

                dbElectroParts.SaveChanges();
                return RedirectToAction("Detail", new { id = aPart.part_id });
            }

            return HttpNotFound();
        }



        public ActionResult Delete(int id, PartFilterVM filters, int? page)
        {
            // Cargar la parte junto con sus archivos asociados
            var part = dbElectroParts.parts
                .Include("category")
                .Include("files")
                .FirstOrDefault(p => p.part_id == id );
            if (part == null)
            {
                return HttpNotFound();
            }
            // Guardar los filtros actuales en ViewBag para regresar a la lista
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = page;

            return View(part);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int part_id)
        {
            // Obtener la parte con sus archivos asociados
            var part = dbElectroParts.parts.Include("files").FirstOrDefault(p => p.part_id == part_id);
            if (part != null)
            {
                dbElectroParts.files.RemoveRange(part.files);

                dbElectroParts.supplier_parts.RemoveRange(part.supplier_parts);

                // Eliminar la parte
                dbElectroParts.parts.Remove(part);

                // Guardar los cambios en la base de datos
                dbElectroParts.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult DeleteFile(int id)
        {
            // Buscar el archivo por su ID
            var file = dbElectroParts.files.FirstOrDefault(f => f.file_id == id);
            if (file != null)
            {
                int partId = file.part_id ?? 0; // Obtener el ID de la parte asociada

                // Eliminar el archivo de la base de datos
                dbElectroParts.files.Remove(file);
                dbElectroParts.SaveChanges();

                // Redirigir a la vista de edición de la parte con el ID de la parte
                return RedirectToAction("Edit", new { id = partId });
            }

            return HttpNotFound();
        }

        public ActionResult Detail(int id, PartFilterVM filters, int? page)
        {
            // Buscar la parte por ID, incluyendo proveedores y archivos asociados
            var part = dbElectroParts.parts
                .Include("category")
                .Include("supplier_parts.supplier") // Incluir proveedores asociados
                .Include("files")                   // Incluir archivos asociados
                .FirstOrDefault(p => p.part_id == id);

            if (part == null)
            {
                return HttpNotFound();
            }
            // Guardar los filtros actuales en ViewBag para regresar a la lista
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = page;

            return View(part);
        }


        public ActionResult DownloadFile(int fileId)
        {
            var file = dbElectroParts.files.Find(fileId);
            if (file == null)
            {
                return HttpNotFound();
            }

            // Devuelve el archivo como una descarga
            return File(file.file_data, file.file_type, file.file_name);
        }


    }
}