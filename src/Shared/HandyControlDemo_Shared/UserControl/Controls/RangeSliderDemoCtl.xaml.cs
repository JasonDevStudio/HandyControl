using HandyControl.Controls;

namespace HandyControlDemo.UserControl
{
    public partial class RangeSliderDemoCtl
    {
        public RangeSliderDemoCtl()
        {
            InitializeComponent();
            // this.DataContext = new ViewModel.RangeSliderDemoViewModel();
            
        }

        private void Rs_DelyValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<HandyControl.Data.DoubleRange> e)
        {
            var rs = sender as RangeSlider;
            MessageBox.Show($"Name:{rs.Name}  OldValue: [{e.OldValue.Start}, {e.OldValue.End}], NewValue:[{e.NewValue.Start}, {e.NewValue.End}]");
        }
    }
}
