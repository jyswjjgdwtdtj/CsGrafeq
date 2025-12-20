using System.Timers;
using TTimer = System.Timers.Timer;

namespace CsGrafeq;

/// <summary>
///     其实不应该叫Clock
///     当距离上一次更新达到一定时间 则调用指定事件
///     用于鼠标滚轮操作的绘制
/// </summary>
public class Clock
{
    private readonly TTimer Timer = new();
    private bool Running;

    public Clock(uint interval = 50)
    {
        Timer.Interval = interval;
        Timer.Elapsed += Timer_Elapsed;
        Timer.Start();
    }

    public event Action? OnElapsed;

    public void Touch()
    {
        Running = true;
    }

    public void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Do();
    }

    private void Do()
    {
        if (Running)
        {
            Console.WriteLine("Do");
            Running = false;
            OnElapsed?.Invoke();
        }
    }

    public void Cancel()
    {
        Running = false;
    }
}