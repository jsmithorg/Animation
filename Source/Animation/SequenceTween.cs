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
    public sealed class SequenceTween : CompoundTween, ITween
    {
        public ITween CurrentTween { get; internal set; }

        private int _currentTweenIndex;
        public int CurrentTweenIndex
        {
            get { return _currentTweenIndex; }
            set
            {
                if (_currentTweenIndex == value || value >= Tweens.Count)
                    return;

                _currentTweenIndex = value;

                CurrentTween = Tweens[_currentTweenIndex];

            }//end set

        }//end property

        public override void Begin()
        {
            base.Begin();

            if (CurrentTween == null && Tweens.Count > 0)
                CurrentTween = Tweens[0];

            Storyboard sb = new Storyboard();
            sb.Duration = BeginTime;
            sb.Completed += new EventHandler(sb_Completed);
            sb.Begin();

        }//end method

        private void sb_Completed(object sender, EventArgs e)
        {
            ((Storyboard)sender).Completed -= new EventHandler(sb_Completed);

            CurrentTweenIndex = 0;

            BeginTween();
        
        }//end method

        private void BeginTween()
        {
            CurrentTween.Progress += new TweenEventHandler(CurrentTween_Progress);
            CurrentTween.Complete += new TweenEventHandler(CurrentTween_Complete);
            
            CurrentTween.Begin();

        }//end method

        private void CurrentTween_Progress(object sender, TweenEventArgs e)
        {
            OnProgress();

        }//end method

        private void CurrentTween_Complete(object sender, TweenEventArgs e)
        {
            base.OnProgress();

            RemoveTweenHandlers(e.Tween);

            if (CurrentTweenIndex == Tweens.Count - 1 )
            {
                OnComplete();

                return;

            }//end if

            CurrentTweenIndex++;

            BeginTween();

        }//end method

        public override void Pause()
        {
            CurrentTween.Pause();

            base.OnPaused();

        }//end method

        public override void Resume()
        {
            CurrentTween.Resume();

            base.OnResumed();

        }//end method

        //right now, only accounts for the current tween.
        //should do some voodoo to determine length of entire
        //sequence
        public override void Seek(TimeSpan offset)
        {
            CurrentTween.Seek(offset);

        }//end method

        public override void Seek(double percentage)
        {
            throw new NotImplementedException();

        }//end method

        public void Seek(int tweenIndex)
        {
            CurrentTweenIndex = tweenIndex;

        }//end method

        public override void Rewind()
        {
            CurrentTweenIndex = 0;

        }//end method

        public override void Stop()
        {
            CurrentTween.Stop();

            CurrentTweenIndex = 0;
            CurrentTween = null;

            base.OnStopped();

        }//end method

        public override void Reverse()
        {
            base.Reverse();

            foreach (ITween tween in Tweens)
                tween.Reverse();

            Tweens.Reverse();

        }//end method

        #region Utitilities

        private void RemoveTweenHandlers(ITween tween)
        {
            System.Diagnostics.Debug.WriteLine("tweeen: " + tween + ", " + (tween == null));

            tween.Progress += new TweenEventHandler(CurrentTween_Progress);
            tween.Complete += new TweenEventHandler(CurrentTween_Complete);
            
        }//end method

        #endregion

    }//end class

}//end namespace