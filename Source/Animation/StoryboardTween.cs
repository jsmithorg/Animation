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
    public class StoryboardTween : TweenBase
    {
        #region Fields / Properties 

        private Storyboard _storyboard;

        public new bool AutoReverse
        {
            get { return _storyboard.AutoReverse; }
            set { _storyboard.AutoReverse = value; }

        }//end property

        public new TimeSpan BeginTime
        {
            get { return _storyboard.BeginTime.HasValue ? _storyboard.BeginTime.Value : TimeSpan.Zero; }
            set { _storyboard.BeginTime = value; }

        }//end property

        public new Duration Duration
        {
            get { return _storyboard.Duration; }
            set { _storyboard.Duration = value; }

        }//end property

        public new RepeatBehavior RepeatBehavior
        {
            get { return _storyboard.RepeatBehavior; }
            set { _storyboard.RepeatBehavior = value; }

        }//end property

        #endregion

        public StoryboardTween(Storyboard storyboard)
        {
            _storyboard = storyboard;
            
        }//end constructor

        #region TweenBase Overrides 

        public sealed override void Begin()
        {
            base.OnStarted();

            //_storyboard.Progress += new EventHandler(_storyboard_Progress);
            _storyboard.Completed += new EventHandler(_storyboard_Completed);
            _storyboard.Begin();

        }//end method

        public sealed override void Pause()
        {
            _storyboard.Pause();

            base.OnPaused();

        }//end method

        public sealed override void Resume()
        {
            _storyboard.Resume();

            base.OnResumed();

        }//end method

        public sealed override void Rewind()
        {
            _storyboard.Stop();

        }//end method

        public sealed override void Seek(TimeSpan offset)
        {
            _storyboard.Seek(offset);

        }//end method

        public sealed override void Seek(double percentage)
        {
            double ms = _storyboard.Duration.TimeSpan.TotalMilliseconds;
            TimeSpan offset = TimeSpan.FromMilliseconds(ms * percentage);

            _storyboard.Seek(offset);

        }//end method

        public sealed override void Stop()
        {
            _storyboard.Stop();

            RemoveStoryboardHandlers(_storyboard);

            base.OnStopped();

        }//end method

        public sealed override void Reverse()
        {
            base.Reverse();

            _storyboard.AutoReverse = !_storyboard.AutoReverse;

        }//end method

        #endregion

        private void _storyboard_Progress(object sender, EventArgs e)
        {
            base.OnProgress();

        }//end method

        private void _storyboard_Completed(object sender, EventArgs e)
        {
            RemoveStoryboardHandlers((Storyboard)sender);

            base.OnComplete();

        }//end method

        #region Utilities

        private void RemoveStoryboardHandlers(Storyboard storyboard)
        {
            //storyboard.Progress -= new EventHandler(_storyboard_Progress);
            storyboard.Completed -= new EventHandler(_storyboard_Completed);

        }//end method

        #endregion

    }//end class

}//end namespace