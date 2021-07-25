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
using HandyControl.Interactivity;

namespace HandyControl.Controls
{
    public class CheckBox : System.Windows.Controls.CheckBox, IDelyControl
    {
        #region Dely

        public CheckBox()
        { 
        }

        #region DependencyProperty

        public static readonly DependencyProperty IsCommandProperty = DependencyProperty.Register("IsCommand", typeof(bool), typeof(CheckBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty IsDelyProperty = DependencyProperty.Register("IsDely", typeof(bool), typeof(CheckBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty DelyIntervalProperty = DependencyProperty.Register("DelyInterval", typeof(double), typeof(CheckBox), new PropertyMetadata(1000d));

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(CheckBox), new PropertyMetadata(Guid.NewGuid().ToString()));

        public static readonly DependencyProperty DelyCommandProperty = DependencyProperty.Register("DelyCommand", typeof(ICommand), typeof(CheckBox), new PropertyMetadata(default(ICommand)));

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
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(CheckBox));

        public event RoutedPropertyChangedEventHandler<bool> DelyValueChanged
        {
            add => AddHandler(DelyValueChangedEvent, value);
            remove => RemoveHandler(DelyValueChangedEvent, value);
        }

        public ICommand DelyCommand
        {
            get => (ICommand) GetValue(DelyCommandProperty);
            set => SetValue(DelyCommandProperty, value);
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
                        this.OnDelyValueChanged(); 
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
                    if (this.NewValue == this.OldValue)
                        return;

                    this.InitTimer().Start();
                }
            }
            finally
            {
                base.OnUnchecked(e);
            }
        }

        private void OnDelyValueChanged()
        {
            Dispatcher.Invoke(
               new Action(
                   delegate
                   {
                       if (this.IsCommand)
                       {
                           this.CommandParameter ??= this;
                           this.CommandTarget ??= this;

                           switch (DelyCommand)
                           {
                               case null:
                                   return;
                               case RoutedCommand command:
                                   command.Execute(CommandParameter, CommandTarget);
                                   break;
                               default:
                                   DelyCommand.Execute(CommandParameter);
                                   break;
                           }
                       }
                       else
                           this.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>((bool) this.NewValue, (bool) this.NewValue) { RoutedEvent = DelyValueChangedEvent });
                   }
               )
           , DispatcherPriority.Send); 
        }

        #endregion

        #endregion
    }
}
