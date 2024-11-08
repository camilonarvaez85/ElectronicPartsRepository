using ElectronicPartsProject.Models;
using ElectronicPartsProject.Models.DBElectronicPartsModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicPartsProject.Controllers
{
    public class CategoryController : Controller
    {
        DB_ElectronicPartsEntities dbElectroParts = new DB_ElectronicPartsEntities();
        // GET: Category
        public ActionResult Index(CategoryFilterVM filters, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 14;

            var categoriesQuery = dbElectroParts.categories.AsQueryable();

            if (!string.IsNullOrEmpty(filters.CategoryName))
                categoriesQuery = categoriesQuery.Where(c => c.category_name.Contains(filters.CategoryName));
            if (!string.IsNullOrEmpty(filters.GlobalRefDes))
                categoriesQuery = categoriesQuery.Where(c => c.global_ref_des.Contains(filters.GlobalRefDes));

            // Cargar la paginación con los datos filtrados
            var pagedList = categoriesQuery.OrderBy(p => p.category_name).ToPagedList(pageNumber, pageSize);

            // Guardar filtros y página actuales en ViewBag
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = pageNumber;

            return View(pagedList); // Pasar la lista de categorías como modelo
        }

        public ActionResult FilterCategories(string categoryName = null, string globalRefDes = null, int? page = 1)
        {
            int pageNumber = page ?? 1;
            int pageSize = 14;

            var categoriesQuery = dbElectroParts.categories.AsQueryable();

            if (!string.IsNullOrEmpty(categoryName))
            {
                categoriesQuery = categoriesQuery.Where(c => c.category_name.Contains(categoryName));
            }

            if (!string.IsNullOrEmpty(globalRefDes))
            {
                categoriesQuery = categoriesQuery.Where(c => c.global_ref_des.Contains(globalRefDes));
            }

            // Paginación
            var pagedList = categoriesQuery.OrderBy(p => p.category_name).ToPagedList(pageNumber, pageSize);

            return PartialView("_CategoriesTable", pagedList);
        }

        public ActionResult Create()
        {
            category newCategory = new category();

            return View(newCategory);
        }

        [HttpPost]
        public ActionResult Create(category newCategory)
        {
            dbElectroParts.categories.Add(newCategory);
            dbElectroParts.SaveChanges();

            return RedirectToAction("Index", "Category");
        }

        public ActionResult Edit(int id, CategoryFilterVM filters, int? page)
        {
            category editCategory = dbElectroParts.categories.Where(c => c.category_id == id).FirstOrDefault();
            if (editCategory == null)
            {
                return HttpNotFound();
            }
            // Guardar los filtros actuales en ViewBag para regresar a la lista
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = page;

            return View(editCategory);
        }

        [HttpPost]
        public ActionResult Edit(category editedCat)
        {
            //category editCategory = dbElectroParts.categories.Where(c => c.category_id == id).FirstOrDefault();
            dbElectroParts.Entry(editedCat).State = EntityState.Modified;
            dbElectroParts.SaveChanges();
            return RedirectToAction("Index", "Category");
        }

        public ActionResult Delete(int id, CategoryFilterVM filters, int? page)
        {
            category deleteCategory = dbElectroParts.categories.Where(c => c.category_id == id).FirstOrDefault();

            if (deleteCategory == null)
            {
                return HttpNotFound();
            }
            // Guardar los filtros actuales en ViewBag para regresar a la lista
            ViewBag.CurrentFilters = filters;
            ViewBag.CurrentPage = page;

            return View(deleteCategory);
        }

        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var categoryToDelete = dbElectroParts.categories.FirstOrDefault(c => c.category_id == id);

            if (categoryToDelete == null)
            {
                return HttpNotFound();
            }

            if (categoryToDelete.parts.Any())
            {
                TempData["ErrorMessage"] = "The category cannot be deleted because it has associated parts.";
                return RedirectToAction("Delete", new { id = id });
            }
            else {
                dbElectroParts.categories.Remove(categoryToDelete);
                dbElectroParts.SaveChanges();

                return RedirectToAction("Index", "Category");
            }

            
        }
    }
}