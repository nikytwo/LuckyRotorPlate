using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LuckyRotorPlate.DataService;
using LuckyRotorPlate.Model;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;

namespace LuckyRotorPlate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private LuckyControl curLuckyCtrl;
        private AppSettingsSection appSettings = ((LuckyRotorPlateApp)Application.Current).AppSettings;
        private BrushConverter bc = new BrushConverter();

        private int times;
        private String backgroudImgPath = "img/backgroud.jpg";
        private Brush backColor = Brushes.White;
        private BitmapImage leaveImg;
        private BitmapImage enterImg;
        private BitmapImage clickImg;
        private Cursor arrowCursor;
        private Cursor waitCursor;
        private Cursor handCursor;
        private Cursor noCursor;
        private Brush highLightColor = Brushes.Yellow;
        private double dispatcherTime = 10;
        private double minPower = 3;
        private bool isTest = false;

        public MainWindow()
        {
            initConfig();

            InitializeComponent();

            initControl();
        }

        private void initConfig()
        {
            highLightColor = null != appSettings.Settings["HighLightPieColor"] ? 
                bc.ConvertFromString(appSettings.Settings["HighLightPieColor"].Value) as Brush : highLightColor;
            backColor = null != appSettings.Settings["WindowBackColor"] ? 
                bc.ConvertFromString(appSettings.Settings["WindowBackColor"].Value) as Brush : backColor;
            backgroudImgPath = null != appSettings.Settings["BackgroudImg"] ? 
                appSettings.Settings["BackgroudImg"].Value : backgroudImgPath;
            dispatcherTime = null != appSettings.Settings["DispatcherTime"] ?
                double.Parse(appSettings.Settings["DispatcherTime"].Value) : dispatcherTime;
            minPower = null != appSettings.Settings["Speed"] ? 
                double.Parse(appSettings.Settings["Speed"].Value) / 100 : minPower;//转盘初始量(每10毫秒移动的度数) = 速度 / 100
            isTest = null != appSettings.Settings["Test"] ? 
                appSettings.Settings["Test"].Value.ToUpper().Equals("TRUE") : false;
        }

        private void initControl()
        {
            leaveImg = new BitmapImage(new Uri(@"img\button\play1.png", UriKind.Relative));
            enterImg = new BitmapImage(new Uri(@"img\button\play2.png", UriKind.Relative));
            clickImg = new BitmapImage(new Uri(@"img\button\play3.png", UriKind.Relative));
            this.imgBtnStart.Source = leaveImg;

            arrowCursor = new Cursor(Directory.GetCurrentDirectory() +
                Path.DirectorySeparatorChar + @"img\Cursor\Arrow.cur");
            waitCursor = new Cursor(Directory.GetCurrentDirectory() +
                Path.DirectorySeparatorChar + @"img\Cursor\Wait.ani");
            handCursor = new Cursor(Directory.GetCurrentDirectory() +
                Path.DirectorySeparatorChar + @"img\Cursor\Hand.cur");
            noCursor = new Cursor(Directory.GetCurrentDirectory() +
                Path.DirectorySeparatorChar + @"img\Cursor\No.cur");
            this.Cursor = arrowCursor;

            this.Background = backColor;

            ImageBrush imagebrush = new ImageBrush();
            Uri uri = new Uri(backgroudImgPath, UriKind.Relative);
            imagebrush.ImageSource = new BitmapImage(uri);
            this.gdMain.Background = imagebrush;
        }

        private void btnManager_Click(object sender, RoutedEventArgs e)
        {
            ManagerWindow mw = new ManagerWindow();
            mw.Owner = this;
            mw.ShowDialog();
        }

        private void cmSolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gdForRotorPlate != null)
            {

                Solution sou = CollectionViewSource.GetDefaultView(cmSolutions.ItemsSource).CurrentItem as Solution;
                if (isTest)
                {
                    curLuckyCtrl = new LuckyControl(sou, minPower, true);
                }
                else
                {
                    curLuckyCtrl = new LuckyControl(sou, minPower, false);
                }
                curLuckyCtrl.DispatcherTime = dispatcherTime;
                curLuckyCtrl.HighLightColor = highLightColor;
                curLuckyCtrl.GetingResult += new EventHandler(curLucky_GetingResult);
                curLuckyCtrl.StepCompleted += new EventHandler(curLuckyCtrl_LuckyCompleted);
                gdForRotorPlate.Children.Clear();
                gdForRotorPlate.Children.Add(curLuckyCtrl);
            }
        }

        void curLuckyCtrl_LuckyCompleted(object sender, EventArgs e)
        {
            ObservableCollection<Solution> solutions = (Application.Current as LuckyRotorPlateApp).Solutions;
            Solution selectSou;
            for (int i = 0; i < solutions.Count; i++)
            {
                selectSou = solutions[i];
                foreach (RealThing t in selectSou.StepList[0].Players)
                {
                    if (String.IsNullOrEmpty(t.ObtainedGift))
                    {
                        cmSolutions.SelectedIndex = i;
                        return;
                    }
                }
            }

            //if (cmSolutions.SelectedIndex < cmSolutions.Items.Count - 1)
            //{
            //    cmSolutions.SelectedIndex = cmSolutions.SelectedIndex + 1;
            //}
        }

        void curLucky_GetingResult(object sender, EventArgs e)
        {
            enabledButton();
            if (--times > 0)
            {
                startLucky();
            }
        }

        private void disabledButton()
        {
            btnManager.IsEnabled = false;
            btnLucky.IsEnabled = false;
            cmSolutions.IsEnabled = false;
            imgBtnStart.IsEnabled = false;
            imgBtnStart.Source = clickImg;
            this.Cursor = waitCursor;
        }

        private void enabledButton()
        {
            btnManager.IsEnabled = true;
            btnLucky.IsEnabled = true;
            cmSolutions.IsEnabled = true;
            imgBtnStart.IsEnabled = true;
            imgBtnStart.Source = leaveImg;
            this.Cursor = arrowCursor;
            txtLoopTimes.Focus();
            txtLoopTimes.SelectAll();
        }

        private void btnLucky_Click(object sender, RoutedEventArgs e)
        {
            curLuckyCtrl.Ready();
        }

        private void startLucky()
        {
            disabledButton();
            curLuckyCtrl.LuckyTime();     
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Solution sou = CollectionViewSource.GetDefaultView(cmSolutions.ItemsSource).CurrentItem as Solution;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel Worksheets|*.xls";
            dlg.FileName = sou.Name;
            if (dlg.ShowDialog(this).Value)
            {
                ObservableCollection<RealThing> things 
                    = CollectionViewSource.GetDefaultView(this.lvResults.ItemsSource).SourceCollection 
                    as ObservableCollection<RealThing>;
                XlsHelper.SaveRealThings(dlg.FileName, things, sou.Name);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog().Value == true)
            {
                Thickness margin = lvResults.Margin;
                lvResults.Margin = new Thickness(5, 5, 5, 5);
                HorizontalAlignment ha = lvResults.HorizontalAlignment;
                lvResults.HorizontalAlignment = HorizontalAlignment.Stretch;
                double orgWidth = lvResults.Width;
                lvResults.Width = 780;

                dialog.PrintVisual(lvResults, "打印结果");

                lvResults.Margin = margin;
                lvResults.HorizontalAlignment = ha;
                lvResults.Width = orgWidth;
            }
        }

        private void imgBtnStart_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (imgBtnStart.IsEnabled)
            {
                imgBtnStart.Source = enterImg;
                this.Cursor = handCursor;
            }
            else
            {
                this.Cursor = noCursor;
            }
        }

        private void imgBtnStart_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (imgBtnStart.IsEnabled)
            {
                this.Cursor = arrowCursor;
                imgBtnStart.Source = leaveImg;
            }
            else
            {
                this.Cursor = waitCursor;
            }
        }

        private void imgBtnStart_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int.TryParse(txtLoopTimes.Text, out times);
            startLucky();
        }

        private void gridTool_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridTool.Opacity = 1;
        }

        private void gridTool_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridTool.Opacity = 0;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (imgBtnStart.IsEnabled)
            {
                if (e.Key == Key.F12)
                {
                    btnManager_Click(sender, null);
                }
                if (e.Key == Key.Enter)
                {
                    imgBtnStart_MouseDown(sender, null);
                }
            }
        }
    }
}
