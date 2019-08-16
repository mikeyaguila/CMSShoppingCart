﻿using CMSShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSShoppingCart.Models.ViewModels.Pages
{
    public class SidebarVM
    {
        public SidebarVM()
        {

        }

        public SidebarVM(SidebarDTOes row)
        {
            Id = row.Id;
            Body = row.Body;
        }

        public int Id { get; set; }

        [AllowHtml]
        public string Body { get; set; }
    }
}