﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using taka.Models.DatabaseInteractive;
using taka.Models.Enitities;
using taka.Utils;

namespace taka.Controllers
{
    public class HomeController : Controller
    {

        TakaDB dB = new TakaDB();

        public ActionResult List(int page = 1, string text = "", int cate = 0, int sort = 0, int pageSize = 16, int type = 0, int language = 0, int priceFrom = 0, int priceTo = 0)
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
            return View(listBook.books);
        }

        public ActionResult Index()
        {
            return View(dB.GetHomePage());
        }


        [HttpPost]
        public ActionResult Login(string phone, string password, string returnUrl)
        {
            User user = dB.Login(phone, password);
            if (user != null)
            {
                if (user.is_ban == 1)
                {
                    TempData[C.TEMPDATA.Message] = "Tài khoản của bạn đã bị khoá, vùi lòng liên hiện để biết thêm thông tin";
                    return RedirectToAction("Login", "Home", new { returnUrl, phone });
                }
                FormsAuthentication.SetAuthCookie("" + user.ID, true);
                Session[C.SESSION.UserInfo] = user;
            }
            else
            {
                TempData[C.TEMPDATA.Message] = "Sai số điện thoại hoặc mật khẩu";
                return RedirectToAction("Login", "Home", new { returnUrl, phone });
            }
            if (user.ID.Equals(C.ID_ADMIN))
                return RedirectToAction("Book", "Admin");

            if (returnUrl.Equals(""))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }


        [HttpPost]
        public ActionResult LoginWithGoogle(string googleId, string fullname, string email, string returnUrl = "")
        {
            User user = dB.LoginWithGoogle(googleId, fullname, email);
            if (user.is_ban == 1)
            {
                TempData[C.TEMPDATA.Message] = "Tài khoản của bạn đã bị khoá, vùi lòng liên hiện để biết thêm thông tin";
                return Json(new
                {
                    status = 0,
                    returnUrl = returnUrl
                });
            }

            FormsAuthentication.SetAuthCookie(user.ID == C.ID_ADMIN ? "admin" : "" + user.ID, true);
            Session[C.SESSION.UserInfo] = user;
            return Json(new
            {
                status = 1,
            });
        }



        [HttpPost]
        public ActionResult Register(string phone, string password, string repassword, string email, string gender, string fullname, string birthday, string returnUrl, string tab)
        {
            if (!password.Equals(repassword))
            {
                TempData[C.TEMPDATA.Message] = "Mật khẩu không trùng khớp";
                return RedirectToAction("Login", "Home", new { returnUrl, phone, email, fullname, gender, birthday, tab });
            }

            User user = dB.Register(phone, password, email, gender, fullname, birthday);

            if (user != null)
            {
                Session[C.SESSION.UserInfo] = user;
                FormsAuthentication.SetAuthCookie("" + user.ID, true);
            }
            else
            {
                TempData[C.TEMPDATA.Message] = "Số điện thoại đã tồn tại";
                return RedirectToAction("Login", "Home", new { returnUrl, phone, email, fullname, gender, birthday, tab });
            }

            if (returnUrl.Equals(""))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }

        public ActionResult Logout()
        {
            Session[C.SESSION.UserInfo] = null;
            Session[C.SESSION.Cart] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Detail(int id = -1)
        {
            //try
            //{
                if (id == -1)
                    throw new Exception("Not found");
                var item = dB.GetBookDetail(id);
                var suggestItem = dB.GetSuggestBook(id);
                ViewBag.suggestItem = suggestItem;
                return View(item);
            //}
            //catch (Exception)
            //{
            //    return RedirectToAction("Error", "Home");
            //}

        }
        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Login(string phone = "", string email = "", string gender = "", string fullname = "", string birthday = "", string returnUrl = "", string tab = "login")
        {
            if (Session[C.SESSION.UserInfo] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            FormsAuthentication.SignOut();
            ViewBag.returnUrl = returnUrl;
            ViewBag.phone = phone;
            ViewBag.email = email;
            ViewBag.gender = gender;
            ViewBag.fullname = fullname;
            ViewBag.birthday = birthday;
            ViewBag.tab = tab;
            return View();
        }
    }
}