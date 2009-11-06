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
    public delegate void TweenEventHandler(object sender, TweenEventArgs e);

    public abstract class TweenBase : ITween
    {
        #region ITween Events

        private readonly object _lock = new object();

        private TweenEventHandler _started;
        public event TweenEventHandler Started
        {
            add
            {
                lock (_lock)
                    _started += value;

            }//end add

            remove
            {
                lock (_lock)
                    _started -= value;

            }//end remove

        }//end event

        private TweenEventHandler _stopped;
        public event TweenEventHandler Stopped
        {
            add
            {
                lock (_lock)
                    _stopped += value;

            }//end add

            remove
            {
                lock (_lock)
                    _stopped -= value;

            }//end remove

        }//end event

        private TweenEventHandler _progress;
        public event TweenEventHandler Progress
        {
            add
            {
                lock (_lock)
                    _progress += value;

            }//end add

            remove
            {
                lock (_lock)
                    _progress -= value;

            }//end remove

        }//end event

        private TweenEventHandler _paused;
        public event TweenEventHandler Paused
        {
            add
            {
                lock (_lock)
                    _paused += value;

            }//end add

            remove
            {
                lock (_lock)
                    _paused -= value;

            }//end remove

        }//end event

        private TweenEventHandler _resumed;
        public event TweenEventHandler Resumed
        {
            add
            {
                lock (_lock)
                    _resumed += value;

            }//end add

            remove
            {
                lock (_lock)
                    _resumed -= value;

            }//end remove

        }//end event

        private TweenEventHandler _complete;
        public event TweenEventHandler Complete
        {
            add
            {
                lock (_lock)
                    _complete += value;

            }//end add

            remove
            {
                lock (_lock)
                    _complete -= value;

            }//end remove

        }//end event

        #endregion

        #region ITween Properties

        public bool IsReversed { get; internal set; }

        public bool AutoReverse { get; set; }

        public TimeSpan BeginTime { get; set; }

        public Duration Duration { get; set; }

        public RepeatBehavior RepeatBehavior { get; set; }

        public object Target { get; set; }

        #endregion

        #region ITween Methods

        public virtual void Begin()
        {
            OnStarted();

        }//end method

        public virtual void Pause()
        {
            OnPaused();

        }//end method

        public virtual void Resume()
        {
            OnResumed();

        }//end method

        public virtual void Stop()
        {
            OnStopped();

        }//end method

        public virtual void Reverse()
        {
            IsReversed = !IsReversed;

        }//end method

        public abstract void Rewind();

        public abstract void Seek(TimeSpan offset);

        public abstract void Seek(double percentage);

        #endregion

        private TweenEventArgs CreateEventArgs()
        {
            double progress = Double.NaN; // current time / total time;

            return new TweenEventArgs(this, Target, progress);

        }//end method

        protected virtual void OnStarted()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _started;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

        protected virtual void OnStopped()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _stopped;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

        protected virtual void OnProgress()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _progress;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

        protected virtual void OnPaused()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _paused;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

        protected virtual void OnResumed()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _resumed;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

        protected virtual void OnComplete()
        {
            TweenEventHandler handler;
            TweenEventArgs args;

            lock (_lock)
            {
                handler = _complete;
                args = CreateEventArgs();

            }//end lock

            if (handler != null)
                handler(this, args);

        }//end method

    }//end class

}//end namespace