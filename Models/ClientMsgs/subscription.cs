using System;
using System.Collections.Generic;
namespace ClientMsgs
{
    public class AddActinsToExamSession
    {
        public ForeignKey2<Models.ExamPartSession, Guid> examSessionId { get; set; }
        public List<Models.ExamAction> actions { get; set; }
        public int _version { get; set; }
    }

    public class AddActinsToExamSessionResponse
    {
        public ForeignKey2<Models.ExamPartSession, Guid> examSessionId { get; set; }
        public int version { get; set; }
        public enum SendPolicy
        {
            SEND_LIVE = 1,
            SEND_BUFFERED = 2
        }
        public SendPolicy sendPolicy { get; set; } = SendPolicy.SEND_LIVE;
        public int logSendIntervalTime { get; set; } = 10;
    }





}