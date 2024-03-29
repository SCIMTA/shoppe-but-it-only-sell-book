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
    //[Authorize(Users = "admin")]
    [Authorize(Users = "4")]
    public class AdminController : Controller
    {

        TakaDB dB = new TakaDB();

        // GET: Admin
        public ActionResult Book(int page = 1, string text = "", int cate = 0, int sort = 0, int pageSize = 16, int type = 0, int language = 0, int priceFrom = 0, int priceTo = 0)
        {
            ViewBag.ListCate = dB.GetCategories();
            ViewBag.ListType = dB.GetTypes();
            ViewBag.ListLanguage = dB.GetLanguages();
            ViewBag.Cate = cate;
            ViewBag.Sort = sort;
            ViewBag.Type = type;
            if (priceFrom > priceTo)
                priceTo = 0;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.Language = language;
            ViewBag.PageSize = 16;
            ViewBag.CurrentPage = page;
            switch (sort)
            {
                case 0:
                    ViewBag.TextSort = C.DROPDOWN_SORT.NEWEST;
                    break;
                case 1:
                    ViewBag.TextSort = C.DROPDOWN_SORT.OLDEST;
                    break;
                case 2:
                    ViewBag.TextSort = C.DROPDOWN_SORT.LOWEST_PRICE;
                    break;
                case 3:
                    ViewBag.TextSort = C.DROPDOWN_SORT.HIGHEST_PRICE;
                    break;
            }

            if (pageSize != 16 && pageSize % 16 == 0 && pageSize <= 64)
            {
                ViewBag.PageSize = pageSize;
            }
            ListBook listBook = dB.GetListBook(page, text, cate, sort, pageSize, type, language, priceFrom, priceTo);
            ViewBag.ListPage = HelperFunctions.getNumPage(page, listBook.pages);
            ViewBag.maxPage = listBook.pages;
            ViewBag.TextSearch = text;
            ViewBag.list = listBook.books;
            return View();
        }


        public ActionResult Order()
        {
            List<Order> processingOrder = dB.GetProcessingOrders();
            List<Order> doneOrder = dB.GetDoneOrders();
            ViewBag.ProcessingOrders = processingOrder;
            ViewBag.ProcessingOrdersAddresses = processingOrder.Select(x => dB.GetAddressByIdAddress(x.IDAddress)).ToList();
            ViewBag.DoneOrders = doneOrder;
            ViewBag.DoneOrdersAddresses = doneOrder.Select(x => dB.GetAddressByIdAddress(x.IDAddress)).ToList();
            return View();
        }

        public ActionResult User(string text = "")
        {
            ViewBag.list = dB.GetUsers(text);
            ViewBag.TextSearch = text;
            return View();
        }
        public ActionResult Category()
        {
            ViewBag.list = dB.GetCategories();
            return View();
        }
        public ActionResult Publisher(string text = "")
        {
            ViewBag.list = dB.GetPublishers(text);
            ViewBag.TextSearch = text;
            return View();
        }
        public ActionResult Author(string text = "")
        {
            ViewBag.list = dB.GetAuthors(text);
            ViewBag.TextSearch = text;
            return View();
        }
        public ActionResult Type()
        {
            ViewBag.list = dB.GetTypes();
            return View();
        }
        public ActionResult Language()
        {
            ViewBag.list = dB.GetLanguages();
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
                return RedirectToAction("Book", "Admin");
            }
        }

        [HttpPost]
        public ActionResult EditBook(int ID,
             IEnumerable<HttpPostedFileBase> Images,
             IEnumerable<int> images_delete,
            string Title,
            string Price,
            int idCategory,
            int idAuthor,
            int idPublisher,
            int idLanguage,
            int idType,
            string Page,
            string Date,
            int Quantity,
            string Description,
            int[] idImage,
            int[] indexImage)
        {
            try
            {
                dB.EditBook(ID,
                    images_delete,
                    Images,
                    Title,
                    Price,
                    idCategory,
                    idAuthor,
                    idPublisher,
                    idLanguage,
                    idType,
                    Page,
                    Date,
                    Quantity,
                    Description,
                    idImage,
                    indexImage,
                    Server);
                TempData[C.TEMPDATA.Message] = "Cập nhật sách thành công";
                return RedirectToAction("Book", "Admin");
            }
            catch (Exception)
            {
                TempData[C.TEMPDATA.Message] = "Cập nhật sách thất bại, vui lòng thử lại";
                return RedirectToAction("Edit", "Admin", new { id = ID });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                if (id == -1)
                    throw new Exception("Not found");
                dB.DeleteBook(id, true);
                TempData[C.TEMPDATA.Message] = "Đã xoá sách thành công";
            }
            catch (Exception)
            {
                TempData[C.TEMPDATA.Message] = "Xoá sách thất bại, vui lòng thử lại";
            }
            return RedirectToAction("Book", "Admin");
        }


        [HttpPost]
        public ActionResult AddBook(
            IEnumerable<HttpPostedFileBase> Images,
            string Title,
            string Price,
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

            try
            {
                Book book = dB.AddBook(Images, Title, Price, idCategory, idAuthor, idPublisher, idLanguage, idType, Page, Date, Quantity, Description, Server);
                TempData[C.TEMPDATA.Message] = "Tạo sách thành công";
                return RedirectToAction("Book", "Admin");
            }
            catch (Exception)
            {
                TempData[C.TEMPDATA.Message] = "Tạo sách thất bại, vui lòng thử lại";
                return RedirectToAction("Add", "Admin");
            }

        }


        [HttpPost]
        public ActionResult ConfirmOrder(int id)
        {
            dB.ConfirmOrder(id);
            return RedirectToAction("Order", "Admin");
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

        public ActionResult BanUser(int id, int ban = 0)
        {
            dB.BanUser(id, ban);
            return RedirectToAction("User", "Admin");
        }

        public ActionResult UpdateUser(string phone, string email, string fullname, string gender, string birthday)
        {
            dB.UpdateUser(phone, email, fullname, gender, birthday);
            return RedirectToAction("User", "Admin");
        }

        public ActionResult UpdateCategory(int id, string name)
        {
            dB.UpdateCategory(id, name);
            return RedirectToAction("Category", "Admin");
        }

        public ActionResult AddCategory(string name)
        {
            dB.AddCategory(name);
            return RedirectToAction("Category", "Admin");
        }

        public ActionResult RemoveCategory(int id)
        {
            dB.RemoveCategory(id);
            return RedirectToAction("Category", "Admin");
        }

        public ActionResult UpdateLanguage(int id, string name)
        {
            dB.UpdateLanguage(id, name);
            return RedirectToAction("Language", "Admin");
        }

        public ActionResult AddLanguage(string name)
        {
            dB.AddLanguage(name);
            return RedirectToAction("Language", "Admin");
        }

        public ActionResult RemoveLanguage(int id)
        {
            dB.RemoveLanguage(id);
            return RedirectToAction("Language", "Admin");
        }

        public ActionResult UpdatePublisher(int id, string name)
        {
            dB.UpdatePublisher(id, name);
            return RedirectToAction("Publisher", "Admin");
        }

        public ActionResult AddPublisher(string name)
        {
            dB.AddPublisher(name);
            return RedirectToAction("Publisher", "Admin");
        }

        public ActionResult RemovePublisher(int id)
        {
            dB.RemovePublisher(id);
            return RedirectToAction("Publisher", "Admin");
        }


        public ActionResult UpdateAuthor(int id, string name)
        {
            dB.UpdateAuthor(id, name);
            return RedirectToAction("Author", "Admin");
        }

        public ActionResult AddAuthor(string name)
        {
            dB.AddAuthor(name);
            return RedirectToAction("Author", "Admin");
        }

        public ActionResult RemoveAuthor(int id)
        {
            dB.RemoveAuthor(id);
            return RedirectToAction("Author", "Admin");
        }


        public ActionResult UpdateType(int id, string name)
        {
            dB.UpdateType(id, name);
            return RedirectToAction("Type", "Admin");
        }

        public ActionResult AddType(string name)
        {
            dB.AddType(name);
            return RedirectToAction("Type", "Admin");
        }

        public ActionResult RemoveType(int id)
        {
            dB.RemoveType(id);
            return RedirectToAction("Type", "Admin");
        }
    }
}