
using Models;
using Models.Exams;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ClientMsgs
{

    public class AddWord
    {
        public string word { get; set; }
        public List<LeitnerType> litnerTypes;
        public List<ForeignKey2<LeitnerWordTag, Guid>> tags { get; set; }


        public string? note { get; set; }

        public string? sampleText { get; set; }
        public string? sampleAudioPath { get; set; }

        public ForeignKey2<Section,int>? sectionId { get; set; }
        public NpgsqlRange<int>? sampleTimingMilli { get; set; }
    }
    public class CreateTag
    {
        [MaxLength(20)]
        public string title { get; set; }
        [MaxLength(20)]
        public string color { get; set; }
    }


    public class UpdateCard
    {
        public string word { get; set; }
        public List<LeitnerType> litnerTypes;
        public List<ForeignKey2<LeitnerWordTag, Guid>> tags { get; set; }

        public string? note { get; set; }
        public string? sampleText { get; set; }
        public string? sampleAudioPath { get; set; }

        public ForeignKey2<Section, int>? sectionId { get; set; }
        public NpgsqlRange<int>? sampleTimingMilli { get; set; }
    }
    
    public class AddWordResponse: ObjectContainerRespone<UserLeitnerCard>
    {
        public string extraMsg { get; set; }

    }
    
    public class GetWordsOfDay
    {
        public List<ForeignKey2<LeitnerWordTag, Guid>> tags { get; set; }
        [Obsolete("Method1 is deprecated, please use leitnerType instead.", false)]
        public LeitnerType ?litnerType { get => leitnerType; set => leitnerType = value; }
        public LeitnerType ?leitnerType { get; set; }
    }

    public class RemoveWords : GetWordsOfDay
    {
        public bool removeLearned { get; set; } = false;
    }



}