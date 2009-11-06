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
    public class TweenProperty
    {
        public object Property { get; set; }

        public TweenProperty() { }

        public TweenProperty(object property)
        {
            Property = property;

        }//end constructor

    }//end class

}//end namespace