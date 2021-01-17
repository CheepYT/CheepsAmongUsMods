using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CheepsAmongUsApi.API
{
    public class CustomStopwatch : Stopwatch
    {
        public CustomStopwatch()
        {
            TimeSpanOffset = TimeSpan.Zero;
        }

        public CustomStopwatch(bool start) : this()
        {
            if (start)
                Start();
        }

        public TimeSpan TimeSpanOffset { get; set; }

        public new TimeSpan Elapsed
        {
            get
            {
                return base.Elapsed.Add(TimeSpanOffset);
            }
        }
    }
}