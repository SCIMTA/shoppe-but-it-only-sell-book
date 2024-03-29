﻿using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using taka.Models.Enitities;
using taka.Utils;

namespace taka.Models.DatabaseInteractive
{
    public class BillItem
    {
        public int id { get; set; }
        public string bookName { get; set; }
        public int price { get; set; }
        public int quantity { get; set; }
    }
    public class ListBook
    {
        public int pages { get; set; }
        public Category cate { get; set; }

        public List<Book> books { get; set; }

        public ListBook(int pages, List<Book> books)
        {
            this.pages = pages;
            this.books = books;
        }
    }
    public class TakaDB
    {
        TakaDBContext takaDB;

        public TakaDB()
        {
            takaDB = new TakaDBContext();
        }

        public ListBook GetListBook(int page = 1, string text = "", int cate = 0, int sort = 0, int pageSize = 16, int type = 0, int language = 0, int priceFrom = 0, int priceTo = 0)
        {
            var removeUnicode = HelperFunctions.RemoveUnicode(text);
            var listItem = takaDB.Books.Where(x => x.isHidden != 1);
            listItem = listItem.Where(m => m.KeySearch.Contains(removeUnicode));
            if (cate != 0)
                listItem = listItem.Where(m => m.idCategory == cate);

            if (type != 0)
                listItem = listItem.Where(m => m.idType == type);

            if (language != 0)
                listItem = listItem.Where(m => m.idLanguage == language);
            if (priceTo > 0)
                listItem = listItem.Where(m => (m.Price > priceFrom && m.Price < priceTo));
            else if (priceFrom > 0)
                listItem = listItem.Where(m => m.Price > priceFrom);

            switch (sort)
            {
                case 0:
                    listItem = listItem.OrderByDescending(m => m.ID);
                    break;
                case 1:
                    listItem = listItem.OrderBy(m => m.ID);
                    break;
                case 2:
                    listItem = listItem.OrderBy(m => m.Price);
                    break;
                case 3:
                    listItem = listItem.OrderByDescending(m => m.Price);
                    break;
            }

            int maxPage = listItem.Count() / pageSize + 1;
            return new ListBook(maxPage, listItem.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public List<ListBook> GetHomePage()
        {
            int pageSize = 10;
            var listItem = takaDB.Books.Where(x => x.isHidden != 1);
            List<ListBook> list = new List<ListBook>();
            foreach (var cate in GetCategories())
            {
                ListBook listBook = new ListBook(0, listItem.Where(m => m.idCategory == cate.ID).OrderBy(m => m.ID).Take(pageSize).ToList());
                listBook.cate = cate;
                list.Add(listBook);
            }
            return list;
        }

        public List<Book> GetSuggestBook(int idBook)
        {
            int numIteme = 10;
            var book = takaDB.Books.Where(x => x.isHidden != 1 && x.ID.Equals(idBook)).First();
            Random r = new Random();
            var listByCate = takaDB.Books.Where(x => x.idCategory == book.idCategory);
            int rInt = r.Next(0, listByCate.Count() - 11);
            List<Book> list = listByCate.OrderBy(m => m.ID).Skip(rInt).Take(numIteme).ToList();
            return list;
        }



        public List<Category> GetCategories()
        {
            return takaDB.Categories.ToList();
        }

        public void RateBook(int idBook, int idUser, string comment, int star)
        {
            Rate rate = new Rate();
            rate.idBook = idBook;
            rate.idUser = idUser;
            rate.Comment = comment;
            rate.Rate1 = star;
            takaDB.Rates.Add(rate);
            takaDB.SaveChanges();
        }

        public List<Author> GetAuthors(string text = "")
        {
            var removeUnicode = HelperFunctions.RemoveUnicode(text);
            var listAuthor = takaDB.Authors.Where(x => x.KeySearch.Contains(removeUnicode)).ToList();
            return listAuthor;
        }
        public List<Publisher> GetPublishers(string text = "")
        {
            var removeUnicode = HelperFunctions.RemoveUnicode(text);
            var list = takaDB.Publishers.Where(x => x.KeySearch.Contains(removeUnicode)).ToList();
            return list;
        }
        public List<Language> GetLanguages()
        {
            return takaDB.Languages.ToList();
        }

        public List<Enitities.Type> GetTypes()
        {
            return takaDB.Types.ToList();
        }

        public List<User> GetUsers(string text = "")
        {
            var removeUnicode = HelperFunctions.RemoveUnicode(text);
            var list = takaDB.Users.Where(x => (!x.ID.Equals(C.ID_ADMIN) && x.KeySearch.Contains(removeUnicode))).ToList();
            //return takaDB.Users.Where(x => !x.ID.Equals(C.ID_ADMIN)).ToList();
            return list;
        }

        public string findTextCategory(int id)
        {
            return takaDB.Categories.Where(x => x.ID == id).First().Name;
        }

        public Book GetBookDetail(int id)
        {
            return takaDB.Books.Where(x => x.ID == id && x.isHidden != 1).First();
        }

        public User Register(string phone, string password, string email = "", string gender = "", string fullname = "", string birthday = "")
        {
            if (takaDB.Users.Where(x => x.Phone == phone).Count() > 0)
                return null;
            User user = new User();
            user.Phone = phone.Replace("+84", "0");
            user.Password = HelperFunctions.sha256(password);
            user.Email = email;
            user.Fullname = fullname;
            user.Gender = gender;
            user.Birthday = birthday.Length == 0 ? DateTime.Now.ToShortDateString() : birthday;
            takaDB.Users.Add(user);
            takaDB.SaveChanges();
            return user;
        }

        public User UpdateUser(string phone, string email, string fullname, string gender, string birthday)
        {
            User user = takaDB.Users.Where(x => x.Phone == phone).First();
            if (user == null)
                return null;
            user.Email = email;
            user.Fullname = fullname;
            user.Gender = gender;
            user.Birthday = birthday.Length == 0 ? DateTime.Now.ToShortDateString() : birthday;
            takaDB.SaveChanges();
            return user;
        }
        public void BanUser(int ID, int ban = 0)
        {
            User user = takaDB.Users.Where(x => x.ID == ID).First();
            if (user == null)
                return;
            user.is_ban = ban;
            takaDB.SaveChanges();
        }

        public void UpdateCategory(int id, string name)
        {
            Category cate = takaDB.Categories.Where(x => x.ID == id).First();
            if (cate == null)
                return;
            cate.Name = name;
            takaDB.SaveChanges();
        }

        public void AddCategory(string name)
        {
            if (takaDB.Categories.Where(e => e.Name.Equals(name)).Count() > 0)
                return;
            Category cate = new Category();
            cate.Name = name;
            takaDB.Categories.Add(cate);
            takaDB.SaveChanges();
        }

        public void RemoveCategory(int id)
        {
            try
            {
                Category cate = takaDB.Categories.Where(x => x.ID == id).First();
                takaDB.Categories.Remove(cate);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }

        public void UpdateLanguage(int id, string name)
        {
            Language lang = takaDB.Languages.Where(x => x.ID == id).First();
            if (lang == null)
                return;
            lang.Name = name;
            takaDB.SaveChanges();
        }

        public void AddLanguage(string name)
        {
            if (takaDB.Languages.Where(e => e.Name.Equals(name)).Count() > 0)
                return;
            Language lang = new Language();
            lang.Name = name;
            takaDB.Languages.Add(lang);
            takaDB.SaveChanges();
        }

        public void RemoveLanguage(int id)
        {
            try
            {
                Language lang = takaDB.Languages.Where(x => x.ID == id).First();
                takaDB.Languages.Remove(lang);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }

        public void ConfirmOrder(int id)
        {
            Order order = takaDB.Orders.Where(x => x.ID.Equals(id)).First();
            if (order != null)
            {
                order.OrderStatus = 1;
                order.CreateDate = DateTime.Now;
                takaDB.SaveChanges();
            }
        }

        public void UpdatePublisher(int id, string name)
        {
            Publisher pub = takaDB.Publishers.Where(x => x.ID == id).First();
            if (pub == null)
                return;
            pub.Name = name;
            takaDB.SaveChanges();
        }

        public void AddPublisher(string name)
        {
            if (takaDB.Publishers.Where(e => e.Name.Equals(name)).Count() > 0)
                return;
            Publisher pub = new Publisher();
            pub.Name = name;
            takaDB.Publishers.Add(pub);
            takaDB.SaveChanges();
        }

        public void RemovePublisher(int id)
        {
            try
            {
                Publisher pub = takaDB.Publishers.Where(x => x.ID == id).First();
                takaDB.Publishers.Remove(pub);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }



        public void UpdateAuthor(int id, string name)
        {
            Author author = takaDB.Authors.Where(x => x.ID == id).First();
            if (author == null)
                return;
            author.Name = name;
            takaDB.SaveChanges();
        }

        public void AddAuthor(string name)
        {
            if (takaDB.Publishers.Where(e => e.Name.Equals(name)).Count() > 0)
                return;
            Author author = new Author();
            author.Name = name;
            takaDB.Authors.Add(author);
            takaDB.SaveChanges();
        }

        public void RemoveAuthor(int id)
        {
            try
            {
                Author author = takaDB.Authors.Where(x => x.ID == id).First();
                takaDB.Authors.Remove(author);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }


        public void UpdateType(int id, string name)
        {
            Enitities.Type type = takaDB.Types.Where(x => x.ID == id).First();
            if (type == null)
                return;
            type.Name = name;
            takaDB.SaveChanges();
        }

        public void AddType(string name)
        {
            if (takaDB.Types.Where(e => e.Name.Equals(name)).Count() > 0)
                return;
            Enitities.Type author = new Enitities.Type();
            author.Name = name;
            takaDB.Types.Add(author);
            takaDB.SaveChanges();
        }

        public void RemoveType(int id)
        {
            try
            {
                Enitities.Type author = takaDB.Types.Where(x => x.ID == id).First();
                takaDB.Types.Remove(author);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }


        public User Login(string phone, string password)
        {
            string hashpass = HelperFunctions.sha256(password);
            var user = takaDB.Users.Where(x => x.Phone == phone && x.Password == hashpass);
            if (user.Count() > 0)
                return user.First();
            return null;
        }
        public User LoginWithGoogle(string gooogleId, string fullname, string email)
        {
            var user = takaDB.Users.Where(x => x.google_id == gooogleId);
            if (user.Count() > 0)
                return user.First();
            User newUser = new User();
            newUser.google_id = gooogleId;
            newUser.Fullname = fullname;
            newUser.Email = email;
            newUser.Birthday = C.DEFAULT_USER_VALUE.BIRTHDAY;
            newUser.Gender = C.DEFAULT_USER_VALUE.GENDER;
            newUser.Phone = email;
            takaDB.Users.Add(newUser);
            takaDB.SaveChanges();
            return newUser;
        }

        public Cart AddCart(int idBook, int idUser, int quantity = 1)
        {
            Cart cart;
            var find_cart = takaDB.Carts.Where(x => x.idBook == idBook && x.idUser == idUser);
            if (find_cart.Count() > 0)
            {
                find_cart.First().Quantity += quantity;
                cart = find_cart.First();
            }
            else
            {
                cart = new Cart();
                cart.idBook = idBook;
                cart.idUser = idUser;
                cart.Quantity = quantity;
                takaDB.Carts.Add(cart);
            }
            takaDB.SaveChanges();
            return cart;
        }
        public List<Cart> GetListCarts(int idUser, int take = -1)
        {
            var listCarts = takaDB.Carts.Where(x => x.idUser == idUser).ToList();
            if (!take.Equals(-1))
                listCarts = listCarts.Take(take).ToList();
            return listCarts;
        }
        public void DeleteCartItem(int idUser, int idBook)
        {
            Cart deleteItem = takaDB.Carts.Where(x => x.idUser == idUser && x.idBook == idBook).First();
            takaDB.Carts.Remove(deleteItem);
            takaDB.SaveChanges();
        }
        public bool DeleteBook(int id, bool permanently = false)
        {
            var item = takaDB.Books.Where(x => x.ID == id).First();
            if (item != null)
                if (!permanently)
                {
                    item.isHidden = 1;
                }
                else
                {
                    takaDB.Carts.RemoveRange(takaDB.Carts.Where(x => x.idBook == id));
                    takaDB.Images.RemoveRange(takaDB.Images.Where(x => x.idBook == id));
                    takaDB.Rates.RemoveRange(takaDB.Rates.Where(x => x.idBook == id));
                    takaDB.OrderDetails.RemoveRange(takaDB.OrderDetails.Where(x => x.idBook == id));
                    takaDB.Books.Remove(item);
                }
            takaDB.SaveChanges();
            return true;
        }




        public Book AddBook(IEnumerable<HttpPostedFileBase> Images, string Title,
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
            HttpServerUtilityBase Server
            )
        {
            Book book = new Book();
            book.Title = Title;
            book.Price = int.Parse(string.Join("", Price.Split('.').ToArray()));
            book.idCategory = idCategory;
            book.idAuthor = idAuthor;
            book.idPublisher = idPublisher;
            book.idLanguage = idLanguage;
            book.idType = idType;
            book.Page = Page;
            book.Date = Date;
            book.Quantity = Quantity;
            book.Description = Description;
            book.RateCount = 0;
            book.RatePoint = 0;
            takaDB.Books.Add(book);
            takaDB.SaveChanges();
            if (Images != null && Images.Count() > 0)
            {
                foreach (var image in Images)
                {
                    try
                    {
                        UploadImage(book.ID, image, Server);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            takaDB.SaveChanges();
            return book;
        }

        public Image UploadImage(int idBook, HttpPostedFileBase image, HttpServerUtilityBase Server)
        {
            try
            {
                string fn = DateTime.Now.Ticks + "_" + idBook + "_" + Path.GetFileName(image.FileName);
                string path = Path.Combine(Server.MapPath("~/UploadedFiles"), fn);
                image.SaveAs(path);
                Image imgObj = new Image();
                imgObj.idBook = idBook;
                imgObj.Url = "/UploadedFiles/" + fn;
                takaDB.Images.Add(imgObj);
                takaDB.SaveChanges();
                return imgObj;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DelteImage(int idImage)
        {
            takaDB.Images.Remove(takaDB.Images.Where(x => x.ID.Equals(idImage)).First());
            takaDB.SaveChanges();
        }

        public Book EditBook(int ID,
            IEnumerable<int> images_delete,
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
            string Description,
            int[] idImage,
            int[] indexImage,
            HttpServerUtilityBase Server
            )
        {
            if (images_delete != null)
                takaDB.Images.RemoveRange(takaDB.Images.Where(x => images_delete.Contains(x.ID)));
            Book book = takaDB.Books.Where(x => x.ID == ID).First();
            book.Title = Title;
            book.Price = int.Parse(string.Join("", Price.Split('.').ToArray()));
            book.idCategory = idCategory;
            book.idAuthor = idAuthor;
            book.idPublisher = idPublisher;
            book.idLanguage = idLanguage;
            book.idType = idType;
            book.Page = Page;
            book.Date = Date;
            book.Quantity = Quantity;
            book.Description = Description;
            takaDB.SaveChanges();
            if (Images != null && Images.Count() > 0)
            {
                foreach (var image in Images)
                {
                    try
                    {
                        if (image != null)
                        {
                            UploadImage(book.ID, image, Server);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            if (idImage != null && idImage.Length > 0)
                for (int i = 0; i < idImage.Length; i++)
                {
                    var idImg = idImage[i];
                    book.Images.Where(x => x.ID.Equals(idImg)).First().order = indexImage[i];
                }
            takaDB.SaveChanges();
            return book;
        }

        public void ChangeQuantity(int idCart, int quantity)
        {
            takaDB.Carts.Where(x => x.ID == idCart).First().Quantity = quantity;
            takaDB.SaveChanges();
        }


        public int ChangePassword(int idUser, string oldPassword, string password, string rePassword)
        {
            User user = takaDB.Users.Where(x => x.ID.Equals(idUser)).First();
            if (!HelperFunctions.sha256(oldPassword).Equals(user.Password))
            {
                return -1;
            }

            if (!password.Equals(rePassword))
            {
                return -2;
            }
            user.Password = HelperFunctions.sha256(password);
            takaDB.SaveChanges();
            return 1;
        }

        public List<BillItem> GetBillItems(int[] ids)
        {

            List<BillItem> billItems = new List<BillItem>();


            foreach (var id in ids)
            {

                BillItem billItem = new BillItem();
                Cart cart = takaDB.Carts.Where(x => x.ID == id).ToList().First();
                billItem.id = cart.ID;
                billItem.price = (int)cart.Book.Price;
                billItem.quantity = (int)cart.Quantity;
                billItem.bookName = cart.Book.Title;
                billItems.Add(billItem);
            }


            return billItems;
        }

        public List<Address> GetListAddressByUserId(int idUser)
        {
            List<Address> addresses = new List<Address>();
            addresses = takaDB.Addresses.Where(x => x.idUser == idUser).ToList();
            return addresses;
        }

        public Address GetAddressByIdAddress(int? idAddress)
        {
            return takaDB.Addresses.Where(x => x.ID == idAddress).First();
        }

        public void AddAddress(int idUser, string name, string phone, string address)
        {
            Address adr = new Address();
            adr.idUser = idUser;
            adr.Name = name;
            adr.Phone = phone;
            adr.Address1 = address;
            takaDB.Addresses.Add(adr);
            takaDB.SaveChanges();
        }

        public void EditAddress(int idAddress, int idUser, string name, string phone, string address)
        {
            Address adr = takaDB.Addresses.Where(x => x.ID == idAddress).First();
            if (adr == null)
                return;
            adr.Name = name;
            adr.Phone = phone;
            adr.Address1 = address;
            adr.idUser = idUser;
            takaDB.SaveChanges();
        }

        public void DeleteAddress(int idAddress)
        {
            try
            {
                Address address = takaDB.Addresses.Where(x => x.ID == idAddress).First();
                takaDB.Addresses.Remove(address);
                takaDB.SaveChanges();
            }
            catch (Exception)
            {

            }
        }

        public void CheckOut(int[] id_cart, int id_address, int totalPrice, string shipper, int idUser, string fullName, string phone, string address, string message, int shipFee)
        {

            Order order = new Order();
            order.CreateDate = DateTime.Now;
            order.idUser = idUser;
            if (id_address != -1)
            {
                string clientName = takaDB.Addresses.Where(x => x.ID == id_address).First().Name;

                order.IDAddress = id_address;
                order.ClientName = clientName;
            }
            else
            {
                Address newAdress = new Address();
                newAdress.idUser = idUser;
                newAdress.Name = fullName;
                newAdress.Phone = phone;
                newAdress.Address1 = address;
                takaDB.Addresses.Add(newAdress);
                takaDB.SaveChanges();
                order.IDAddress = takaDB.Addresses.Where(x => x.Address1 == address).First().ID;
                order.ClientName = fullName;
            }
            order.TotalPrice = totalPrice;
            order.OrderStatus = 0;
            order.Shipper = shipper;
            order.Notes = message;
            order.ShipFee = shipFee;
            takaDB.Orders.Add(order);
            takaDB.SaveChanges();
            foreach (var idItem in id_cart)
            {
                OrderDetail orderDetail = new OrderDetail();
                Cart cartItem = takaDB.Carts.Where(x => x.ID == idItem).First();
                orderDetail.idOrder = takaDB.Orders.OrderByDescending(x => x.ID).First().ID;
                orderDetail.idBook = (int)cartItem.idBook;
                orderDetail.Quantity = (int)cartItem.Quantity;
                takaDB.OrderDetails.Add(orderDetail);
                takaDB.Carts.Remove(cartItem);
            }
            takaDB.SaveChanges();
        }

        public List<Order> GetProcessingOrders(int id = -1)
        {
            List<Order> orders = id.Equals(-1) ?
                takaDB.Orders.Where(x => x.OrderStatus == 0).ToList()
                : takaDB.Orders.Where(x => x.idUser == id && x.OrderStatus == 0).ToList();
            orders = orders.OrderByDescending(x => x.CreateDate).ToList();
            return orders;
        }
        public List<Order> GetDoneOrders(int id = -1)
        {
            List<Order> orders = id.Equals(-1) ?
                takaDB.Orders.Where(x => x.OrderStatus == 1).ToList()
                : takaDB.Orders.Where(x => x.idUser == id && x.OrderStatus == 1).ToList();
            orders = orders.OrderByDescending(x => x.CreateDate).ToList();
            return orders;
        }

    }
}