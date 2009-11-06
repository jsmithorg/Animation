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
    public abstract class CompoundTween : TweenBase, ITween
    {
        public List<ITween> Tweens { get; set; }

        public CompoundTween()
        {
            Tweens = new List<ITween>();

        }//end constructor

        public virtual void AddTween(ITween tween)
        {
            Tweens.Add(tween);

        }//end method

        public virtual void RemoveTween(ITween tween)
        {
            Tweens.Remove(tween);

        }//end method

    }//end class

}//end namespace