﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace taka.Utils
{
    public class C
    {
        public class SESSION
        {
            public static string UserInfo = "UserInfo";
            public static string Cart = "Cart";
        }

        public class TEMPDATA
        {
            public static string Message = "Message";
            public static string RequireLogin = "RequireLogin";
        }

        public class DROPDOWN_SORT
        {
            public static string NEWEST = "Mới nhất";
            public static string OLDEST = "Cũ nhất";
            public static string HIGHEST_PRICE = "Giá cao nhất";
            public static string LOWEST_PRICE = "Giá thấp nhất";
        }
        public static int ID_ADMIN = 4;
        public class DEFAULT_USER_VALUE
        {
            public static string GENDER = "Khác";
            public static string BIRTHDAY = "0000-00-00";
        }
    }
}