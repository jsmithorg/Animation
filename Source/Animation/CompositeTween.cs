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
using System.Collections.Generic;

namespace JSmith.Animation
{
    public class CompositeTween : CompoundTween
    {
        private int _tweensComplete;

        public sealed override void Begin()
        {
            base.Begin();

            foreach (ITween tween in Tweens)
            {
                tween.Complete += new TweenEventHandler(Tween_Complete);
                tween.BeginTime = BeginTime;
                tween.Begin();

            }//end foreach

            /*Storyboard sb = new Storyboard();
            sb.Duration = BeginTime;
            sb.Completed += new EventHandler(sb_Completed);
            sb.Begin();
            */
            
        }//end method

        private void Tween_Complete(object sender, TweenEventArgs e)
        {
            ((ITween)sender).Complete -= new TweenEventHandler(Tween_Complete);

            _tweensComplete++;

            if (_tweensComplete >= Tweens.Count)
                OnComplete();

        }//end method

        /*private void sb_Completed(object sender, EventArgs e)
        {
            ((Storyboard)sender).Completed -= new EventHandler(sb_Completed);

            foreach (ITween tween in Tweens)
                tween.Begin();

        }//end method
        */

        public sealed override void Pause()
        {
            foreach (ITween tween in Tweens)
                tween.Pause();

            base.Pause();

        }//end method

        public sealed override void Resume()
        {
            foreach (ITween tween in Tweens)
                tween.Resume();

            base.Resume();

        }//end method

        public sealed override void Rewind()
        {
            throw new NotImplementedException();

        }//end method

        public sealed override void Seek(TimeSpan offset)
        {
            foreach (ITween tween in Tweens)
                tween.Seek(offset);
            
        }//end method

        public sealed override void Seek(double percentage)
        {
            foreach (ITween tween in Tweens)
                tween.Seek(percentage);

        }//end method

        public sealed override void Stop()
        {
            foreach (ITween tween in Tweens)
                tween.Stop();

            base.Stop();

        }//end method

        public sealed override void Reverse()
        {
            base.Reverse();

            foreach (ITween tween in Tweens)
                tween.Reverse();

            Tweens.Reverse();

        }//end method

    }//end class

}//end namespace