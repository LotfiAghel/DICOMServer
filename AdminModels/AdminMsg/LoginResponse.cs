using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Newtonsoft.Json;

using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
namespace AdminMsg
{
   
}

public class RunConsensusRequest
{
    public List<string> SegFiles { get; set; }
    public List<string> CtFiles { get; set; }
    public string OutputFile { get; set; }
}

[Delimiter(",")]
[UpdateAccess(AdminUserRole.SUPER_USER)]
[CultureInfo("InvariantCulture")]
public class CreateUserRow : AIdMapper<string>
{
    [Name("userName")]
    public string userName { get => id; set => id = value; }


    [Name("pass")]
    public string pass { get; set; }


     

}

 

public class GetUsers
{
        
   
    public Range<DateTime> range { get; set; }
        

}

public class GetUsersResponse
{
    
    public int number { get; set; }
    public int numberWithEmail { get; set; }
}
public class AddUserWithEmail
{
        
    public string email { get; set; }
    
    public string pass { get; set; }
    public bool disableZarinPalBuying { get; set; }
    public Lang lang { get; set; }        

}
public class CheckDbResponse
{
    public int userCount { get; set; }
    public int ResponseCount { get; set; }
    public decimal? ResponseSum { get; set; }
    public int serviceBuys { get; set; }
    public decimal serviceBuySums { get; set; }
}

public enum ColRange{
    DAY=1,
    DAY3=2,
    DAY7=3,
    DAY30=4,
    Year=10
}

[PersianLabel("اانخاب بازه")]
public class ServiceBuyReq 
{
    public Range<DateTime> range{ get; set; }
    public ColRange collapseRange { get; set; }
    
}

public class ServiceBuyRow:IIdMapper<DateTime>
{

    public DateTime key{ get; set; }

    
    public decimal all{ get; set; }
    public decimal paye{ get; set; }
    public decimal toefl{ get; set; }
    public decimal revise{ get; set; }
    
    public decimal ielts{ get; set; }
    public decimal gre { get; set; }
    
    
    public decimal service1M { get; set; }
    public decimal service3M { get; set; }
    public decimal service6M { get; set; }
    public decimal service12M { get; set; }

    public object getId()
    {
        return id;
    }

    [JsonIgnore]
    public DateTime id { get=>key; set=> key=value; }
    public ChangeEventList onChanges { get; set; }
    
}
public class ServiceBuyGRow:IIdMapper<string>
{
    public decimal all { get; set; }
    public DateTime key{ get; set; }

    
    public object getId()
    {
        return id;
    }

    [JsonIgnore]
    public string id { get=>key.ToString(); set=> key=DateTime.UtcNow; }
    public ChangeEventList onChanges { get; set; }
    
    public decimal ServiceGroup2 { get; set; }
    public decimal ServiceGroup14 { get; set; }
    public decimal ServiceGroup20 { get; set; }
    public decimal ServiceGroup25 { get; set; }
    
}
public class ServiceBuyResponse 
{
    
    public ServiceBuyGRow data2 { get; set; }
    public decimal sum { get; set; }

    [ChartShow(nameof(ServiceBuyRow.key))]
    public List<ServiceBuyRow> data { get; set; }
    
    
}


public class UniqUserPerLearnRow:IIdMapper<DateTime>
{
    public int withEmail { get; set; }

    //public int gre{ get; set; }
    //public int pte { get; set; }
    public DateTime key { get; set; }
    public int toefl{ get; set; }
    public int ielts { get; set; }
    
    public int gre { get; set; }
    
    public int pte { get; set; }
    public object getId()
    {
        return key;
    }

    [JsonIgnore]
    public DateTime id { get=>key; set=> key=value; }
    public ChangeEventList onChanges { get; set; }
}

public class UniqUserPerLearnRow2:IIdMapper<DateTime>
{
    public DateTime key { get; set; }
    public int toefl{ get; set; }
    public int Ielts { get; set; }
    public int gre { get; set; }
    
    public int pte { get; set; }
    public object getId()
    {
        return key;
    }

    [JsonIgnore]
    public DateTime id { get=>key; set=> key=DateTime.UtcNow; }
    public ChangeEventList onChanges { get; set; }
}

public class UniqUserPerLearnResponse
{
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data3 { get; set; }


    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data { get; set; }
    
    
    
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data2 { get; set; }
}

public class STReviseResponseRow:IIdMapper<DateTime>
{
    //public int gre{ get; set; }
    //public int pte { get; set; }
    public DateTime key { get; set; }
    public int all{ get; set; }
    
    public int SpeakingAdjustment { get; set; }
    public int ResponseAdjust { get; set; }
    public int IeltsWritingAdjust { get; set; }
    public int IeltsSpeakingAdjustment { get; set; }
    public int ToeflWritingAdjust { get; set; }
    public object getId()
    {
        return key;
    }

    
    
    [JsonIgnore]
    public DateTime id { get=>key; set=> key=DateTime.UtcNow; }
    public ChangeEventList onChanges { get; set; }

}
public class STReviseResponse
{
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data3 { get; set; }


    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<STReviseResponseRow> data { get; set; }
    
    
    
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data2 { get; set; }
}


public class STFrResponse
{
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<UniqUserPerLearnRow> data3 { get; set; }

    public int all { get; set; }
    [ChartShow(nameof(UniqUserPerLearnRow.id))]
    public List<STReviseResponseRow> data { get; set; }
    
    
    
    
}

[Delimiter(",")]
[CultureInfo("InvariantCulture")]
public class LietnerCsvRow: AIdMapper<string>
{
    [Name("word")]
    public string word { get => id; set => id=value; }

    [Name("meaning_fa")]
    public string meanFa { get; set; }


    [Name("lesson")]
    public string lesson { get; set; }



}
public  class LietnerMap : ClassMap<LietnerCsvRow>
{
    public LietnerMap()
    {
            

        Map(m => m.word).Name("word");
        Map(m => m.meanFa).Name("meaning_fa");

    }
}
public class LeitnerWordPackData 
{
    [GridShow]
    [CsvInput]
    public List<LietnerCsvRow> rows { get; set; }

        
}
