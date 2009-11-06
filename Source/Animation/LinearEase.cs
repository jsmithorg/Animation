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
    internal class LinearEase : EasingFunctionBase
    {
        protected sealed override double EaseInCore(double normalizedTime)
        {
            return normalizedTime;

        }//end method

    }//end class

}//end namespace