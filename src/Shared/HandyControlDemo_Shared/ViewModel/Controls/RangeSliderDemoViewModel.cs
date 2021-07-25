using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Controls;
using HandyControl.Data;

namespace HandyControlDemo.ViewModel
{
    public class RangeSliderDemoViewModel : ViewModelBase
    {
        public RangeSliderDemoViewModel()
        {
            //CommandManager.RegisterClassCommandBinding(typeof(RangeSlider), new CommandBinding(DelyValueChangedCmd)); 
            //CommandManager.RegisterClassCommandBinding(typeof(CheckBox), new CommandBinding(CheckBoxDelyValueChangedCmd)); 
        }

        public RelayCommand<RangeSlider> DelyValueChangedCmd => new(DelyValueChanged);
        public RelayCommand<CheckBox> CheckBoxDelyValueChangedCmd => new(CheckBox_DelyValueChanged);
        public RelayCommand<RadioButton> RadioButtonDelyValueChangedCmd => new(RadioButton_DelyValueChanged);
         
        public ObservableCollection<FilterItem> FilterItems { get; set; } = new ObservableCollection<FilterItem>()
        {
            new FilterItem { Name = "test1", GroupName = "AAnn", IsChecked = true },
            new FilterItem { Name = "test2", GroupName = "AAnn", IsChecked = false }
        };
         
        private void DelyValueChanged(RangeSlider sender)
        {
            var oldValue = (DoubleRange) sender.OldValue;
            var newValue = (DoubleRange) sender.NewValue;

            MessageBox.Show($"Name:{sender.Name}, IsCommandModel,  OldValue: [{oldValue.Start}, {oldValue.End}], NewValue:[{newValue.Start}, {newValue.End}]");
        }

        private void CheckBox_DelyValueChanged(CheckBox sender)
        {
            var newValue = (bool) sender.NewValue;

            MessageBox.Show($"Name:{sender.Name}, IsCommandModel,  NewValue:[{newValue}]");
        }

        private void RadioButton_DelyValueChanged(RadioButton sender)
        {
            var newValue = (bool) sender.NewValue;

            MessageBox.Show($"Name:{sender.Name}, IsCommandModel,  NewValue:[{newValue}]");
        }

        private void Rs_DelyValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<bool> e)
        {
            MessageBox.Show($"OldValue: {e.OldValue}, NewValue:{e.NewValue}");
        }
    }

    public class FilterItem
    {
        public string Name { get; set; }
        public string GroupName { get; set; }
        public bool IsChecked { get; set; }
    }
}
