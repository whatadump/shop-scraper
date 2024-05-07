namespace ShopScraper.Infrastructure.Entities;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    [Column("real_name")]
    [Required]
    [DefaultValue("")]
    public string RealName { get; set; }
}