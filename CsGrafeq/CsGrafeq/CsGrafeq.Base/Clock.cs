using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Timers;
using TTimer = System.Timers.Timer;

namespace CsGrafeq
{
    public class Clock
    {
        private TTimer Timer=new();
        public event Action? OnElapsed;
        private DateTime LastTime;
        private bool Running = false;
        public Clock(uint interval=50) {
            Timer.Interval=interval;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }
        public void Touch() { 
            LastTime = DateTime.Now;
            Running = true;
        }
        public void Timer_Elapsed(object? sender,ElapsedEventArgs e)
        {
            Do();
        }
        public void Do()
        {
            if (Running && (DateTime.Now - LastTime).TotalMilliseconds > Timer.Interval)
            {
                Running = false;
                OnElapsed?.Invoke();
            }
        }
        public void Cancel()
        {
            Running = false;
        }
    }
}
