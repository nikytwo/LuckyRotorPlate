using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LuckyRotorPlate.DataService;
using LuckyRotorPlate.Model;
using Microsoft.Win32;

namespace LuckyRotorPlate
{
    /// <summary>
    /// ManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private int LoadType = 0;//导入数据方式:0=清除原数据
        private int startNO = 0;//顺序号开始号码

        public ManagerWindow()
        {
            InitializeComponent();

            LoadType = 0;
        }

        private void LoadEmployees_Click(object sender, RoutedEventArgs e)
        {
            LoadXlsDataTo(((LuckyRotorPlateApp)Application.Current).Employees);
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            AddThing ath = new AddThing(null, ((LuckyRotorPlateApp)Application.Current).Gifts);
            ath.Owner = this;
            ath.ShowDialog();
            if (ath.Thing != null)
            {
                ((LuckyRotorPlateApp)Application.Current).Employees.Add(ath.Thing);
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource empsSrc = this.Resources["employeeList"] as CollectionViewSource;
            AddThing ath = new AddThing((RealThing)empsSrc.View.CurrentItem, ((LuckyRotorPlateApp)Application.Current).Gifts);
            ath.Owner = this;
            ath.ShowDialog();
        }

        private void DelEmployee_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource empsSrc = this.Resources["employeeList"] as CollectionViewSource;
            RealThing thing = (RealThing)empsSrc.View.CurrentItem;
            if (MessageBox.Show("确定要删除[" + thing.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ((LuckyRotorPlateApp)Application.Current).Employees.Remove(thing);
            }
        }

        private void LoadGifts_Click(object sender, RoutedEventArgs e)
        {
            LoadXlsDataTo(((LuckyRotorPlateApp)Application.Current).Gifts);
        }

        private void AddGift_Click(object sender, RoutedEventArgs e)
        {
            AddThing ath = new AddThing(null, ((LuckyRotorPlateApp)Application.Current).Employees);
            ath.Owner = this;
            ath.ShowDialog();
            if (null != ath.Thing)
            {
                ((LuckyRotorPlateApp)Application.Current).Gifts.Add(ath.Thing);
            }
        }

        private void EditGift_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource empsSrc = this.Resources["giftList"] as CollectionViewSource;
            AddThing ath = new AddThing((RealThing)empsSrc.View.CurrentItem, ((LuckyRotorPlateApp)Application.Current).Employees);
            ath.Owner = this;
            ath.ShowDialog();
        }

