using CMSShoppingCart.Models.Data;
using CMSShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSShoppingCart.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {

            //Declare a list of page View Model
            List<PageVM> pagesList;


            using (DataContext db = new DataContext())
            {
                //Initialize the list
                pagesList = db.Pages.ToArray().OrderBy(p => p.Sorting).Select(p => new PageVM(p)).ToList();

            }

            //Return view with the list
            return View(pagesList);
        }


        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }


        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Check modelstate
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DataContext db = new DataContext())
            {
                //Declare slug
                string slug;

                //Initialize PageDTOes
                PageDTOes dto = new PageDTOes();

                //DTOes Title
                dto.Title = model.Title;

                //Check and set Slug
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Make title and slug are unique
                if (db.Pages.Any(p => p.Title == model.Title) || db.Pages.Any(p => p.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }


                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save the DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }


            //Set tempData message
            TempData["SM"] = "You have added a new page";

            //Redirect
            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare PageVM
            PageVM model;

            //Get the page
            using (DataContext db = new DataContext())
            {
                //Select the row from the value in 'id'
                PageDTOes dto = db.Pages.Find(id);

                //Confirm if page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Initialize ViewModel
                model = new PageVM(dto);
            }


            //Return view with Model
            return View(model);

        }

        // GET: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check if model state is valuid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DataContext db = new DataContext())
            {
                //Get page ID
                int id = model.Id;

                //Initialize slug
                string slug = "home";

                //Get the page
                PageDTOes dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //Check slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Make sure title and slug are unique
                if (db.Pages.Where(p => p.Id != id).Any(p => p.Title == model.Title) ||
                    db.Pages.Where(p => p.Id != id).Any(p => p.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug already exists.");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save the DTO
                db.SaveChanges();
            }
            //Set tempData message

            TempData["SM"] = "Successfully edited the page!";

            //Redirect to action
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/DetailsPage/id
        [HttpGet]
        public ActionResult DetailsPage(int id)
        {
            //Declare PageVM
            PageVM model;

            using (DataContext db = new DataContext())
            {
                //get the page
                PageDTOes dto = db.Pages.Find(id);

                //Confirm page if it exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }

        // GET: Admin/Pages/DetailsPage/id
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (DataContext db = new DataContext())
            {
                //Get the page
                PageDTOes dto = db.Pages.Find(id);

                //Remove the page
                db.Pages.Remove(dto);

                //Save
                db.SaveChanges();
            }
            //Redirect
            return RedirectToAction("Index");
        }


        // GET: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (DataContext db = new DataContext())
            {
                //Set initial count
                int count = 1; // home starts from '0'

                //Declare page DTO
                PageDTOes dto;

                //Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }


        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare Model
            SidebarVM model;

            using (DataContext db = new DataContext())
            {
                //Need to get DTO
                SidebarDTOes dto = db.Sidebar.Find(1); //always going to be one that's why it's hard-coded

                //Initialize model
                model = new SidebarVM(dto);


                //return view with model
                return View(model);
            }
        }


        // GET: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (DataContext db = new DataContext())
            {
                //Get the DTO
                SidebarDTOes dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();

                //Set tempData
                TempData["SM"] = "You have successfully edited the Sidebar!";

                //Redirect
                return RedirectToAction("EditSidebar");
            }
        }
    }
}