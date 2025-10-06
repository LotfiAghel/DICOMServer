using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace Models
{

    public class SegmentedEntity : BaseEntity
    {

        public bool published { get; set; } = false;
        public DateTime? publishDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string minVersion { get; set; }
        public string maxVersion { get; set; }



        

        
        [MultiSelect]
        public List<string> branchs { get; set; }


        [Column(TypeName = "jsonb[]")]
        public List<Conndtion> extraConndtion { get; set; } = new List<Conndtion>();



        public bool checkActive(User ud, string version, string branch, DateTime now)
        {

            if (published)
                return false;

            if (branchs != null && !branchs.Contains(branch))
                return false;
            if (publishDate != null && publishDate > now)
                return false;
            if (expiryDate != null && expiryDate < now)
                return false;
            if (maxVersion != null && maxVersion.CompareTo(version) < 0)
                return false;
            if (minVersion != null && minVersion.CompareTo(version) > 0)
                return false;

            return true;

        }


    }
    public class Conndtion
    {
        bool check()
        {
            return true;
        }
        public IQuery<SegmentedEntity> query { get; set; }
    }



    public enum DactiveLevel
    {
        Silent = 0,//nemidunam key
        OnOpenList = 1, // khabar kam mohem
        OnSeenComplete = 2, // khabari ke user bere rush va bekhune
        OnCliam = 3,
        Never = 4 // maslan update kardan hich vaght markesh bardashte nemishe mage inke update kone va khabar hazf mishe
    }

    public enum Lang
    {
        FA=1,
        EN=2
    }
    public class Notice : SegmentedEntity
    {
        public Lang language { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public bool showOnce { get; set; }

        [Column(TypeName = "jsonb[]")]
        public List<ButtonData> buttons { get; set; } = new List<ButtonData>();

    }

    public class ButtonData
    {
        public enum Color
        {
            Normal = 0,
            Recomanded = 2,
            Warning = 2,
            Danger = 3
        };
        public Color color { get; set; } = Color.Normal;
        public string title { get; set; }
        public Behaviour behaviour { get; set; }

    }
    public class Behaviour
    {
        
    }
    public class CloseNotice : Behaviour
    {
    }

    public class OpenLink : Behaviour
    {
        public string url { get; set; }
    }


}