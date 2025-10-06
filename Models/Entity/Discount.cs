using Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Models
{


    [GeneratedController]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class Discount:Entity{
        [MultiSelect]
        public List<ForeignKey<BazaarSku>> subscriptionMasterSkuIds { get; set; }


        [JsonIgnore]
        public ICollection<SingleUseGiftCode> singles{ get; set;}
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Discount>().HasMany(s => s.singles).WithOne(s => s.discount);
        }
    }
    

    
    
    public abstract class DiscountCode:Entity{
        public string code{get;set;}

        [ForeignKeyAttr(typeof(Discount))]
        [ForeignKey(nameof(discount))]
        public int discountId{get;set;}

        [JsonIgnore]
        public Discount discount {get;set;}
    }

    [GeneratedController]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class DiscountFilter:DiscountCode{
        
    }

    [GeneratedController]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class SingleUseGiftCode:DiscountCode{
        [ForeignKeyAttr(typeof(User))]
        [ForeignKey(nameof(user))]
        public int ?userId{get;set;}

        [JsonIgnore]
        public User user{get;set;}

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SingleUseGiftCode>()
                .HasIndex(u => u.code)
                .IsUnique();

            /* builder.Entity<BazaarPurchase>()
            .HasIndex(u => u.requestId)
            .IsUnique();/**/
        }
        
    }


    public abstract class DiscountRequest { }


    public class GiftCodeDiscount : DiscountRequest
    {
        public string code { get; set; }
    }
    public class SingleUseGiftCodeRequest : DiscountRequest
    {
        public string code { get; set; }
    }
    public class UlteraCustomDiscount : DiscountRequest
    {
        public string code { get; set; }
        public string csrf { get; set; }
    }



   
    
}