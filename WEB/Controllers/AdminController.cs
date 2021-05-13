﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using taka.Models.DatabaseInteractive;
using taka.Models.Enitities;
using taka.Utils;

namespace taka.Controllers
{
    [Authorize(Users = "admin")]
    public class AdminController : Controller
    {

        TakaDB dB = new TakaDB();
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.listUsers = dB.GetUsers();
            ViewBag.listCategories = dB.GetCategories();
            ViewBag.listPublishers = dB.GetPublishers();
            ViewBag.listAuthors = dB.GetAuthors();
            ViewBag.listTypes = dB.GetTypes();
            ViewBag.listLanguages = dB.GetLanguages();
            ViewBag.tab = "order";
            return View();
        }
        public ActionResult Edit(int id = -1)
        {
            ViewBag.listCategories = dB.GetCategories();
            ViewBag.listAuthors = dB.GetAuthors();
            ViewBag.listPublishers = dB.GetPublishers();
            ViewBag.listLanguages = dB.GetLanguages();
            ViewBag.listTypes = dB.GetTypes();
            try
            {
                if (id == -1)
                    throw new Exception("Not found");
                var item = dB.GetBookDetail(id);
                return View(item);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost]
        public ActionResult EditBook(int ID,
             IEnumerable<HttpPostedFileBase> Images,
             IEnumerable<int> images_delete,
            string Title,
            int Price,
            int idCategory,
            int idAuthor,
            int idPublisher,
            int idLanguage,
            int idType,
            string Page,
            string Date,
            int Quantity,
            string Description)
        {
            dB.EditBook(ID, images_delete, Images, Title, Price, idCategory, idAuthor, idPublisher, idLanguage, idType, Page, Date, Quantity, Description);
            return RedirectToAction("Detail", "Home", new { id = ID });
        }

        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                if (id == -1)
                    throw new Exception("Not found");
                dB.DeleteBook(id,true);
                return RedirectToAction("List", "Home");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }


        [HttpPost]
        public ActionResult AddBook(
            IEnumerable<HttpPostedFileBase> Images,
            string Title,
            int Price,
            int idCategory,
            int idAuthor,
            int idPublisher,
            int idLanguage,
            int idType,
            string Page,
            string Date,
            int Quantity,
            string Description)
        {
            Book book = dB.AddBook(Images, Title, Price, idCategory, idAuthor, idPublisher, idLanguage, idType, Page, Date, Quantity, Description);
            return RedirectToAction("Detail", "Home", new { id = book.ID });
        }

        public ActionResult Add()
        {
            ViewBag.listCategories = dB.GetCategories();
            ViewBag.listPublishers = dB.GetPublishers();
            ViewBag.listAuthors = dB.GetAuthors();
            ViewBag.listLanguages = dB.GetLanguages();
            ViewBag.listTypes = dB.GetTypes();
            return View();
        }

        public ActionResult Manager(string tab = "order", string text = "")
        {
            ViewBag.listUsers = dB.GetUsers();
            ViewBag.listCategories = dB.GetCategories();
            ViewBag.listPublishers = dB.GetPublishers();
            ViewBag.listAuthors = dB.GetAuthors();
            ViewBag.listTypes = dB.GetTypes();
            ViewBag.listLanguages = dB.GetLanguages();
            ViewBag.tab = tab;
            return View();
        }

        public ActionResult BanUser(int id, int ban = 0)
        {
            dB.BanUser(id, ban);
            return RedirectToAction("Manager", "Admin", new { tab = "user" });
        }

        public ActionResult UpdateUser(string phone, string email, string fullname, string gender, string birthday)
        {
            dB.UpdateUser(phone, email, fullname, gender, birthday);
            return RedirectToAction("Manager", "Admin", new { tab = "user" });
        }

        public ActionResult UpdateCategory(int id, string name)
        {
            dB.UpdateCategory(id, name);
            return RedirectToAction("Manager", "Admin", new { tab = "category" });
        }

        public ActionResult AddCategory(string name)
        {
            dB.AddCategory(name);
            return RedirectToAction("Manager", "Admin", new { tab = "category" });
        }

        public ActionResult RemoveCategory(int id)
        {
            dB.RemoveCategory(id);
            return RedirectToAction("Manager", "Admin", new { tab = "category" });
        }

        public ActionResult UpdateLanguage(int id, string name)
        {
            dB.UpdateLanguage(id, name);
            return RedirectToAction("Manager", "Admin", new { tab = "language" });
        }

        public ActionResult AddLanguage(string name)
        {
            dB.AddLanguage(name);
            return RedirectToAction("Manager", "Admin", new { tab = "language" });
        }

        public ActionResult RemoveLanguage(int id)
        {
            dB.RemoveLanguage(id);
            return RedirectToAction("Manager", "Admin", new { tab = "language" });
        }

        public ActionResult UpdatePublisher(int id, string name)
        {
            dB.UpdatePublisher(id, name);
            return RedirectToAction("Manager", "Admin", new { tab = "publisher" });
        }

        public ActionResult AddPublisher(string name)
        {
            dB.AddPublisher(name);
            return RedirectToAction("Manager", "Admin", new { tab = "publisher" });
        }

        public ActionResult RemovePublisher(int id)
        {
            dB.RemovePublisher(id);
            return RedirectToAction("Manager", "Admin", new { tab = "publisher" });
        }


        public ActionResult UpdateAuthor(int id, string name)
        {
            dB.UpdateAuthor(id, name);
            return RedirectToAction("Manager", "Admin", new { tab = "author" });
        }

        public ActionResult AddAuthor(string name)
        {
            dB.AddAuthor(name);
            return RedirectToAction("Manager", "Admin", new { tab = "author" });
        }

        public ActionResult RemoveAuthor(int id)
        {
            dB.RemoveAuthor(id);
            return RedirectToAction("Manager", "Admin", new { tab = "author" });
        }


        public ActionResult UpdateType(int id, string name)
        {
            dB.UpdateType(id, name);
            return RedirectToAction("Manager", "Admin", new { tab = "type" });
        }

        public ActionResult AddType(string name)
        {
            dB.AddType(name);
            return RedirectToAction("Manager", "Admin", new { tab = "type" });
        }

        public ActionResult RemoveType(int id)
        {
            dB.RemoveType(id);
            return RedirectToAction("Manager", "Admin", new { tab = "type" });
        }
    }
}