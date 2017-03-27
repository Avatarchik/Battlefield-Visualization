
using System;

namespace BattlefieldVisualization
{

    public class DisEvent
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        public DisEvent(DateTime timestamp, string message)
        {
            this.Timestamp = timestamp;
            this.Message = message;
        }
    }

}