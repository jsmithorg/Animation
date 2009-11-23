using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Browser;
using System.Threading;

namespace JSmith.Animation
{
    public class Tween<T> : TweenBase, ITween
    {
        #region Fields / Properties

        //the default frames per second we will use to drive our timer
        protected const double DEFAULT_FPS = 60;

        //the timer thread
        //protected DispatcherTimer timer;
        protected Storyboard timer;

        //our timer interval
        private TimeSpan _interval;

        private double _fps;
        public double FPS
        {
            get { return _fps; }
            set
            {
                if (_fps == value || IsTweening)
                    return;

                if (value <= 0)
                    throw new FrameRateException("The framerate \"" + value + "/fps\" is invalid. Framerate must be a number greater than zero.");

                _fps = value;

                _interval = TimeSpan.FromSeconds(1 / _fps);

            }//end set

        }//end property

        public new T Target
        {
            get { return (T)base.Target; }
            set { base.Target = value; }

        }//end property

        //the property of our target object that we will tween
        //public virtual List<PropertyInfo> Properties { get; set; }
        public TweenPropertyCollection Properties { get; set; }

        private Dictionary<string, PropertyInfo> _propertyInfoDictionary;

        //the starting value for our target object's property
        public double StartValue { get; set; }

        //the ending value for our target object's property
        public double EndValue { get; set; }

        public bool IsTimerDestroyed { get; set; }

        public bool IsTweening { get; set; }

        public IEasingFunction EasingFunction { get; set; }

        protected DateTime startTime;

        #endregion

        #region Constructor

        public Tween()
        {
            FPS = DEFAULT_FPS;

            //Properties = new List<PropertyInfo>();
            Properties = new TweenPropertyCollection();

        }//end constructor

        /*public Tween(T target, string property, double endValue, Duration duration, IEasingFunction easingFunction)
        {
            Type type = Target.GetType();

            PropertyInfo propertyInfo = type.GetProperty(property);
            if (propertyInfo == null)
                throw new TweenPropertyDoesNotExistException("The tween property \"" + property + "\" could not be found on object \"" + type.Name + ".\"");

            StartValue = (double)propertyInfo.GetValue(target, null);

        }//end constructor
        */

        public Tween(T target, DependencyProperty property, double startValue, double endValue, Duration duration) : this(target, property, startValue, endValue, duration, new LinearEase()) { }

