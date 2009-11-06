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
    public class TweenEventArgs : EventArgs
    {
        public ITween Tween { get; internal set; }
        public object Target { get; internal set; }
        public double Progress { get; internal set; }

        public TweenEventArgs(ITween tween, object target, double progress) : base()
        {
            Tween = tween;
            Target = target;
            Progress = progress;

        }//end constructor

        public TweenEventArgs(ITween tween, object target) : base()
        {
            Progress = double.NaN;

        }//end constructor

    }//end class

}//end namespace