        private void DelGift_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource empsSrc = this.Resources["giftList"] as CollectionViewSource;
            RealThing thing = (RealThing)empsSrc.View.CurrentItem;
            if (MessageBox.Show("确定要删除[" + thing.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ((LuckyRotorPlateApp)Application.Current).Gifts.Remove(thing);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<RealThing> employees = ((LuckyRotorPlateApp)Application.Current).Employees;
            AppSettingsSection appSettings = ((LuckyRotorPlateApp)Application.Current).AppSettings;
            XmlHelper.SaveRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["EmployeesData"].Value, employees);
            ObservableCollection<RealThing> gifts = ((LuckyRotorPlateApp)Application.Current).Gifts;
            XmlHelper.SaveRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["GiftsData"].Value, gifts);
            ObservableCollection<Solution> solutions = ((LuckyRotorPlateApp)Application.Current).Solutions;
            XmlHelper.SaveSolutions(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["SolutionsData"].Value, solutions);
        }

        private void LoadXlsDataTo(ObservableCollection<RealThing> things)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Excel Worksheets|*.xls";
            if (dlg.ShowDialog(this).Value)
            {
                if (LoadType == 0)
                {
                    things.Clear();
                }
                ObservableCollection<RealThing> newThings = XlsHelper.GetRealThings(dlg.FileName, 0, 0);
                foreach (RealThing thing in newThings)
                {
                    things.Add(thing);
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditEmployee_Click(null, e);
        }

        private void listView2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditGift_Click(null, e);
        }

        private void btAddSolution_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Solution> sous = ((LuckyRotorPlateApp)Application.Current).Solutions;
            ObservableCollection<StepInfo> steps = new ObservableCollection<StepInfo>();
            StepInfo step = new StepInfo(){Name = "步骤1"};
            steps.Add(step);
            sous.Add(new Solution("新方案", steps));
            ICollectionView view = CollectionViewSource.GetDefaultView(this.lbSolutions.ItemsSource);
            view.MoveCurrentToLast();
            txtSolutionName.Focus();
            txtSolutionName.SelectAll();
        }

        private void btDelSolution_Click(object sender, RoutedEventArgs e)
        {
            Solution sou = CollectionViewSource.GetDefaultView(this.lbSolutions.ItemsSource).CurrentItem as Solution;
            if (MessageBox.Show("确定要删除[" + sou.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ((LuckyRotorPlateApp)Application.Current).Solutions.Remove(sou);
            }
        }

        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this.lbSolutions.ItemsSource);
            Solution sou = view.CurrentItem as Solution;
            if (null == sou)
            {
                return;
            }
            StepInfo step = new StepInfo() { Name = "新步骤" };
            sou.StepList.Add(step);
            CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).MoveCurrentToLast();
            txtStepName.Focus();
            txtStepName.SelectAll();
        }

        private void btnDelStep_Click(object sender, RoutedEventArgs e)
        {
            Solution sou = CollectionViewSource.GetDefaultView(this.lbSolutions.ItemsSource).CurrentItem as Solution;
            if (null == sou)
            {
                return;
            }
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            if (MessageBox.Show("确定要删除[" + step.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                sou.StepList.Remove(step);
            }
        }

        private void btAddPie_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            SeleteSourceForm sf = new SeleteSourceForm();
            sf.Owner = this;
            sf.ShowDialog();
            if (sf.rbFromEmployees.IsChecked.Value)
            {
                //从员工列表获取
                //setThingsList(step.Pies, ((LuckyRotorPlateApp)Application.Current).Employees);
                SelectList sl;
                sl = new SelectList(((LuckyRotorPlateApp)Application.Current).Employees);
                sl.Owner = this;
                sl.ShowDialog();
                if (null != sl.Things)
                {
                    ObservableCollection<RealThing> sourceThings = RandomThings(sl.Things);
                    AddThings(step.Pies, sourceThings);
                }
            }
            else if (sf.rbFromGifts.IsChecked.Value)
            {
                //从礼品列表获取
                //setThingsList(step.Pies, ((LuckyRotorPlateApp)Application.Current).Gifts);
                SelectList sl;
                sl = new SelectList(((LuckyRotorPlateApp)Application.Current).Gifts);
                sl.Owner = this;
                sl.ShowDialog();
                if (null != sl.Things)
                {
                    ObservableCollection<RealThing> sourceThings = RandomThings(sl.Things);
                    AddThings(step.Pies, sourceThings);
                }
            }
            else if (sf.rbFromNumbers.IsChecked.Value)
            {
                for (int i = 0; i < int.Parse(sf.txtcount.Text); i++)
                {
                    step.Pies.Add(new RealThing((i + startNO).ToString(), 1, ""));
                }
            }
            else if (sf.rbFromCustom.IsChecked.Value)
            {
                //自定义
                AddThing ath = new AddThing(null, step.Pies);
                ath.Owner = this;
                ath.ShowDialog();
                if (ath.Thing != null)
                {
                    step.Pies.Add(ath.Thing);
                }
            }

        }

        /// <summary>
        /// 打乱Things集合顺序
        /// </summary>
        /// <param name="things">操作前的集合</param>
        /// <returns>操作后的集合</returns>
        private ObservableCollection<RealThing> RandomThings(ObservableCollection<RealThing> things)
        {
            ObservableCollection<RealThing> tmpThings = new ObservableCollection<RealThing>(things);
            ObservableCollection<RealThing> newThings = new ObservableCollection<RealThing>();

            int randomIndex;
            int seed = unchecked((int)(DateTime.Now.Ticks));
            Random rd = new Random(seed);
            while (tmpThings.Count > 0)
            {
                randomIndex = rd.Next(0, tmpThings.Count);
                newThings.Add(tmpThings[randomIndex]);
                tmpThings.RemoveAt(randomIndex);
            }

            return newThings;
        }

        //private void setThingsList(ObservableCollection<RealThing> things, ObservableCollection<RealThing> thingsForSelect)
        //{
        //    SelectList sl;
        //    sl = new SelectList(thingsForSelect);
        //    sl.ShowDialog();
        //    if (null != sl.Things)
        //    {
        //        ObservableCollection<RealThing> selectThings = RandomThings(sl.Things);
        //        AddThings(things, selectThings);
        //    }
        //}

        private static void AddThings(ObservableCollection<RealThing> targetThings, 
            ObservableCollection<RealThing> sourceThings)
        {
            foreach (RealThing thing in sourceThings)
            {
                if (!targetThings.Contains(thing) 
                    && !StepInfo.ThingsContainNameAndValue(thing.Name, thing.Value, targetThings))
                {
                    //TODO 未考虑Player对象
                    targetThings.Add(thing.Clone() as RealThing);
                }
            }
        }

        private void btnDelPie_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            RealThing thing = CollectionViewSource.GetDefaultView(lbPies.ItemsSource).CurrentItem as RealThing;
            if (null == thing)
            {
                return;
            }
            if (MessageBox.Show("确定要删除[" + thing.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                step.Pies.Remove(thing);
            }
        }

        private void btnAddPlayer_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            SeleteSourceForm sf = new SeleteSourceForm();
            sf.Owner = this;
            sf.ShowDialog();
            if (sf.rbFromEmployees.IsChecked.Value)
            {
                //从员工列表获取
                //setThingsList(step.Players, ((LuckyRotorPlateApp)Application.Current).Employees);
                SelectList sl;
                sl = new SelectList(((LuckyRotorPlateApp)Application.Current).Employees);
                sl.Owner = this;
                sl.ShowDialog();
                if (null != sl.Things)
                {
                    AddThings(step.Players, sl.Things);
                }
            }
            else if (sf.rbFromGifts.IsChecked.Value)
            {
                //从礼品列表获取
                //setThingsList(step.Players, ((LuckyRotorPlateApp)Application.Current).Gifts);
                SelectList sl;
                sl = new SelectList(((LuckyRotorPlateApp)Application.Current).Gifts);
                sl.Owner = this;
                sl.ShowDialog();
                if (null != sl.Things)
                {
                    AddThings(step.Players, sl.Things);
                }
            }
            else if (sf.rbFromNumbers.IsChecked.Value)
            {
                for (int i = 0; i < int.Parse(sf.txtcount.Text); i++)
                {
                    step.Players.Add(new RealThing((i + 1).ToString(), 1, ""));
                }
            }
            else if (sf.rbFromCustom.IsChecked.Value)
            {
                AddThing ath = new AddThing(null, step.Players);
                ath.Owner = this;
                ath.ShowDialog();
                if (ath.Thing != null)
                {
                    step.Players.Add(ath.Thing);
                }
            }
        }

        private void btnDelPlayer_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            RealThing thing = CollectionViewSource.GetDefaultView(this.lbPlayers.ItemsSource).CurrentItem as RealThing;
            if (null == thing)
            {
                return;
            }
            if (MessageBox.Show("确定要删除[" + thing.Name + "]？", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                step.Players.Remove(thing);
            }
        }

        private void btnEditPlayer_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            RealThing thing = CollectionViewSource.GetDefaultView(this.lbPlayers.ItemsSource).CurrentItem as RealThing;
            if (null == thing)
            {
                return;
            }
            AddThing ath = new AddThing(thing, step.Pies);
            ath.Owner = this;
            ath.ShowDialog();
        }

        private void lbPlayers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEditPlayer_Click(sender, e);
        }

        private void btnExcludes_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            AdvManager advm = new AdvManager(step);
            advm.Owner = this;
            advm.ShowDialog();
        }

        private void btnEditPie_Click(object sender, RoutedEventArgs e)
        {
            StepInfo step = CollectionViewSource.GetDefaultView(lbStepList.ItemsSource).CurrentItem as StepInfo;
            if (null == step)
            {
                return;
            }
            RealThing thing = CollectionViewSource.GetDefaultView(this.lbPies.ItemsSource).CurrentItem as RealThing;
            if (null == thing)
            {
                return;
            }
            AddThing ath = new AddThing(thing, step.Players);
            ath.Owner = this;
            ath.ShowDialog();
        }

        private void lbPies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEditPie_Click(sender, e);
        }

        private void chkStepToPies_Checked(object sender, RoutedEventArgs e)
        {
            cmStepForPie.IsEnabled = true;
            btnAddPie.IsEnabled = false;
            btnEditPie.IsEnabled = false;
            btnDelPie.IsEnabled = false;
            lbPies.IsEnabled = false;
        }

        private void btnExportResults_Click(object sender, RoutedEventArgs e)
        {
            Solution sou = CollectionViewSource.GetDefaultView(lbSolutions.ItemsSource).CurrentItem as Solution;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel Worksheets|*.xls";
            dlg.FileName = sou.Name;
            if (dlg.ShowDialog(this).Value)
            {
                ObservableCollection<RealThing> things
                    = CollectionViewSource.GetDefaultView(this.lstResults.ItemsSource).SourceCollection
                    as ObservableCollection<RealThing>;
                XlsHelper.SaveRealThings(dlg.FileName, things, sou.Name);
            }
        }

        private void btnPrintResults_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog().Value == true)
            {
                GridLength g1 = grid1.ColumnDefinitions[0].Width;
                GridLength g2 = grid1.ColumnDefinitions[1].Width;
                grid1.ColumnDefinitions[0].Width = new GridLength(0);
                grid1.ColumnDefinitions[1].Width = new GridLength(0);
                Thickness margin = this.lstResults.Margin;
                lstResults.Margin = new Thickness(5, 5, 5, 5);
                HorizontalAlignment ha = lstResults.HorizontalAlignment;
                lstResults.HorizontalAlignment = HorizontalAlignment.Stretch;
                double orgWidth = lstResults.Width;
                lstResults.Width = 780;

                dialog.PrintVisual(lstResults, "打印结果");

                grid1.ColumnDefinitions[0].Width = g1;
                grid1.ColumnDefinitions[1].Width = g2;
                lstResults.Margin = margin;
                lstResults.HorizontalAlignment = ha;
                lstResults.Width = orgWidth;
            }
        }

        private void btnClearResults_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<RealThing> things
                = CollectionViewSource.GetDefaultView(this.lstResults.ItemsSource).SourceCollection
                as ObservableCollection<RealThing>;
            foreach (RealThing thing in things)
            {
                thing.ObtainedGift = string.Empty;
            }
        }

        private void txtDrap_TextChanged(object sender, TextChangedEventArgs e)
        {
            double dp;
            if (double.TryParse(txtDrap.Text, out dp))
            {
                Solution sou = CollectionViewSource.GetDefaultView(lbSolutions.ItemsSource).CurrentItem as Solution;
                sou.Drap = dp;
            }
            else
            {
                MessageBox.Show("请输入0-1之间的小数。");
            }
        }
    }
}
