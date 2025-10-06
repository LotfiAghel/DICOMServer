using Models;
using System;
using System.Collections.Generic;
namespace ClientMsgs
{



    public class DiscountCheck
    {
        public int serviceId { get; set; }
        public string Code { get; set; }
    }
    public class DiscountCheckRes:BooleanResponse
    {
        public Guid discountId { get; set; }
        public decimal price { get; set; }
    }
    public class PayRequest
    {
        public int serviceId { get; set; }
        public Guid ?discountId { get; set; }
        public string  cameBackUrl { get; set; }
            
    }
    public class ToDoItemViewModel
    {
        public Rial Amount;
        public string TerminalId;
        public string ResNum;
        public string ResNum1;
        public string ResNum2;
        public string ResNum3;
        public string RedirectURL;
        public string CellNumber;


    }


    public class ShopItem
    {
        public ForeignKey<BazaarSku> skuId { get; set; }
        public string code { get; set; }
        public DiscountRequest discountData { get; set; }
    }


    public class SkuWithDiscountData : ShopItem
    {

    }




    public class CheckDiscountResponse : BooleanResponse
    {
        public List<SkuWithDiscountData> shopItems { get; set; }


        public bool buyingNotNeeded{get;set;}
        public User user{get;set;}
        public string dialogText{get;set;}

    }
    public abstract class StartRequest : ShopItem
    {
        //public ForeignKey<BazaarSku> skuId { get; set; }
        //public DiscountRequest discountData { get; set; }
    }
    public class SamanStartReuest : StartRequest
    {
        public string redirectURL { get; set; }



    }

    public class BazarStartReuest : StartRequest
    {

    }

    public class EntityResponse<T> : ClientMsgs.BooleanResponse
    {
        public T entity { get; set; }
    }


    public class SamanStartRequestResponse : EntityResponse<SamanRequest>
    {
        public CheckDiscountResponse discountResponse { get; set; }
    }
     

    public class SamanResponse
    {
        public int MID { get; set; }
        public string State { get; set; }
        public SamanRequest.Status Status { get; set; }
        public string RRN { get; set; }
        public string RefNum { get; set; }
        public int ResNum { get; set; }
        public int TerminalID { get; set; }
        public string TraceNo { get; set; }
        public Rial Amount { get; set; }
        public string SecurePan { get; set; }
    }


    public class BazarResponse{
        public bool error{get;set;}
        public bool error_description{get;set;}
        public bool consumptionState{get;set;}
        public bool purchaseState{get;set;}
        public string developerPayload{get;set;}
        public long purchaseTime{get;set;}
        public DateTime purchaseTime2{get{
             DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds( purchaseTime ).ToUniversalTime();
            return dateTime;
            
        }}
    }
    public class BazaarVerifyResponse:BooleanResponse
    {
        
        public Models.User user { get; set; }

    }

}
