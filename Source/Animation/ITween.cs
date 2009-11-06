using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JSmith.Animation
{
    public interface ITween
    {
        #region Events

        event TweenEventHandler Started;
        event TweenEventHandler Stopped;
        event TweenEventHandler Progress;
        event TweenEventHandler Paused;
        event TweenEventHandler Resumed;
        event TweenEventHandler Complete;
        
        #endregion 

        #region Properties

        bool IsReversed { get; }
        bool AutoReverse { get; set; }
        TimeSpan BeginTime { get; set; }
        Duration Duration { get; set; }
        RepeatBehavior RepeatBehavior { get; set; }
        object Target { get; set; }
        
        #endregion

        #region Methods

        void Begin();
        void Pause();
        void Resume();
        void Seek(TimeSpan offset);
        void Seek(double percentage);
        void Rewind();
        void Stop();
        void Reverse();

        #endregion

    }//end interface

}//end namespace