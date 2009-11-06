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
    public class MarginTween : Tween
    {
        private Thickness _startMargin;
        private Thickness _endMargin;

        public MarginTween(FrameworkElement target, Thickness startValue, Thickness endValue, Duration duration) : this(target, startValue, endValue, duration, new LinearEase()) { }

        public MarginTween(FrameworkElement target, Thickness startValue, Thickness endValue, Duration duration, IEasingFunction easingFunction)
        {
            _startMargin = startValue;
            _endMargin = endValue;

            Target = target;
            Duration = duration;
            EasingFunction = easingFunction;

        }//end constructor

        protected override void OnTick()
        {
            ((FrameworkElement)Target).Margin = GetNewValue();

            /*for (int i = 0; i < Properties.Count; i++)
            {
                TweenProperty p = Properties[i];

                try
                {
                    //Properties[i].SetValue(Target, newValue, null);

                    if (p.Property is string)
                        SetProperty((string)p.Property, newValue);
                    else if (p.Property is DependencyProperty)
                        SetProperty((DependencyProperty)p.Property, newValue);
                }
                catch (TweenException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("prop: " + (Properties[i] == null) + ", ex: " + ex);

                    throw new TweenException("Property \"" + p.GetType().Name + "\" could not be set for object \"" + Target.GetType().Name + "\".", ex);

                }//end try

            }//end for
            */

            CheckTweenComplete();

        }//end method

        protected new Thickness GetNewValue()
        {
            double timeDiff = startTime.Subtract(DateTime.Now).TotalMilliseconds;
            double percentage = -timeDiff / Duration.TimeSpan.TotalMilliseconds;

            double newPercentage = EasingFunction.Ease(percentage);
            return new Thickness
            {
                Left = (_endMargin.Left * newPercentage) + _startMargin.Left,
                Right = (_endMargin.Right * newPercentage) + _startMargin.Right,
                Top = (_endMargin.Top * newPercentage) + _startMargin.Top,
                Bottom = (_endMargin.Bottom * newPercentage) + _startMargin.Bottom
            };

        }//end method

    }//end class

}//end namespace