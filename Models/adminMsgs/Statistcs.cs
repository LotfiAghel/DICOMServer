using Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Newtonsoft.Json.Linq;

namespace AdminMsgs
{

   
    public class ListResult<T>
    {
        [GridShow]
        public List<T> data { get; set; }
    }
    public class GetOldPurchaseStatsRes
    {
        public TimeRangeRequest req { get; set; }
        public int usersNumber { get; set; }
        public int paiedUsers { get; set; }
        public int paiedUsers2 { get; set; }
        public int purchaseCount { get; set; }
        public Rial purchaseSum { get; set; }
        public Rial newPurchaseSum { get; set; }

        public string paiedUsersQuery { get; set; }
        public string paiedUsersQuery2 { get; set; }
        public string samanPurchasesQuery { get; set; }
        public string newPurchaseSumSumanQueryString { get; set; }




    }
    public class AcuntantReq
    {
        public long fromTimeStamp { get; set; }
        public long toTimeStamp { get; set; }
        public string secretKey { get; set; }

    }




    public class PurchaseStaticsResult
    {


        public class Row
        {
            public Rial amount { get; set; }
            public Rial bazar { get; set; }
            public Rial saman { get; set; }
            public string monthName { get; set; }// farvardin
            public int monthId { get; set; }    // 1-12
        }

        public TimeRangeRequest inp { get; set; }

        public Rial sumOfPardkhat { get; set; }

        [GridShow]
        public List<Row> months { get; set; }

    }


    public class UnTamdidSubscriptionResult
    {
        public TimeRangeRequest inp { get; set; }
        public class Row
        {
            public ForeignKey2<User,Guid> userId { get; set; }
            public string phoneNumber { get; set; }
            public DateTime endDate { get; set; }
            public int monthPaid;
            public Rial sumOfPardkhat { get; set; }
        }

        [GridShow]
        public List<Row> users { get; set; }
        public string queryString { get; set; }
    }

  

}