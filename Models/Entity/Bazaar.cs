
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Models
{
    [GeneratedController]
    public abstract class PurchasbleItem : Id4Entity
    {
        [JsonIgnore]
        public ICollection<BazaarSku> skus { get; set; }



    }





    public class BuySubscribe : PurchasbleItem
    {
        public int subscriptionMounts { get; set; }
    }



    public class SKUAttr : Attribute
    {
    }

    [GeneratedController]
    [ViewAccess(AdminUserRole.SUPPORT,AdminUserRole.DATA_ENTRY,AdminUserRole.DEVELOPER)]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class SubscriptionMasterSku : Id4Entity
    {
        public string title { get; set; }
        public int subscriptionMounts { get; set; }

        [SKUAttr]
        [CustomIgnoreTag(CustomIgnoreTag.Kind.CLIENT)]
        public string sku { get; set; }


        [RialAttr]
        [DocAttr(nameof(buyablePrice))]
        public Rial buyablePrice { get; set; }



        [RialAttr]
        [DocAttr(nameof(unrealPrice))]
        public Rial unrealPrice { get; set; }






        public string perMountPriceText { get; set; }


        public Rial perMountPrice => subscriptionMounts!=0? buyablePrice / subscriptionMounts:0;
        public int primitiveDiscount => unrealPrice != 0 ? (int)(100 - buyablePrice * 100 / unrealPrice) : 0;


    }

    [GeneratedController]
    public class AbstractSKU : Id4Entity
    {


        [ForeignKey(nameof(master))]
        [ForeignKeyAttr(typeof(SubscriptionMasterSku))]
        public int masterId { get; set; }

        [JsonIgnore]
        public SubscriptionMasterSku master { get; set; }


        [SKUAttr]
        
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please, use letters in the first name. Digits are not allowed.")]
        public string sku { get; set; }


        [RialAttr]
        [DocAttr(nameof(buyablePrice))]
        public Rial buyablePrice { get; set; }





        public bool limited { get; set; } = false;



        public string title { get; set; }



        
        //public int subscriptionMounts { get; set; }





        public async Task<int> percentOfMaster()
        {

            var master = await getMaster();
            return master.unrealPrice != 0 ? (int)(buyablePrice * 100 / master.unrealPrice) : 100;

            /*set {
                buyablePrice = value*getMaster().unrealPrice/100;
            }/**/
        }



        public async Task<SubscriptionMasterSku> getMaster()
        {
            if (master == null)
                master = IAssetManager.instance.getManager<SubscriptionMasterSku,int>().get0(masterId);
            return master;
        }
        public SubscriptionMasterSku getMaster0()
        {

            if (master == null){
                if(IAssetManager.instance==null)
                    return null;
                master = IAssetManager.instance.getManager<SubscriptionMasterSku,int>().get0(masterId);
            }
            return master;
        }

        //public Rial perMountPrice() => buyablePrice / subscriptionMounts;


        
        public async Task<int> _extraEventDiscount()
        {
            var master = await getMaster();
            long x = master.primitiveDiscount;
            long y = master.buyablePrice != 0 ? 100 - buyablePrice * 100 / master.unrealPrice - x : 0;
            return (int)y;

        }

        [CustomIgnoreTag(CustomIgnoreTag.Kind.ADMIN,CustomIgnoreTag.Kind.DB_ANALYSIS,CustomIgnoreTag.Kind.CLIENT)]
        public int extraEventDiscount{
            get{
                var master = getMaster0();
                if(master==null)
                    return -1;
                long x = master.primitiveDiscount;
                long y = master.buyablePrice != 0 ? 100 - buyablePrice * 100 / master.unrealPrice - x : 0;
                return (int)y;
            }
            /*set{
                var master = getMaster0();
                long x = master.primitiveDiscount;
                buyablePrice=master.buyablePrice*(value+x)/100;
            }/**/

        }

    }

    /*open class AbstractSKU  : EntityId() {

    @SerializedName("sku")
    @Expose
    var sku: String? = ""

    @SerializedName("rial")
    @Expose
    var rial: Int? = 0

    @SerializedName("limited")
    @Expose
    var limited: Boolean? = false

    @SerializedName("percentOfMaster")
    @Expose
    var percentOfMaster: Int = 0
}/**/
    [GeneratedController]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class BazaarSku : AbstractSKU
    {





        [JsonIgnore]
        public ICollection<SamanPurchase> samanPurchases{get;set;}


        [JsonIgnore]
        public ICollection<BazaarPurchase> bazarPurchases{get;set;}



        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
           

            modelBuilder.Entity<BazaarSku>()
              .HasMany<SamanPurchase>(u => u.samanPurchases).WithOne(s=>s.skuObject);

             modelBuilder.Entity<BazaarSku>()
              .HasMany<BazaarPurchase>(u => u.bazarPurchases).WithOne(s=>s.skuObject);

           


        }




       
    }
    [ShowClassHirarci]
    [Table("sku_2_item")]
    public class Sku2Item : Id4Entity
    {

        [ForeignKey(nameof(sku))]
        public int skuId { get; set; }

        [CustomIgnoreTag(CustomIgnoreTag.Kind.CLIENT)]
        public BazaarSku sku { get; set; }


        [ForeignKey(nameof(item))]
        public int itemId { get; set; }

        [CustomIgnoreTag(CustomIgnoreTag.Kind.CLIENT)]
        public PurchasbleItem item { get; set; }

    }
    public abstract class IBazarTransaction:Id4Entity{
        [ForeignKeyAttr(typeof(BazaarSku))]
        [ForeignKey(nameof(sku))]
        public int skuId { get; set; }

        [JsonIgnore]
        public BazaarSku sku { get; set; }

        public string developerPayload { get; set; }

        [ForeignKey(nameof(user))]
        public int userId { get; set; }
        public User user { get; set; }

        [PhoneNumAttr]
        public string phoneNumber { get; set; }
    }

    [GeneratedController]
    [AdminWriteBan]
    [BigTable]
    public class BazaarInAppPurchaseRequest : IBazarTransaction
    {
        public string pass { get; set; }    
    }
    [GeneratedController]
    [AdminWriteBan]
    [BigTable]
    public class BazaarInAppPurchase : IBazarTransaction
    {
       
        public string purchaseToken { get; set; }
        public DateTime purchaseTime { get; set; }
        
    }
    

    [GeneratedController]
    public class BazaarAPIToken : Id4Entity
    {
        public enum Type
        {
            NONE = 0,
            Bearer = 1,
            sec = 100
        }
        public Type TokenType { get; set; }//TODO this may enum
        public DateTime ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string AccessToken { get; set; }
    }


}