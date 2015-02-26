using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aphelion
{
    // TO DO: Make this easier on memory, maybe use structs instead of tuples
    public static class Timer
    {
        // Timer Format: initiation time, identifier, interval, recurrences, callback
        private static List<Tuple<TimeSpan, string, float, int?, Action>> timers = new List<Tuple<TimeSpan, string, float, int?, Action>>();
        private static TimeSpan currentTime;

        public static void Create(string identifier, float interval, int? recurrences, Action callback) // 0 recurrences = infinite recurrences
        {
            if (recurrences == null || recurrences == 0)
            {
                timers.Add(Tuple.Create(currentTime, identifier, interval, (int?)null, callback));
            }
            else
            {
                timers.Add(Tuple.Create(currentTime, identifier, interval, (int?)recurrences, callback));
            }
        }

        public static void Remove(string identifier)
        {
            timers.RemoveAll((timer) => { return timer.Item2 == identifier; });
        }

        public static void Update(GameTime time)
        {
            if (currentTime == null) // Remove all timers created before the manager is initialized
            {
                currentTime = TimeSpan.FromMilliseconds(time.TotalGameTime.Milliseconds);
                timers.RemoveAll((timer) => { return timer.Item1 == null; });
            }

            currentTime = TimeSpan.FromMilliseconds(time.TotalGameTime.TotalMilliseconds);

            for (int i = 0; i < timers.Count; i++)
			{
                double difference = (timers[i].Item1.TotalMilliseconds + timers[i].Item3) - time.TotalGameTime.TotalMilliseconds;

                if (difference <= 0)
                {
                    timers[i].Item5();

                    if (timers[i].Item4 == null)
                    {
                        timers[i] = Tuple.Create(TimeSpan.FromMilliseconds(currentTime.TotalMilliseconds - difference), timers[i].Item2, timers[i].Item3, (int?)null, timers[i].Item5);
                    }
                    else if (timers[i].Item4 - 1 > 0)
                    {
                        timers[i] = Tuple.Create(TimeSpan.FromMilliseconds(currentTime.TotalMilliseconds - difference), timers[i].Item2, timers[i].Item3, timers[i].Item4 - 1, timers[i].Item5);
                    }
                    else
                    {
                        timers.RemoveAt(i);
                    }
                }
            }
        }
    }
}
