using System;

namespace Bb.Brokers
{

    public static class RabbitClock
    {

        static RabbitClock()
        {
            GetNow = () => DateTimeOffset.Now;
        }

        public static Func<DateTimeOffset> GetNow { get; set; }

    }

}