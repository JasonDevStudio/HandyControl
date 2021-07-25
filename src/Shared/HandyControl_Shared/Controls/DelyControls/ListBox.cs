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
using System.Collections;

namespace HandyControl.Controls
{
    public class ListBox : System.Windows.Controls.ListBox, IDelyControl, ICommandSource
    {
        #region Dely

        public ListBox()
        {
        }

        #region DependencyProperty

        public static readonly DependencyProperty IsCommandProperty = DependencyProperty.Register("IsCommand", typeof(bool), typeof(ListBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty IsDelyProperty = DependencyProperty.Register("IsDely", typeof(bool), typeof(ListBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty DelyIntervalProperty = DependencyProperty.Register("DelyInterval", typeof(double), typeof(ListBox), new PropertyMetadata(1000d));

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(ListBox), new PropertyMetadata(Guid.NewGuid().ToString()));

        public static readonly DependencyProperty DelyCommandProperty = DependencyProperty.Register("DelyCommand", typeof(ICommand), typeof(ListBox), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(ListBox), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(ListBox), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget", typeof(IInputElement), typeof(ListBox), new PropertyMetadata(default(IInputElement)));

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

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public IInputElement CommandTarget
        {
            get => (IInputElement) GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        public bool IsFirstChange { get; private set; } = true;

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public bool IsProcessing { get; private set; }

        #endregion

        #region Events

        public static readonly RoutedEvent DelyValueChangedEvent = EventManager.RegisterRoutedEvent("DelyValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(ListBox));

        public event RoutedPropertyChangedEventHandler<object> DelyValueChanged
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

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsDely)
                {
                    if (this.NewValue == this.OldValue)
                        return;
                     
                    this.NewValue = e.AddedItems;
                    this.OldValue = e.RemovedItems;
                    this.InitTimer().Start();
                }
            }
            finally
            {
                base.OnSelectionChanged(e);
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
                           this.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(this.NewValue, this.NewValue) { RoutedEvent = DelyValueChangedEvent });
                   }
               )
           , DispatcherPriority.Send);
        }

        #endregion

        #endregion
    }
}
