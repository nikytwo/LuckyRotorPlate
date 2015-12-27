using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using LuckyRotorPlate.Model;
using NKLogic.UIElements;
using Petzold.Text3D;

namespace LuckyRotorPlate
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private Brush luckyPiontColor = Brushes.Black;
        private Brush pieColor1 = Brushes.Yellow;
        private Brush pieColor2 = Brushes.Red;
        private Brush pieColor3 = Brushes.Blue;
        private Brush pieFontColor = Brushes.Black;
        private Brush pieBorderColor = Brushes.LightGray;
        private Brush showTextColor = Brushes.Blue;
        private Brush showTextSideColor = Brushes.Blue;
        private String backgroudImg = "img/backgroud.jpg";
        private Brush backColor = Brushes.White;

        private ObservableCollection<Solution> solutionList;
        private ObservableCollection<PiePieceElement> employeePies = new ObservableCollection<PiePieceElement>();//人员转盘
        private ObservableCollection<PiePieceElement> giftPies = new ObservableCollection<PiePieceElement>();//礼品转盘
        private double power = 1.199;//转盘初始量
        private double drap = 0.002;//转盘阻力系数
        private Point luckyPoint;
        Polygon pg;
        DispatcherTimer dispatcherTimer;
        private double testPower = 1.199;//2.07746102021178;//1.69605656946;//1.19900;
        private double testDist;

        private int selectPieIndex = -1;
        private Storyboard reBuildStoryboard;

        RotateTransform3D transform;
        DirectionalLight dirLight;
        PerspectiveCamera camera;
        Viewport3D vp3d;

        List<RealThing> employeeList;//人员列表
        List<RealThing> giftList;//礼品列表

        public Window1()
        {
            InitializeComponent();

            //加载配置
            BrushConverter bc = new BrushConverter();
            AppSettingsSection appSettings = ((LuckyRotorPlateApp)Application.Current).AppSettings;
            luckyPiontColor = bc.ConvertFromString(appSettings.Settings["LuckyPiontColor"].Value) as Brush;
            pieColor1 = bc.ConvertFromString(appSettings.Settings["PieColor1"].Value) as Brush;
            pieColor2 = bc.ConvertFromString(appSettings.Settings["PieColor2"].Value) as Brush;
            pieColor3 = bc.ConvertFromString(appSettings.Settings["PieColor3"].Value) as Brush;
            pieFontColor = bc.ConvertFromString(appSettings.Settings["PieFontColor"].Value) as Brush;
            pieBorderColor = bc.ConvertFromString(appSettings.Settings["PieBorderColor"].Value) as Brush;
            showTextColor = bc.ConvertFromString(appSettings.Settings["GiftColor"].Value) as Brush;
            showTextSideColor = bc.ConvertFromString(appSettings.Settings["GiftColor"].Value) as Brush;
            backColor = bc.ConvertFromString(appSettings.Settings["WindowBackColor"].Value) as Brush;
            backgroudImg = appSettings.Settings["BackgroudImg"].Value;

            this.Background = backColor;
            ImageBrush imagebrush = new ImageBrush();
            Uri uri = new Uri(backgroudImg, UriKind.Relative);
            imagebrush.ImageSource = new BitmapImage(uri);
            this.grid1.Background = imagebrush;

            init3DViewPort();

            solutionList = loadSolutions();

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);

        }

        /// <summary>
        /// 加载抽奖方案列表
        /// </summary>
        private ObservableCollection<Solution> loadSolutions()
        {
            ObservableCollection<Solution> suolist = new ObservableCollection<Solution>();

            return suolist;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            List<RealThing> lst = new List<RealThing>();
            lst.Add(new RealThing("最佳男猪脚", 1, ""));
            lst.Add(new RealThing("金鸡奖", 10, ""));
            lst.Add(new RealThing("最佳女猪脚", 1, ""));
            lst.Add(new RealThing("金马奖", 10, ""));
            lst.Add(new RealThing("最佳导演", 1, ""));
            lst.Add(new RealThing("金牛奖", 10, ""));
            lst.Add(new RealThing("影后", 1, ""));
            lst.Add(new RealThing("奥斯卡奖", 10, ""));
            lst.Add(new RealThing("影帝", 1, ""));
            lst.Add(new RealThing("金龙奖", 10, ""));
            //initRotorPlate(employeePies, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height, lst);
            initRotorPlate(employeePies, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            initLuckyPiont(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);

            if (grid1.Children.Contains(vp3d))
            {
                grid1.Children.Remove(vp3d);
            }
            grid1.Children.Add(vp3d);
        }

        /// <summary>
        /// 初始化转盘
        /// </summary>
        private void initRotorPlate(ObservableCollection<PiePieceElement> pies, double width, double height)
        {
            double radius = Math.Min(width, height) / 2 - 20;//圆半径
            int count = 24;//圆饼数
            double angle = 360.0 / count;
            double centreX = radius + 10;
            double centreY = radius + 10;

            if (pies.Count > 0)
            {
                for (int i = 0; i < pies.Count; i++)
                {
                    grid1.Children.Remove(pies[i]);
                }
                //myPies.Clear();
                for (int i = 0; i < pies.Count; i++)
                {
                    pies[i].Radius = radius;
                    pies[i].InnerRadius = radius - 30;
                    pies[i].CentreX = centreX;
                    pies[i].CentreY = centreY;
                    grid1.Children.Add(pies[i]);
                }
                return;
            }

            for (int i = 0; i < count; i++)
            {
                PiePieceElement pp;
                if (i % 3 == 0 && i != (count - 1))
                {
                    pp = new PiePieceElement(i.ToString(), radius, radius - 30, 0, angle,
                        (i + 0) * angle, centreX, centreY, 1, pieBorderColor, pieColor1, pieFontColor, "宋体", false);
                }
                else if (i % 3 == 2)
                {
                    pp = new PiePieceElement(i.ToString(), radius, radius - 30, 0, angle,
                        (i + 0) * angle, centreX, centreY, 1, pieBorderColor, pieColor2, pieFontColor, "宋体", false);
                }
                else
                {
                    pp = new PiePieceElement(i.ToString(), radius, radius - 30, 0, angle,
                        (i + 0) * angle, centreX, centreY, 1, pieBorderColor, pieColor3, pieFontColor, "宋体", false);
                }
                pies.Add(pp);
                grid1.Children.Add(pp);
            }
        }

        private void initRotorPlate(ObservableCollection<PiePieceElement> pies, double width, double height, 
            List<RealThing> employeeList)
        {
            double radius = Math.Min(width, height) / 2 - 20;//圆半径
            double allValue = 0;
            for (int i = 0; i < employeeList.Count; i++)
            {
                allValue += employeeList[i].Value;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;
            double centreX = radius + 10;
            double centreY = radius + 10;

            if (pies.Count > 0)
            {
                for (int i = 0; i < pies.Count; i++)
                {
                    grid1.Children.Remove(pies[i]);
                }
                //myPies.Clear();
                for (int i = 0; i < pies.Count; i++)
                {
                    pies[i].Radius = radius;
                    pies[i].InnerRadius = radius - 30;
                    pies[i].CentreX = centreX;
                    pies[i].CentreY = centreY;
                    grid1.Children.Add(pies[i]);
                }
                return;
            }

            for (int i = 0; i < employeeList.Count; i++)
            {
                PiePieceElement pp;
                if (i % 3 == 0 && i != (employeeList.Count - 1))
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - 50, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor1, pieFontColor, "宋体", true);
                }
                else if (i % 3 == 2)
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - 50, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor2, pieFontColor, "宋体", true);
                }
                else
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - 50, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor3, pieFontColor, "宋体", true);
                }
                endAngle += perAngle * employeeList[i].Value;

                pies.Add(pp);
                grid1.Children.Add(pp);
            }
        }

        /// <summary>
        /// 移除某圆饼后重新绘制圆（动画）
        /// </summary>
        /// <param name="removeIndex">被移除的圆饼</param>
        private void reBuildRotorPlate(ObservableCollection<PiePieceElement> pies, int removeIndex)
        {
            double allValue = 0;
            for (int i = 0; i < pies.Count; i++)
            {
                allValue += pies[i].PieceValue;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;
            #region 变换动画
            DoubleAnimation doubleAnimation;

            reBuildStoryboard = new Storyboard();
            reBuildStoryboard.FillBehavior = FillBehavior.Stop;

            for (int i = removeIndex; i < pies.Count; i++)
            {
                doubleAnimation = new DoubleAnimation(pies[i].WedgeAngle, perAngle * pies[i].PieceValue, 
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, pies[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.WedgeAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimation(pies[i].RotationAngle, 90 + endAngle, 
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, pies[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RotationAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                endAngle += perAngle * pies[i].PieceValue;
            }

            endAngle = 0;
            for (int i = removeIndex - 1; i >= 0; i--)
            {
                endAngle += perAngle * pies[i].PieceValue;

                doubleAnimation = new DoubleAnimation(pies[i].WedgeAngle, perAngle * pies[i].PieceValue,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, pies[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.WedgeAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimation(pies[i].RotationAngle, (90 - endAngle + 360) % 360,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, pies[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RotationAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);
            }

            reBuildStoryboard.Completed += new EventHandler(reBuildStoryboard_Completed);
            reBuildStoryboard.Begin();
            #endregion
        }

        void reBuildStoryboard_Completed(object sender, EventArgs e)
        {
            //移除某圆饼后重新绘制圆（非动画）
            double allValue = 0;
            for (int i = 0; i < employeePies.Count; i++)
            {
                allValue += employeePies[i].PieceValue;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;

            for (int i = selectPieIndex; i < employeePies.Count; i++)
            {
                employeePies[i].WedgeAngle = perAngle * employeePies[i].PieceValue;
                employeePies[i].RotationAngle = 90 + endAngle;

                endAngle += perAngle * employeePies[i].PieceValue;
            }

            endAngle = 0;
            for (int i = selectPieIndex - 1; i >= 0; i--)
            {
                endAngle += perAngle * employeePies[i].PieceValue;

                employeePies[i].WedgeAngle = perAngle * employeePies[i].PieceValue;
                employeePies[i].RotationAngle = (90 - endAngle + 360) % 360;
            }

            this.IsEnabled = true;

            //luckyTime();
        }

        /// <summary>
        /// 初始化中奖的点
        /// </summary>
        private void initLuckyPiont(double width, double height)
        {
            double radius = Math.Min(width, height) / 2 - 20;//圆半径
            luckyPoint = new Point(2 * radius - 10, radius + 10);
            if (grid1.Children.Contains(pg))
            {
                grid1.Children.Remove(pg);
            }
            pg = new Polygon();
            pg.Fill = luckyPiontColor;
            Point Point1 = luckyPoint;
            Point Point2 = new Point(luckyPoint.X + 50, luckyPoint.Y - 10);
            Point Point3 = new Point(luckyPoint.X + 50, luckyPoint.Y + 10);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            pg.Points = myPointCollection;
            grid1.Children.Add(pg);
        }

        /// <summary>
        /// 初始化3D场景
        /// </summary>
        private void init3DViewPort()
        {
            sliderAngle.Value = 10.0;
            sliderX.Value = 1.5;
            sliderY.Value = -0.15;
            sliderZ.Value = -4;
            sliderFiledOfView.Value = 35;

            vp3d = new Viewport3D();
            dirLight = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0), 
                new Vector3D(sliderX.Value, sliderY.Value, sliderZ.Value));
            DirectionalLight d2 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
                new Vector3D(2, -2, 6));
            DirectionalLight d3 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
                new Vector3D(-5, -5, 6));
            DirectionalLight d4 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
                new Vector3D(2, 0, -3));
            DirectionalLight d5 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
                new Vector3D(-2, 0, -3));
            transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, -1, 0), sliderAngle.Value));
            camera = new PerspectiveCamera(new Point3D(-1, -0.2, 8), 
                new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), sliderFiledOfView.Value);
            
            ModelVisual3D mv3d = new ModelVisual3D();
            Model3DGroup m3g = new Model3DGroup();
            m3g.Children.Add(new AmbientLight(Color.FromRgb(0x40, 0x40, 0x40)));
            //m3g.Children.Add(dirLight);
            m3g.Children.Add(d2);
            m3g.Children.Add(d3);
            m3g.Children.Add(d4);
            m3g.Children.Add(d5);
            mv3d.Content = m3g;

            vp3d.Camera = camera;
            vp3d.Children.Add(mv3d);

            vp3d.Children.Add(draw3DText("行政科(含离岗退养干部)、技术保障科", transform));
        }

        /// <summary>
        /// 绘制3D字体
        /// </summary>
        /// <param name="text">需要绘制的字符串</param>
        /// <param name="transform">RotateTransform3D 对象</param>
        /// <returns>3D字体对象</returns>
        private SolidText draw3DText(string text, RotateTransform3D transform)
        {
            int wordLength = text.Length;
            SolidText st;
            st = new SolidText();
            st.Text = text;
            st.FontFamily = new FontFamily("Times New Roman");
            st.FontWeight = FontWeights.Bold;
            st.FontSize = 1;
            st.Origin = new Point(-(wordLength * 0.5) - 1, 0.5);
            st.Depth = 0.5;
            MaterialGroup mg;
            mg = new MaterialGroup();
            mg.Children.Add(new DiffuseMaterial(showTextColor));
            SpecularMaterial sm = new SpecularMaterial();
            sm.Brush = Brushes.White;
            sm.SpecularPower = 85.3333;
            mg.Children.Add(sm);
            st.Material = mg;
            mg = new MaterialGroup();
            mg.Children.Add(new DiffuseMaterial(showTextSideColor));
            mg.Children.Add(sm);
            st.SideMaterial = mg;
            st.Transform = transform;

            return st;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            double lastPower = power;
            if (power > 0)
            {

                for (int i = 0; i < employeePies.Count; i++)
                {
                    employeePies[i].RotationAngle += power;
                }
                testDist += power;// 测试转动距离总量
                power -= drap;
            }
            if (lastPower > 0 && power <= 0)
            {

                dispatcherTimer.Tick -= new EventHandler(CompositionTarget_Rendering);

                //showmessge
                selectPieIndex = GetLuckyPieIndex(0);

                ReduceWeight(selectPieIndex);

                string luckyName = employeePies[selectPieIndex].PieName;
                showLuckyName(luckyName);

                playFireworks();

                //this.IsEnabled = true;
            }
        }

        /// <summary>
        /// 重新计算权值
        /// </summary>
        /// <param name="pieId"></param>
        private void ReduceWeight(int pieId)
        {
            employeePies[pieId].PieceValue -= 1;
            if (employeePies[pieId].PieceValue <= 0)
            {
                //当权值小于0时，移除该圆饼
                pushOutPie(pieId);
                smallPie(pieId);
            }
            else
            {
                //重新构建转盘。
                reBuildRotorPlate(employeePies, pieId);
            }
        }

        /// <summary>
        /// 展示幸运人
        /// </summary>
        /// <param name="luckyName">幸运人名称</param>
        private void showLuckyName(string luckyName)
        {
            listBox1.Items.Add("Lucky!!!" + luckyName + ":" + testPower.ToString() + "=>" + testDist.ToString());

            vp3d.Children.Add(draw3DText(luckyName, transform));

            Storyboard storyboard;
            DoubleAnimation doubleAnimation;

            storyboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(170, 35, new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, sliderFiledOfView);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Slider.ValueProperty));
            storyboard.Children.Add(doubleAnimation);

            storyboard.Begin();
        }

        /// <summary>
        /// 获取幸运圆饼的索引号
        /// </summary>
        /// <param name="distanceAngle">圆饼走过的距离(角度值)</param>
        /// <returns>转动了 distanceAngle 角度值后的幸运圆饼的索引号</returns>
        private int GetLuckyPieIndex(double distanceAngle)
        {
            int index = -1;

            for (int i = 0; i < employeePies.Count; i++)
            {
                PiePieceElement pp = employeePies[i];
                double startAngle = (Math.Round(pp.RotationAngle, 8) + distanceAngle) % 360;//获取开始角度及舍入其误差

                if ((startAngle > 90 && startAngle + pp.WedgeAngle > 450)
                    || (startAngle <= 90 && startAngle + pp.WedgeAngle > 90))
                {
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// 移出被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void pushOutPie(int index)
        {            
            Storyboard pushOutStoryboard;
            DoubleAnimation doubleAnimation;

            pushOutStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(0, employeePies[index].Radius - employeePies[index].InnerRadius + 20, 
                new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, employeePies[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.PushOutProperty));
            pushOutStoryboard.Children.Add(doubleAnimation);

            pushOutStoryboard.Begin();            
        }

        /// <summary>
        /// 缩小被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void smallPie(int index)
        {
            Storyboard smallerStoryboard;
            DoubleAnimation doubleAnimation;

            smallerStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(employeePies[index].Radius, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, employeePies[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RadiusProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            doubleAnimation = new DoubleAnimation(employeePies[index].InnerRadius, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, employeePies[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.InnerRadiusProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            doubleAnimation = new DoubleAnimation(employeePies[index].CentreX, employeePies[index].CentreX + employeePies[index].Radius + 20, 
                new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, employeePies[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.CentreXProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            smallerStoryboard.Completed += new EventHandler(smallerStoryboard_Completed);

            smallerStoryboard.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            smallerStoryboard.Begin();
        }

        void smallerStoryboard_Completed(object sender, EventArgs e)
        {
            if (selectPieIndex >= 0)
            {
                removePie(selectPieIndex);
                if (employeePies.Count == 1)
                {
                    smallPie(0);
                }
                else
                {
                    //当圆饼数量大于1个时，重新构建转盘。
                    reBuildRotorPlate(employeePies, selectPieIndex);
                }
            }
        }

        /// <summary>
        /// 移除被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void removePie(int index)
        {
            grid1.Children.Remove(employeePies[index]);
            employeePies.RemoveAt(index);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            /*
            Storyboard storyboard;
            DoubleAnimation doubleAnimation;

            storyboard = new Storyboard();
            for (int i = 0; i < myPies.Count; i++)
            {

                doubleAnimation = new DoubleAnimation(myPies[i].RotationAngle, myPies[i].RotationAngle + 510, new Duration(TimeSpan.FromMilliseconds(5000)));
                Storyboard.SetTarget(doubleAnimation, myPies[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RotationAngleProperty));
                storyboard.Children.Add(doubleAnimation);
            }

            storyboard.Begin();
            */


            luckyTime();
            
        }

        /// <summary>
        /// 开始抽奖
        /// </summary>
        private void luckyTime()
        {
            while (vp3d.Children.Count > 1)
            {
                vp3d.Children.RemoveAt(1);
            }
            //if (reBuildStoryboard != null)
            //{
            //    reBuildStoryboard.Remove(grid1);
            //    reBuildStoryboard = null;
            //}
            //for (int i = 0; i < myPies.Count; i++)
            //{
            //    myPies[i].BeginAnimation(PiePieceElement.RotationAngleProperty, null);
            //    myPies[i].BeginAnimation(PiePieceElement.WedgeAngleProperty, null);
            //}

            int seed = unchecked((int)(DateTime.Now.Ticks));
            Random rd = new Random(seed);
            testDist = 0;//测试
            power = rd.NextDouble() * 0.87846102021178 + 1.199;//1.199;// 1.1990000000000003;// 
            testPower += 0.002;//测试
            listBox1.Items.Add(getDistanceAngle(power).ToString());
            listBox1.Items.Add(GetLuckyPieIndex(getDistanceAngle(power)));

            dispatcherTimer.Tick += new EventHandler(CompositionTarget_Rendering);

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(5); //重复间隔

            dispatcherTimer.Start();
            this.IsEnabled = false;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != transform)
            {
                AxisAngleRotation3D r = (transform.Rotation as AxisAngleRotation3D);
                r.Angle = e.NewValue;
                labelAngle.Content = e.NewValue;
            }
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != dirLight)
            {
                dirLight.Direction = new Vector3D(e.NewValue, dirLight.Direction.Y, dirLight.Direction.Z);
                labelX.Content = e.NewValue.ToString();
            }
        }

        private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != dirLight)
            {
                dirLight.Direction = new Vector3D(dirLight.Direction.X, e.NewValue, dirLight.Direction.Z);
                labelY.Content = e.NewValue.ToString();
            }
        }

        private void slider4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != dirLight)
            {
                dirLight.Direction = new Vector3D(dirLight.Direction.X, dirLight.Direction.Y, e.NewValue);
                labelZ.Content = e.NewValue.ToString();
            }
        }

        private void sliderFiledOfView_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != camera)
            {
                camera.FieldOfView = e.NewValue;
                labelFieldOfView.Content = e.NewValue.ToString();
            }
        }

        /// <summary>
        /// 放烟花
        /// </summary>
        private void playFireworks()
        {
            Random r = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < 10; i++)
            {
                int x = r.Next(-(int)this.Width / 2, (int)this.Width / 2);
                int y = r.Next(-(int)this.Height / 2, 0);
                StackWindow sw = new StackWindow(x, y);
                sw.Height = 10;
                sw.Width = 10;
                grid1.Children.Add(sw);
                sw.Play();
            }
        }

        /// <summary>
        /// 预测转动距离
        /// </summary>
        /// <param name="power">转盘初始量(角度)</param>
        /// <returns>总的转动距离(角度)</returns>
        private double getDistanceAngle(double power)
        {
            double tmpPower = power;
            double tmpAngle = 0;

            while (tmpPower > 0)
            {
                tmpAngle += tmpPower;// 测试转动距离总量
                tmpPower -= drap;
            }

            return tmpAngle;
        }
    }
}
