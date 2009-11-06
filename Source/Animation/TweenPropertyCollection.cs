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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JSmith.Animation
{
    public class TweenPropertyCollection : List<TweenProperty>
    {
        public void Add(string property)
        {
            TweenProperty tp = new TweenProperty(property);
            Add(tp);

        }//end method

        public void Add(DependencyProperty property)
        {
            TweenProperty tp = new TweenProperty(property);
            Add(tp);

        }//end method

    }//end class

}//end namespace