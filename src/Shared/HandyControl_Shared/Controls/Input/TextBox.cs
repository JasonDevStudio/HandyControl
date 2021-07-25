using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HandyControl.Data;
using HandyControl.Interactivity;
using HandyControl.Tools;

namespace HandyControl.Controls
{
    public class TextBox : System.Windows.Controls.TextBox, IDataInput, IDelyControl
    {
        #region Dely

        #region DependencyProperty

        public static readonly DependencyProperty IsCommandProperty = DependencyProperty.Register("IsCommand", typeof(bool), typeof(TextBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty IsDelyProperty = DependencyProperty.Register("IsDely", typeof(bool), typeof(TextBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public static readonly DependencyProperty DelyIntervalProperty = DependencyProperty.Register("DelyInterval", typeof(double), typeof(TextBox), new PropertyMetadata(1000d));

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(TextBox), new PropertyMetadata(Guid.NewGuid().ToString()));

        public static readonly DependencyProperty DelyChangedCmdProperty = DependencyProperty.Register("DelyChangedCmd", typeof(ICommand), typeof(TextBox), new PropertyMetadata(new RoutedCommand(nameof(DelyChangedCmd), typeof(TextBox))));

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
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DoubleRange>), typeof(TextBox));

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

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            try
            {
                this.NewValue = Text;

                if (this.IsDely)
                {
                    if (this.IsFirstChange)
                    {
                        this.IsFirstChange = false;
                        return;
                    }

                    this.InitTimer().Start();
                }
            }
            finally
            {
                base.OnTextChanged(e);
                VerifyData();
            }
        }

        #endregion

        #endregion

        public TextBox()
        {
            CommandBindings.Add(new CommandBinding(ControlCommands.Clear, (s, e) => SetCurrentValue(TextProperty, "")));
            CommandBindings.Add(new CommandBinding(DelyChangedCmd, (s, e) => DelyChangedCmd?.Execute(this)));
        }

        public Func<string, OperationResult<bool>> VerifyFunc { get; set; }

        /// <summary>
        ///     数据是否错误
        /// </summary>
        public static readonly DependencyProperty IsErrorProperty = DependencyProperty.Register(
            "IsError", typeof(bool), typeof(TextBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public bool IsError
        {
            get => (bool) GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, ValueBoxes.BooleanBox(value));
        }

        /// <summary>
        ///     错误提示
        /// </summary>
        public static readonly DependencyProperty ErrorStrProperty = DependencyProperty.Register(
            "ErrorStr", typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

        public string ErrorStr
        {
            get => (string) GetValue(ErrorStrProperty);
            set => SetValue(ErrorStrProperty, value);
        }

        /// <summary>
        ///     文本类型
        /// </summary>
        public static readonly DependencyProperty TextTypeProperty = DependencyProperty.Register(
            "TextType", typeof(TextType), typeof(TextBox), new PropertyMetadata(default(TextType)));

        public TextType TextType
        {
            get => (TextType) GetValue(TextTypeProperty);
            set => SetValue(TextTypeProperty, value);
        }

        /// <summary>
        ///     是否显示清除按钮
        /// </summary>
        public static readonly DependencyProperty ShowClearButtonProperty = DependencyProperty.Register(
            "ShowClearButton", typeof(bool), typeof(TextBox), new PropertyMetadata(ValueBoxes.FalseBox));

        public bool ShowClearButton
        {
            get => (bool) GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, ValueBoxes.BooleanBox(value));
        }

        public virtual bool VerifyData()
        {
            OperationResult<bool> result;

            if (VerifyFunc != null)
            {
                result = VerifyFunc.Invoke(Text);
            }
            else
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    if (TextType != TextType.Common)
                    {
                        var regexPattern = InfoElement.GetRegexPattern(this);
                        result = string.IsNullOrEmpty(regexPattern)
                            ? Text.IsKindOf(TextType)
                                ? OperationResult.Success()
                                : OperationResult.Failed(Properties.Langs.Lang.FormatError)
                            : Text.IsKindOf(regexPattern)
                                ? OperationResult.Success()
                                : OperationResult.Failed(Properties.Langs.Lang.FormatError);
                    }
                    else
                    {
                        result = OperationResult.Success();
                    }
                }
                else if (InfoElement.GetNecessary(this))
                {
                    result = OperationResult.Failed(Properties.Langs.Lang.IsNecessary);
                }
                else
                {
                    result = OperationResult.Success();
                }
            }

            var isError = !result.Data;
            if (isError)
            {
                SetCurrentValue(IsErrorProperty, ValueBoxes.TrueBox);
                SetCurrentValue(ErrorStrProperty, result.Message);
            }
            else
            {
                isError = Validation.GetHasError(this);
                if (isError)
                {
                    SetCurrentValue(ErrorStrProperty, Validation.GetErrors(this)[0].ErrorContent);
                }
            }

            return !isError;
        }
    }
}
