using EnglishToefl.Models;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class UserSubscription : Id4Entity
    {

        public int userId { get; set; }


        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }



    }

    public class FreemiumContent : Id4Entity
    {
        public int number { get; set; } //uniq session 100 bar mitune bashe
        public int numberTv { get; set; } //uniq session 100 bar mitune bashe

    }
   



}