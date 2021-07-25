using System;
using System.Collections.Generic;
using System.Text;
using HandyControl.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;

namespace HandyControl.Controls
{
    public class RadioButton : System.Windows.Controls.RadioButton, IDelyControl
    {
        #region Dely

        public RadioButton()
        {
            CommandBindings.Add(new CommandBinding(DelyChangedCmd, (s, e) => DelyChangedCmd?.Execute(this)));
        }

        #region DependencyProperty

        public static readonly DependencyProperty IsCommandProperty = DependencyProperty.Register("IsCommand", typeof(bool), typeof(RadioButton), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty IsDelyProperty = DependencyProperty.Register("IsDely", typeof(bool), typeof(RadioButton), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty DelyIntervalProperty = DependencyProperty.Register("DelyInterval", typeof(double), typeof(RadioButton), new PropertyMetadata(1000d));

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(RadioButton), new PropertyMetadata(Guid.NewGuid().ToString()));

        public static readonly DependencyProperty DelyChangedCmdProperty = DependencyProperty.Register("DelyChangedCmd", typeof(ICommand), typeof(RadioButton), new PropertyMetadata(new RoutedCommand(nameof(DelyChangedCmd), typeof(RadioButton))));

        #endregion

        #region Property

        public bool IsDely
        {
            get => (bool) GetValue(IsDelyProperty);
            set => SetValue(IsDelyProperty, value);
        }

        public bool IsCommand
        {
            get => (bool) GetValue(IsCommandProperty);
            set => SetValue(IsCommandProperty, value);
        }

        public double DelyInterval
        {
            get => (double) GetValue(DelyIntervalProperty);
            set => SetValue(DelyIntervalProperty, value);
        }

        public string GroupName
        {
            get => (string) GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public bool IsFirstChange { get; private set; } = true;

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public bool IsProcessing { get; private set; }

        #endregion

        #region Events

        public static readonly RoutedEvent DelyValueChangedEvent = EventManager.RegisterRoutedEvent("DelyValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DoubleRange>), typeof(RadioButton));

        public event RoutedPropertyChangedEventHandler<DoubleRange> DelyValueChanged
        {
            add => AddHandler(DelyValueChangedEvent, value);
            remove => RemoveHandler(DelyValueChangedEvent, value);
        }

        public ICommand DelyChangedCmd
        {
            get => GetValue(DelyChangedCmdProperty) as ICommand;
            set => SetValue(DelyChangedCmdProperty, value);
        }

        #endregion

        #region Methods

        public void DelyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;

            try
            {
                if (!this.IsProcessing)
                {
                    this.IsProcessing = true;

                    try
                    {
                        Dispatcher.Invoke(
                           new Action(
                               delegate
                               {
                                   if (this.IsCommand)
                                       this.DelyChangedCmd?.Execute(this);
                                   else
                                       this.RaiseEvent(new RoutedPropertyChangedEventArgs<DoubleRange>((DoubleRange) this.OldValue, (DoubleRange) this.NewValue) { RoutedEvent = DelyValueChangedEvent });
                               }
                           )
                       , DispatcherPriority.Send);

                        this.OldValue = this.NewValue;
                    }
                    finally
                    {
                        this.IsProcessing = false;
                    }
                }
            }
            finally
            {
                timer?.Stop();
            }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            try
            {
                this.NewValue = true;

                if (this.IsDely)
                {
                    if (this.IsFirstChange)
                    {
                        this.IsFirstChange = false;
                        return;
                    }

                    if (this.NewValue == this.OldValue)
                        return;

                    this.InitTimer().Start();
                }
            }
            finally
            {
                base.OnChecked(e);
            } 
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            try
            {
                this.NewValue = false;

                if (this.IsDely)
                {
                    if (this.IsFirstChange)
                    {
                        this.IsFirstChange = false;
                        return;
                    } 
                }
            }
            finally
            {
                base.OnUnchecked(e);
            }
        }

        #endregion

        #endregion
    }
}
