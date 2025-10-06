
using Models;
using System;
using System.Collections.Generic;
namespace ClientMsgs
{
    public class MockExmingSerach
    {
        public Range<DateTime> timeRange { get; set; }
        public string userName { get; set; }
    }
    public class MockExmingSerachResultItem
    {
        public User user { get; set; }
        public ExamSession session { get; set; }
        public IEnumerable<ExamPartSession> parts { get; set; }
    }




}