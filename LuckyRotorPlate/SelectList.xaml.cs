using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LuckyRotorPlate.Model;

namespace LuckyRotorPlate
{
    /// <summary>
    /// SelectList.xaml 的交互逻辑
    /// </summary>
    public partial class SelectList : Window
    {
        private ObservableCollection<RealThing> things = new ObservableCollection<RealThing>();

        public ObservableCollection<RealThing> Things
        {
            get { return things; }
            set { things = value; }
        }

        public SelectList(ObservableCollection<RealThing> things)
        {
            InitializeComponent();

            Binding binding = new Binding();
            binding.Source = things;
            this.ThingList.SetBinding(ListBox.ItemsSourceProperty, binding);
        }

        private void BtnSelete_Click(object sender, RoutedEventArgs e)
        {
            if (ThingList.SelectedIndex >= 0)
            {
                foreach (RealThing item in this.ThingList.SelectedItems)
                {
                    things.Add(item);
                }
            }
            this.Close();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            ThingList.SelectAll();
        }
    }
}
