using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Models.Notifications;
using Newtonsoft.Json;

namespace Models
{
    public class DependesyContainer
    {
        public static IServiceCollection serviceCollection { get; set; }
        public static IServiceProvider IServiceProvider { get;  set; }
    }
    public enum Market
    {
        None = 0,
        BAZAAR = 1,
        GOOGLE_PLAY = 2,
        PWA=3,
        IOS_TEST_FLIGHT=4,
    }
    public enum MoneyGateWay
    {
        UNKNOWN_OR_OLDDATA = 0,
        FREE=1,
        ZRINPAL=2,
        ATM=3,
        CRYPTO=4 ,       
        SAMAN = 5,
        BAZAAR = 6,
       
    }


    public enum UserRole
    {
        BAZZAR = 1,
        SAMAN = 2
    }
    public enum Platform
    {
        NONE = 0,
        WEB = 1,
        MOBILE = 2,
        ADMIN=3,
    }
    public enum PlatformBranch
    {
        NONE = 0,
        WEB_MOC = 1,
        WEB_Mobile = 2,
        FlutterAndroid = 3,
        FlutterIos = 4,
        FlutterPWA = 5
    }
    public enum Gender
    {
        UNKNOWN = 0,
        MAN = 1,
        WOMAN = 2
    }



    [ShowClassHirarci]
    [GeneratedController]
    [BigTable]
    [DefultSort<SortByCreateAtDescending<VerificationCode>>]
    [SelectAccess(AdminUserRole.SUPPORT)]
    [ViewAccess(AdminUserRole.SUPPORT)]
    public class VerificationCode : Id4Entity
    {

        public string phoneNumber { get; set; }
        public int code { get; set; }
        public int marketId { get; set; } = 0;
        public string metrixSessionId { get; set; }
        public string advertisingId { get; set; }
        public string metrixUserId { get; set; }

    }



    
    [Models.GeneratedControllerAttribute]
    [BigTable]
    [ViewAccess(AdminUserRole.SUPPORT)]
    [InsertAccess(AdminUserRole.SUPPORT)]
    [UpdateAccess(AdminUserRole.SUPPORT)]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class UserExtraData : IdMapper<Guid>
    {
        public List< Guid> readedNews { get; set; }
        public List<Guid> closedPopup { get; set; }
        
        public string language { get; set; }
        
        [Column(TypeName = "jsonb")]
        public Dictionary<string,string> settings { get; set; }

        public IQueryable<User> Revises(IServiceProvider Services)
        {
            return null;
        }
        

    }


    [Models.GeneratedControllerAttribute]
    [BigTable]
    public class SupportNote : IdMapper<Guid>
    {
        public User user { get; set; }
        public string PhoneNum { get; set; }
        public string Note { get; set; }
    }

    
    [Models.GeneratedControllerAttribute]
    [BigTable]
    public class Profile : IdMapper<Guid>
    {


        public string name { get; set; }
        
        public int level { get; set; }
        public string disease { get; set; }
        public string avatar { get; set; }


    }


    







}
