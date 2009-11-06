using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JSmith.Animation;

namespace ExampleProject
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            DateTime startTime = DateTime.Now;

            Storyboard sb2 = new Storyboard();
            sb2.Completed += (s, e) =>
            {
                time.Text = DateTime.Now.Subtract(startTime).TotalSeconds.ToString();
                sb2.Begin();
            };
            
            Storyboard sb3 = new Storyboard();
            sb3.Completed += (s, e) =>
            {
                time2.Text = DateTime.Now.Subtract(startTime).TotalSeconds.ToString();
                sb3.Begin();
            };

            sb2.Begin();
            sb3.Begin();

            DoubleAnimation da = new DoubleAnimation();
            da.From = 100;
            da.To = 600;
            da.Duration = TimeSpan.FromSeconds(4);
            da.EasingFunction = new ExponentialEase();

            Storyboard sb = new Storyboard();
            sb.Completed += (s, e) => { sb2.Stop(); sb2 = new Storyboard(); };
            sb.Children.Add(da);

            Storyboard.SetTarget(da, rect);
            Storyboard.SetTargetProperty(da, new PropertyPath(Canvas.LeftProperty));

            //sb.Begin();

            TweenPropertyCollection tpc = new TweenPropertyCollection
            {
                Canvas.LeftProperty,
                Canvas.TopProperty
                //"ScaleX",
                //"ScaleY"
            };
            
            ScaleTransform st = new ScaleTransform();
            rect2.RenderTransform = st;
            
            Tween<ScaleTransform> t = new Tween<ScaleTransform>(st, "ScaleX", 0, 2, TimeSpan.FromSeconds(4), new ExponentialEase());
            t.Complete += (s, e) => { sb3.Stop(); sb3 = new Storyboard(); };
            t.Progress += (s, e) => { progress.Text = "progress: " + DateTime.Now.Subtract(startTime).TotalSeconds; };
            //t.Begin();

            progress.Text = "progress";

            Tween myTween = (Tween)Resources["myTween"];
            //myTween.Target = rect;
            System.Diagnostics.Debug.WriteLine("ease: " + expo + ", " + (expo == null));
            System.Diagnostics.Debug.WriteLine("twen: " + myTween +", " + (myTween == null));
            System.Diagnostics.Debug.WriteLine("tween prop: " + myTween.Target + ", " + myTween.EasingFunction + ", " + myTween.Duration.TimeSpan.TotalSeconds + ", " + myTween.StartValue + ", " + myTween.EndValue);

            CompositeTween ct = new CompositeTween();
            ct.AddTween(new StoryboardTween(sb));
            ct.AddTween(t);
            ct.AddTween(new Tween<Rectangle>(rect2, tpc, 100, 600, TimeSpan.FromSeconds(4), new ExponentialEase()));
            //ct.AddTween(myTween);
            ct.BeginTime = TimeSpan.FromSeconds(1);
            ct.Begin();

            
            
        }
    }
}
