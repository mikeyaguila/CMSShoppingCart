using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CMSShoppingCart.Models.Data
{
    public class DataContext : DbContext
    {
        public DbSet<PageDTOes> Pages { get; set; }
        public DbSet<SidebarDTOes> Sidebar { get; set; }
        public DbSet<CategoryDtoes> Categories { get; set; }
        public DbSet<ProductDtoes> Products { get; set; }
    }
}