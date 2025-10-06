using Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Newtonsoft.Json.Linq;

namespace AdminMsgs{

    public class ChoiceAttibute:Attribute{
        public string funcName;
        public ChoiceAttibute(string funcName){
            this.funcName=funcName;
        }
    }
    public class CreateUser{
        public string phoneNumber{get;set;}
        public string name{get;set;}
        public string chanel{get;set;}="";
        //public ForeignKey<Market> marketId{get;set;}
        //public TimeSpan Subciption 
        public int subciptionMounth{get;set;} 
    }

    public class CreateUserResponse:ClientMsgs.BooleanResponse{
        public User user{get;set;}
        public Profile profile{get;set;}
    }

    

}