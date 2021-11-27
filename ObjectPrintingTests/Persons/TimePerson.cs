using System;

namespace ObjectPrintingTests.Persons
{
    public class TimePerson
    {
        public DateTime BirthDay { get; set; }
        public TimeSpan WeekWorkTime { get; set; }

        public TimePerson(DateTime birthDay, TimeSpan weekWorkTime)
        {
            BirthDay = birthDay;
            WeekWorkTime = weekWorkTime;
        }
    }
}