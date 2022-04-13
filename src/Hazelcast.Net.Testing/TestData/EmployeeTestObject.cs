using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hazelcast.Tests.TestObjects
{
    public class EmployeeTestObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Salary { get; set; }
        private DateTime _startAt;
        public DateTime Started
        {
            get => _startAt.ToUniversalTime();

            set
            {
                _startAt = value;
                StartedAtTimeStamp = _startAt.Ticks;
            }
        }
        public long StartedAtTimeStamp { get; set; }
        public char Type { get; set; }
    }
}
