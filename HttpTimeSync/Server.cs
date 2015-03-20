using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpTimeSync
{
    public class Server
    {
        public Uri Url { get; set; }
        public DateTime RequestStartDateTime { get; set; }
        public DateTime RequestFinishDateTime { get; set; }
        public DateTime ResponseDateTime { get; set; }

        public TimeSpan Duration
        {
            get
            {
               return RequestFinishDateTime.Subtract(RequestStartDateTime);
            }
        }

        public TimeSpan Offset
        {
            get { return ResponseDateTime.Subtract(RequestStartDateTime); //RequestFinishDateTime.Subtract(new TimeSpan(Duration.Ticks / 2)));
            }
        }

        public bool IsOutlier { get; set; }

        public override string ToString()
        {
            return String.Format("[{6}] {0}\n    Request {1} Took {4}ms\n    Response {3} Offset={5}", Url, RequestStartDateTime.TimeOfDay, RequestFinishDateTime.TimeOfDay, ResponseDateTime.TimeOfDay, Duration.TotalMilliseconds, Offset, IsOutlier ? " " : "V");
        }
    }
}
