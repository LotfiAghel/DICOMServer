using ClientMsgs;
using System;
using System.Collections.Generic;
using Models;

namespace AdminMsgs
{
    public class ExamMigrateResult: BooleanResponse
    {
        
        public int sectionsPartsCount { get; set; }
        public int questionCounts { get; set; }
    }
    public class MigrateExamsHeaderResult:BooleanResponse
    {
        
        public int sectionsCount { get; set; }
    }
    public class SessionData
    {
        public DateTime lastUse { get; set; }
        public DateTime createAt { get; set; }
        public Guid sessionId { get; set; }
        public Guid password { get; set; }
        public Platform platform { get; set; }
    }

   public class GetSessionsResponse : BooleanResponse
    {

        public List<SessionData> sessions { get; set; }
    }
    public class GetJwtResponse
    {
        public string authToken { get; set; }
    }
}