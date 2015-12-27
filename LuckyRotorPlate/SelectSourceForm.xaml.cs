using System.Windows;

namespace LuckyRotorPlate
{
    /// <summary>
    /// SeleteSourceForm.xaml 的交互逻辑
    /// </summary>
    public partial class SeleteSourceForm : Window
    {
        public SeleteSourceForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            int count;
            if (rbFromNumbers.IsChecked.Value && int.TryParse(txtcount.Text, out count) && count > 1
                || !rbFromNumbers.IsChecked.Value)
            {
                this.Close();
            }
        }
    }
}
