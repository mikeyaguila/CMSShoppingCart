using CMSShoppingCart.Models.Data;
using CMSShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CMSShoppingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare a list of models
            List<CategoryVM> categoryVMList;

            using (DataContext db = new DataContext())
            {

                //Initialize that list
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(c => c.Sorting)
                    .Select(c => new CategoryVM(c)).ToList();
            }

            //Return view with the list
            return View(categoryVMList);
        }


        // POST: Admin/Shop/Categories
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare id
            string id;

            using (DataContext db = new DataContext())
            {
                //Check category name is unique
                if (db.Categories.Any(c => c.Name == catName))
                    return "titletaken";

                //Initialize the DTO
                CategoryDtoes dto = new CategoryDtoes();


                //ADD DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //SAVE DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //Get Id
                id = dto.Id.ToString();
            }

            //Return Id
            return id;
        }



        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (DataContext db = new DataContext())
            {
                //Set initial count
                int count = 1; // home starts from '0'

                //Declare Category DTO
                CategoryDtoes dto;

                //Set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (DataContext db = new DataContext())
            {
                //Get the category
                CategoryDtoes dto = db.Categories.Find(id);

                //Remove the category
                db.Categories.Remove(dto);

                //Save
                db.SaveChanges();
            }
            //Redirect
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        public string RenameCategory(string newCatName, int id)
        {
            using (DataContext db = new DataContext())
            {
                //Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                //Get DTO
                CategoryDtoes dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save Changes
                db.SaveChanges();
            }

            //Return View
            return "ok";
        }


        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Initialize Model
            ProductVM model = new ProductVM();

            // Add select list of categories to the model
            using (DataContext db = new DataContext())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // Return the view with the model
            return View(model);
        }



        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {

            //Always check model state after form submission
            if (!ModelState.IsValid)
            {
                //Here you can't just return View with the model
                //Because of the SelectList we have in the view - need to populate it each time.

                using (DataContext db = new DataContext())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }


            //Check product Name is unique
            using (DataContext db = new DataContext())
            {
                if (db.Products.Any(p => p.Name == model.Name))
                {
                    //We need this line if we find a match for the product name
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //Declare product Id
            int id;

            //Initialize and save product DTO
            using (DataContext db = new DataContext())
            {
                ProductDtoes product = new ProductDtoes();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;


                //Need the category selected in the dropdownlist
                //Initialize Category Model
                CategoryDtoes catDTO = db.Categories.FirstOrDefault(c => c.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //Get the Id
                id = product.Id; // Primary key of the row we just inserted
            }

            //Set TempData Message
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            //Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            //Check if a file was uploaded
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            //Get file extension
            if (file != null && file.ContentLength > 0)
            {
                //Verify extension
                string ext = file.ContentType.ToLower();

                //Initialize image name
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (DataContext db = new DataContext())
                    {
                        //We need this line if we find a match for the product name
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded! - wrong image extension");
                        return View(model);
                    }
                }

                //Save image name to DTO
                //string imageName = file.FileName;
                string imageName = new FileInfo(file.FileName).Name;

                using (DataContext db = new DataContext())
                {
                    ProductDtoes dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //Set original and thumbnail image path
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //Save original
                file.SaveAs(path);

                //Create and Save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Redirect
            return RedirectToAction("AddProduct");
        }


        // GET: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            //Declare List of product VM
            List<ProductVM> listOfProductVM;

            //Set Page Number
            var pageNumber = page ?? 1;

            using (DataContext db = new DataContext())
            {
                //Initialize the List
                listOfProductVM = db.Products.ToArray()
                                  .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                  .Select(x => new ProductVM(x))
                                  .ToList();

                //Populate Categories Select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }
            //Set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //Return view with list
            return View(listOfProductVM);
        }


        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declare productVM
            ProductVM model;

            using (DataContext db = new DataContext())
            {
                //Get the product
                ProductDtoes dto = db.Products.Find(id);

                //Make sure product exists
                if (dto == null)
                {
                    return Content("That product does not exist!");
                }

                //Initialize Model
                model = new ProductVM(dto);

                //Make a select List
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));
            }

            //Return View with model
            return View(model);
        }


        // POST: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get product Id
            int id = model.Id;

            //Populate categories select list and gallery images
            using (DataContext db = new DataContext())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            //Get all gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                           .Select(fn => Path.GetFileName(fn));


            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Make sure product name is unique
            using (DataContext db = new DataContext())
            {
                if (db.Products.Where(p => p.Id != id).Any(p => p.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }
            //Update product
            using (DataContext db = new DataContext())
            {
                ProductDtoes dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDtoes catDTO = db.Categories.FirstOrDefault(c => c.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //Set TempData message
            TempData["SM"] = "You have successfully updated the product!";

            #region Image Upload

            //Check for file upload
            if (file != null && file.ContentLength > 0)
            {
                //Get extension
                string ext = file.ContentType.ToLower();

                //Verify extension
                //Initialize image name
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (DataContext db = new DataContext())
                    {
                        ModelState.AddModelError("", "The image was not uploaded! - wrong image extension");
                        return View(model);
                    }
                }

                //Set upload directory path
                //Create necessary directories
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                //Check if a file was uploaded
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                //Save image name
                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();


                //Save original and thumb images
                string imageName = new FileInfo(file.FileName).Name;
                using (DataContext db = new DataContext())
                {
                    ProductDtoes dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

               
                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //redirect
            return RedirectToAction("EditProduct");
        }


        // POST: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete product from DB
            using (DataContext db = new DataContext())
            {
                ProductDtoes dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            //Redirect
            return RedirectToAction("Products");
        }


        // POST: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Loop through the files
            foreach(string fileName in Request.Files)
            {
                //Initialize the file
                HttpPostedFileBase file = Request.Files[fileName];

                //Check if not null
                if(file != null && file.ContentLength > 0)
                {
                    //Set directory path
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Set image path
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //Save original and thumb
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }


        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
    }
}