        public Tween(T target, DependencyProperty property, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : this()
        {
            Target = target;
            /*Type type = Target.GetType();

            PropertyInfo propertyInfo = type.GetProperty(property);
            if(propertyInfo == null)
                throw new TweenPropertyDoesNotExistException("The tween property \"" + property + "\" could not be found on object \"" + type.Name + ".\"");
            
            Properties.Add(propertyInfo);
            */
            Properties.Add(property);

            EasingFunction = easingFunction;

            //EaseType = easeType;
            StartValue = startValue;
            EndValue = endValue;
            Duration = duration;

        }//end constructor

        public Tween(T target, string property, double startValue, double endValue, Duration duration) : this(target, property, startValue, endValue, duration, new LinearEase()) { }

        public Tween(T target, string property, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : this()
        {
            Target = target;
            /*Type type = Target.GetType();

            PropertyInfo propertyInfo = type.GetProperty(property);
            if(propertyInfo == null)
                throw new TweenPropertyDoesNotExistException("The tween property \"" + property + "\" could not be found on object \"" + type.Name + ".\"");
            
            Properties.Add(propertyInfo);
            */
            Properties.Add(property);

            EasingFunction = easingFunction;
            
            //EaseType = easeType;
            StartValue = startValue;
            EndValue = endValue;
            Duration = duration;

        }//end constructor

        public Tween(T target, TweenPropertyCollection properties, double startValue, double endValue, Duration duration) : this(target, properties, startValue, endValue, duration, new LinearEase()) { }

        public Tween(T target, TweenPropertyCollection properties, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : this()
        {
            Target = target;
            /*Type type = Target.GetType();

            PropertyInfo propertyInfo = type.GetProperty(property);
            if(propertyInfo == null)
                throw new TweenPropertyDoesNotExistException("The tween property \"" + property + "\" could not be found on object \"" + type.Name + ".\"");
            
            Properties.Add(propertyInfo);
            */
            //Properties.Add(property);
            Properties = properties;

            EasingFunction = easingFunction;

            //EaseType = easeType;
            StartValue = startValue;
            EndValue = endValue;
            Duration = duration;

        }//end constructor

        #endregion

        #region ITween Methods

        public override void Begin()
        {
            if (timer != null)
                DestroyTimer();

            IsTweening = true;
            IsTimerDestroyed = false;
            
            Storyboard sb = new Storyboard();
            sb.Duration = BeginTime;
            sb.Completed += new EventHandler(sb_Completed);
            sb.Begin();

        }//end method

        private void sb_Completed(object sender, EventArgs e)
        {
            ((Storyboard)sender).Completed -= new EventHandler(sb_Completed);

            CreateTimer();

            startTime = DateTime.Now;

            timer.Begin();

        }//end method

        public override void Pause()
        {
            DestroyTimer();
        }

        public override void Resume()
        {
            CreateTimer();

            timer.Begin();
        }

        public override void Seek(TimeSpan offset)
        {
            throw new NotImplementedException();
        }

        public override void Seek(double percentage)
        {
            throw new NotImplementedException();
        }

        public override void Rewind()
        {
            //iteration = 0;
            startTime = DateTime.Now;

        }//end method

        public override void Stop()
        {
            //don't try to destroy again, or face
            //a null reference exception
            if (IsTimerDestroyed)
                return;

            //timer.Stop();
            //timer.Pause();
            //IsTweening = false;

            DestroyTimer();

            base.Stop();

        }//end method

        public override void Reverse()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            OnTick();

        }//end method

        protected virtual void OnTick()
        {
            double newValue = GetNewValue();

            for (int i = 0; i < Properties.Count; i++)
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
                catch(TweenException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("prop: " + (Properties[i] == null) + ", ex: " + ex);

                    throw new TweenException("Property \"" + p.GetType().Name + "\" could not be set for object \"" + Target.GetType().Name + "\".", ex);

                }//end try

            }//end for

            CheckTweenComplete();

        }//end method

        #region Utilities

        protected double GetNewValue()
        {
            double timeDiff = startTime.Subtract(DateTime.Now).TotalMilliseconds;
            double percentage = -timeDiff / Duration.TimeSpan.TotalMilliseconds;

            double newPercentage = EasingFunction.Ease(percentage);
            //double difference = GetDifference(StartValue, EndValue);

            double newValue = StartValue + (GetDifference(StartValue, EndValue) * newPercentage);

            if (EndValue > StartValue && newValue > EndValue)
                newValue = EndValue;

            if (EndValue < StartValue && newValue < EndValue)
                newValue = EndValue;

            return newValue;

        }//end method

        /*protected double GetNewValue()
        {
            double timeDiff = startTime.Subtract(DateTime.Now).TotalMilliseconds;
            double percentage = -timeDiff / Duration.TimeSpan.TotalMilliseconds;

            double newPercentage = EasingFunction.Ease(percentage);
            return (EndValue * newPercentage) + StartValue;

        }//end method
        */

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

        protected void SetProperty(string property, double value)
        {
            PropertyInfo pi;

            if (_propertyInfoDictionary == null)
                _propertyInfoDictionary = new Dictionary<string, PropertyInfo>();

            if (_propertyInfoDictionary.ContainsKey(property))
            {
                pi = _propertyInfoDictionary[property];
            }
            else
            {
                Type t = Target.GetType();
                pi = t.GetProperty(property);
                if (pi == null)
                    throw new TweenPropertyDoesNotExistException("The tween property \"" + property + "\" could not be found on object \"" + t.Name + ".\"");

                _propertyInfoDictionary.Add(property, pi);

            }//end if

            pi.SetValue(Target, value, null);

        }//end method

        protected void SetProperty(DependencyProperty property, double value)
        {
            FrameworkElement element = Target as FrameworkElement;
            if (element == null)
                throw new TweenTargetException("The target \"" + Target.GetType().Name + "\" must derive from System.Windows.FrameworkElement to set DependencyProperty \"" + property.GetType().Name + "\".");

            element.SetValue(property, value);

        }//end method

        protected void CheckTweenComplete()
        {
            double timeDiff = startTime.Subtract(DateTime.Now).TotalMilliseconds;
            double percentage = -timeDiff / Duration.TimeSpan.TotalMilliseconds;

            //stop the timer if we've reached the end
            if (percentage >= 1)
            {
                DestroyTimer();

                OnComplete();
            }
            else
            {
                OnProgress();

                //storyboard version only
                timer.Begin();

            }//end if

        }//end method

        protected void CreateTimer()
        {
            timer = new Storyboard();
            timer.Duration = _interval;//new Duration(new TimeSpan(0, 0, 0, 0, _interval));
            timer.Completed += new EventHandler(timer_Tick);

        }//end method

        protected void DestroyTimer()
        {
            //don't try to destroy again, or face
            //a null reference exception
            if (IsTimerDestroyed || timer == null)
                return;

            //Stop();

            //timer.Tick -= new EventHandler(timer_Tick);
            timer.Stop();
            timer.Completed -= new EventHandler(timer_Tick);
            timer = null;

            IsTimerDestroyed = true;
            IsTweening = false;

        }//end method

        #endregion

    }//end class

    public class Tween : Tween<object>
    {
        public Tween() : base() { }

        public Tween(object target, DependencyProperty property, double startValue, double endValue, Duration duration) : base(target, property, startValue, endValue, duration) { }

        public Tween(object target, DependencyProperty property, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : base(target, property, startValue, endValue, duration, easingFunction) { }

        public Tween(object target, string property, double startValue, double endValue, Duration duration) : base(target, property, startValue, endValue, duration) { }

        public Tween(object target, string property, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : base(target, property, startValue, endValue, duration, easingFunction) { }

        public Tween(object target, TweenPropertyCollection properties, double startValue, double endValue, Duration duration) : base(target, properties, startValue, endValue, duration) { }

        public Tween(object target, TweenPropertyCollection properties, double startValue, double endValue, Duration duration, IEasingFunction easingFunction) : base(target, properties, startValue, endValue, duration, easingFunction) { }

    }//end class

}//end namespace