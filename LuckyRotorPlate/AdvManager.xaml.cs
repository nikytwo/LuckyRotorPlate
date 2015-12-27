using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LuckyRotorPlate.Model;

namespace LuckyRotorPlate
{
    /// <summary>
    /// GlobalExcludes.xaml 的交互逻辑
    /// </summary>
    public partial class AdvManager : Window
    {
        private StepInfo step;

        public AdvManager(StepInfo step)
        {
            InitializeComponent();

            this.step = step;
            Binding b = new Binding();
            b.Source = this.step.ExcludePies;
            this.lbGlobalExcludes.SetBinding(ListBox.ItemsSourceProperty, b);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //显示例外的列表并添加
            SelectList sl;
            sl = new SelectList(step.Pies);
            sl.Owner = this;
            sl.ShowDialog();
            if (null != sl.Things)
            {
                foreach (RealThing thing in sl.Things)
                {
                    if (!step.ExcludePies.Contains(thing) && !StepInfo.ThingsContainNameAndValue(thing.Name, thing.Value, step.ExcludePies))
                    {
                        step.ExcludePies.Add(thing);
                    }
                }
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (lbGlobalExcludes.SelectedIndex >= 0)
            {
                //lbGlobalExcludes.Items.RemoveAt(lbGlobalExcludes.SelectedIndex);
                step.ExcludePies.Remove(lbGlobalExcludes.SelectedItem as RealThing);
            }
        }
    }
}
