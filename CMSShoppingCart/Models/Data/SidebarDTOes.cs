using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CMSShoppingCart.Models.Data
{
    [Table("Sidebar")]
    public class SidebarDTOes
    {
        [Key]
        public int Id { get; set; }
        public string Body { get; set; }
    }
}