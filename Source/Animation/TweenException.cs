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
    public class TweenException : Exception
    {
        public TweenException() : base() { }
        public TweenException(string message) : base(message) { }
        public TweenException(string message, Exception innerException) : base(message, innerException) { }

    }//end class

}//end namespace