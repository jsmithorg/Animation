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

            CheckTweenComplete();

        }//end method

        protected new Thickness GetNewValue()
        {
            double timeDiff = startTime.Subtract(DateTime.Now).TotalMilliseconds;
            double percentage = -timeDiff / Duration.TimeSpan.TotalMilliseconds;

            double newPercentage = EasingFunction.Ease(percentage);
            double difference = GetDifference(_startMargin.Left, _endMargin.Left);

            Thickness newMargin = new Thickness
            {
                Left = (_startMargin.Left + (GetDifference(_startMargin.Left, _endMargin.Left) * newPercentage)),
                Right = (_startMargin.Right + (GetDifference(_startMargin.Right, _endMargin.Right) * newPercentage)),
                Top = (_startMargin.Top + (GetDifference(_startMargin.Top, _endMargin.Top) * newPercentage)),
                Bottom = (_startMargin.Bottom + (GetDifference(_startMargin.Bottom, _endMargin.Bottom) * newPercentage))
            };

            return newMargin;

        }//end method

        protected double GetDifference(double startValue, double endValue)
        {
            double newStartValue = startValue;
            double newEndValue = endValue;

            //if one of the numbers is less than 0, normalize to 0
            if (startValue < 0)
            {
                newStartValue += -startValue;
                newEndValue += -startValue;

            }
            else if (endValue < 0)
            {
                newStartValue += -endValue;
                newEndValue += -endValue;

            }//end if

            return endValue - startValue;

        }//end method

    }//end class

}//end namespace