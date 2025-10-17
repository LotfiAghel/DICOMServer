using Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

using Microsoft.AspNetCore.Http;


namespace ClientMsgs
{
    
    public class LoginRequest0
    {
        public int marketId { get; set; }
        public string versionName { get; set; }




        public string metrixSessionId { get; set; }
        public string advertisingId { get; set; }
        public string metrixUserId { get; set; }
    }
    public class LoginRequest : LoginRequest0
    {

        public int codeId { get; set; }

        [Required]
        public string phoneNumber { get; set; }

        [Required]
        public int code { get; set; }





    }

    public class TvLoginRequest : LoginRequest0
    {
        [Required]
        [MaxLength(12)]
        public string tvToken { get; set; }

    }
    public class UpdateRequest
    {
        [Required]
        public Guid userId;
        [Required]
        public int marketId { get; set; }
        [Required]
        public string versionName { get; set; }

    }

    public class Login00Request
    {
        [Required]
        public Guid userId;
        [Required]
        public string code { get; set; }
        
    }


    public class CheckUpdateResponse : BooleanResponse
    {
        public enum UpdateState
        {
            NONE = 0,
            UPDATE = 1,
            FORCE_UPDATE = 2
        };

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        //public List<Notice> notices { get; set; }
        public UpdateState updateState { get; set; }
       
    }
    public class LoginResponse : CheckUpdateResponse
    {
        public SessionMakeResult sessionCookieData { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        //public List<Notice> notices { get; set; }
        
        

        
        public Models.Profile profile { get; set; }
        public Models.User user { get; set; }
    }
    public class GetUserResponse{
        public Models.Profile profile { get; set; }
        public Models.User user { get; set; }
        public DateTime branchTime { get; set; }
        public DateTime neoTime { get; set; }
        public DateTime leitnerTime { get; set; }
        public UserExtraData extraData { get; set; }
        public DateTime ieltsTime { get; set; }
        public DateTime greTime { get; set; }

        //public Models.UserExtraData userExtraData{get;set;}
    }
    public class SmsRequest
    {
        [Required]
        //[MaxLength(10)]
        //[MinLength(10)]
        public string phoneNumber { get; set; }
    }
    public class SmsRequestResponse : BooleanResponse
    {
        public string phoneNumber { get; set; }
        public int codeId { get; set; }
        public int secendsToExpire { get; set; }
        public int secendsToNextSmsAvailabe { get; set; }
        public string extraMsg { get; set; }

    }
    public class OTPLoginReq
    {

    }
    public class OTPLoginRes : BooleanResponse
    {
        public User user { get; set; }
    }

    
    public class LoginWithOTP
    {
        
        public string PhoneNumber { get; set; }
        public string OTP { get; set; }
        public Platform platfrom { get; set; }

        
    }
    public class LoginWithGmail
    {
        
        public string jwt { get; set; }
        public Platform platfrom { get; set; }

        
    }
    public class Login3rdParty
    {

        public string userName { get; set; }
        public string pass { get; set; }


    }
    public class ChangeDisplayName
    {
        [MaxLength(30)]
        [MinLength(3)]
        public string name { get; set; }
        


    }
    
    
    public enum AggregateTime{
        Hours=0,
        Days=1,
        Weekly=2,
        Mountly=3,

    }

    public class TimeSpaningRow0
    {
        public DateTime key { get; set; }
        public double all { get; set; }
    }

    public class TimeSpaningRow{
        public DateTime key { get; set;}
        public double reading { get; set;}
        public double Listening { get; set;}

        public double speaking { get;  set; }
        public double writing { get;  set; }
        public double leitner { get;  set; }
    }
    public class QuestionTypeScore
    {
        public int examId { get; set; }
        public decimal? sum { get; set; }
        public decimal? all { get; set; }
        public decimal? examAll { get; set; } 
    }
    
    public class GraphData<T>{
        public AggregateTime aggregateTime { get; set; }
        public  Range<DateTime> range { get; set; }
        
        public List<T> data{get;set;}
    }


}

