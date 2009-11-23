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
        public new Thickness StartValue { get; set; }
        public new Thickness EndValue { get; set; }

        public MarginTween() : base() { }

        public MarginTween(FrameworkElement target, Thickness startValue, Thickness endValue, Duration duration) : this(target, startValue, endValue, duration, new LinearEase()) { }

        public MarginTween(FrameworkElement target, Thickness startValue, Thickness endValue, Duration duration, IEasingFunction easingFunction) : this()
        {
            StartValue = startValue;
            EndValue = endValue;
            
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
            double difference = GetDifference(StartValue.Left, EndValue.Left);

            Thickness newMargin = new Thickness
            {
                Left = (StartValue.Left + (GetDifference(StartValue.Left, EndValue.Left) * newPercentage)),
                Right = (StartValue.Right + (GetDifference(StartValue.Right, EndValue.Right) * newPercentage)),
                Top = (StartValue.Top + (GetDifference(StartValue.Top, EndValue.Top) * newPercentage)),
                Bottom = (StartValue.Bottom + (GetDifference(StartValue.Bottom, EndValue.Bottom) * newPercentage))
            };

            return newMargin;

        }//end method

        /*protected double GetDifference(double startValue, double endValue)
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
        */

    }//end class

}//end namespace