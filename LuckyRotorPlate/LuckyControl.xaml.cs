using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using LuckyRotorPlate.DataService;
using LuckyRotorPlate.Model;
using NKLogic.UIElements;

namespace LuckyRotorPlate
{
    /// <summary>
    /// LuckyControl.xaml 的交互逻辑
    /// </summary>
    public partial class LuckyControl : UserControl
    {
        private double ROTOR_PLATE_PADING = 10;//圆盘（距离窗体）的边距
        private double pieThickness = 90;//转盘的厚度
        private double minPower = 3;//转盘初始量最小值
        private double powerRange = 1.2;//0.72;//转盘初始量范围(初始量最大值减去初始量最小值)
        private double drap = 0.002;//转盘阻力系数
        private double dispatcherTime = 10;//刷新频率(毫秒/次)
        private double theRate = 1;//与标准刷新频率(10毫秒/次)的比率
        private double angleSouce = 20;//转过该角度则发声(打一下鼓)
        double centreX;//圆盘中心X
        double centreY;//圆盘中心Y

        private SoundPlayer movingSoundPlayer = new SoundPlayer();  //移动声音播放器
        private SoundPlayer fireSoundPlayer = new SoundPlayer();  //烟花声音播放器
        private SoundPlayer luckySoundPlayer = new SoundPlayer();  //中奖声音播放器
        private SoundPlayer readySoundPlayer = new SoundPlayer();  //"Ready"声音播放器
        private SoundPlayer goSoundPlayer = new SoundPlayer();  //"go"声音播放器
        private string readyText = "Ready";
        private string goText = "GO";
        private String rotorPlateImgPath = "img/RotorPlateBackImg.png";
        private Image imgRotor;
        private String luckyImgPath = "img/LuckyPointImg.png";
        private Image imgLucky;
        private Brush luckyPiontColor = Brushes.Black;
        private Brush pieColor1 = Brushes.Yellow;
        private Brush pieColor2 = Brushes.Red;
        private Brush pieColor3 = Brushes.Blue;
        private string pieFontFamily = "宋体";
        private Brush pieFontColor = Brushes.Black;
        private Brush pieBorderColor = Brushes.LightGray;
        private Brush giftColor = Brushes.Red;
        private string giftFontFamily = "华文行楷";
        private Brush playerColor = Brushes.Blue;
        private string playerFontFamily = "宋体";

        private Solution solution;
        private ObservableCollection<RotorPlate> rotorPlates;
        private double valuePerTimes = 1;//每抽(转)一次，中奖的圆饼移除多少Value。
        //private int loopLuckyTimes = 1;//重复抽(转)次数.
        private int luckyTimes = 0;//第几次抽奖
        private Brush highLightColor = Brushes.Yellow;

        private RotorPlate curRotorPlate;//当前转动的转盘
        private int curPieSelecteId;//当前被选中的圆饼
        private Fireworks fw;//烟花
        private ShowWord swGift1;//显示中奖结果的3D字符
        private ShowWord swGift2;//显示中奖结果的3D字符
        private ShowWord swPlayer1;//显示抽奖项目的3D字符
        private ShowWord swPlayer2;//显示抽奖项目的3D字符
    
        private double power = 1.199;//转盘初始量
        private Point luckyPoint;
        private Polygon luckyPolygon;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        /// <summary>
        /// 抽取了一个幸运人事件
        /// </summary>
        public event EventHandler GetingResult;
        /// <summary>
        /// 抽奖结束(已完成所有抽奖项目)事件
        /// </summary>
        public event EventHandler StepCompleted;
        /// <summary>
        /// 所有抽奖（方案）结束
        /// </summary>
        private event EventHandler SolutionCompleted;

        /// <summary>
        /// 圆心X坐标
        /// </summary>
        public double CentreX
        {
            get { return centreX; }
            private set { centreX = value; }
        }

        /// <summary>
        /// 圆心Y坐标
        /// </summary>
        public double CentreY
        {
            get { return centreY; }
            private set { centreY = value; }
        }

        /// <summary>
        /// 刷新频率(毫秒/次)
        /// </summary>
        public double DispatcherTime
        {
            get { return dispatcherTime; }
            set 
            { 
                dispatcherTime = value;
                theRate = value / 10;
            }
        }

        /// <summary>
        /// 当前转盘对象
        /// </summary>
        public RotorPlate CurRotorPlate
        {
            get { return curRotorPlate; }
            private set { curRotorPlate = value; }
        }

        protected void OnGetingResult()
        {
            EventHandler handler = GetingResult;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void OnStepCompleted()
        {
            EventHandler handler = StepCompleted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void OnSolutionCompleted()
        {
            EventHandler handler = SolutionCompleted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public Brush HighLightColor
        {
            get { return highLightColor; }
            set { highLightColor = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="suo">待加载解决方案</param>
        public LuckyControl(Solution suo)
            : this(suo, 3, false)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="suo">待加载解决方案</param>
        /// <param name="isTest">是否显示测试结果</param>
        /// <param name="dp">阻力系数(默认为0.002)</param>
        public LuckyControl(Solution suo, double minP, bool isTest)
        {
            InitializeComponent();

            if (isTest)
            {
                lbTest.Visibility = Visibility.Visible;
            }
            else
            {
                lbTest.Visibility = Visibility.Hidden;
            }

            minPower = minP;

            initConfig();

            init3DViewPort();

            solution = suo;
            LoadSolution();

            this.drap = suo.Drap;
            powerRange = 360 / minPower * drap;
        }

        /// <summary>
        /// 加载方案
        /// </summary>
        public void LoadSolution()
        {
            rotorPlates = new ObservableCollection<RotorPlate>();
            for (int i = 0; i < solution.StepList.Count; i++)
            {
                RotorPlate rp = new RotorPlate();
                rp.Step = solution.StepList[i];
                rotorPlates.Add(rp);
            }

            //绘制
            //initControl(this.Width, this.Height);
        }

        private void initConfig()
        {
            //加载配置
            BrushConverter bc = new BrushConverter();
            AppSettingsSection appSettings = ((LuckyRotorPlateApp)Application.Current).AppSettings;
            pieThickness = null != appSettings.Settings["PieThickness"] ? 
                double.Parse(appSettings.Settings["PieThickness"].Value) : pieThickness;
            luckyPiontColor = null != appSettings.Settings["LuckyPiontColor"] ?
                bc.ConvertFromString(appSettings.Settings["LuckyPiontColor"].Value) as Brush : luckyPiontColor;
            pieColor1 = null != appSettings.Settings["PieColor1"] ? 
                bc.ConvertFromString(appSettings.Settings["PieColor1"].Value) as Brush : pieColor1;
            pieColor2 = null != appSettings.Settings["PieColor2"] ? 
                bc.ConvertFromString(appSettings.Settings["PieColor2"].Value) as Brush : pieColor2;
            pieColor3 = null != appSettings.Settings["PieColor3"] ?
                bc.ConvertFromString(appSettings.Settings["PieColor3"].Value) as Brush : pieColor3;
            pieFontFamily = null != appSettings.Settings["PieFontFamily"] ?
                appSettings.Settings["PieFontFamily"].Value : pieFontFamily;
            pieFontColor = null != appSettings.Settings["PieFontColor"] ? 
                bc.ConvertFromString(appSettings.Settings["PieFontColor"].Value) as Brush : pieFontColor;
            pieBorderColor = null != appSettings.Settings["PieBorderColor"] ? 
                bc.ConvertFromString(appSettings.Settings["PieBorderColor"].Value) as Brush : pieBorderColor;
            giftColor = null != appSettings.Settings["GiftColor"] ?
                bc.ConvertFromString(appSettings.Settings["GiftColor"].Value) as Brush : giftColor;
            playerColor = null != appSettings.Settings["PlayerColor"] ? 
                bc.ConvertFromString(appSettings.Settings["PlayerColor"].Value) as Brush : playerColor;
            giftFontFamily = null != appSettings.Settings["GiftFontFamily"] ? 
                appSettings.Settings["GiftFontFamily"].Value : giftFontFamily;
            playerFontFamily = null != appSettings.Settings["PlayerFontFamily"] ? 
                appSettings.Settings["PlayerFontFamily"].Value : playerFontFamily;
            imgRotor = new Image();
            rotorPlateImgPath = null != appSettings.Settings["RotorPlateImg"] ? 
                appSettings.Settings["RotorPlateImg"].Value : rotorPlateImgPath;
            Uri uri = new Uri(rotorPlateImgPath, UriKind.Relative);
            imgRotor.Source = new BitmapImage(uri);
            imgLucky = new Image();
            luckyImgPath = null != appSettings.Settings["LuckyPointImg"] ? 
                appSettings.Settings["LuckyPointImg"].Value : luckyImgPath;
            imgLucky.Source = new BitmapImage(new Uri(luckyImgPath, UriKind.Relative));

            if (null != appSettings.Settings["MovingSound"])
            {
                this.movingSoundPlayer.SoundLocation = appSettings.Settings["MovingSound"].Value;
                this.movingSoundPlayer.Load();
            }
            else
            {
                this.movingSoundPlayer.SoundLocation = @"C:\Windows\Media\chimes.wav";
            }
            if (null != appSettings.Settings["FireSound"])
            {
                this.fireSoundPlayer.SoundLocation = appSettings.Settings["FireSound"].Value;
            }
            if (null != appSettings.Settings["LuckySound"])
            {
                this.luckySoundPlayer.SoundLocation = appSettings.Settings["LuckySound"].Value;
            }
            if (null != appSettings.Settings["ReadySound"])
            {
                this.readySoundPlayer.SoundLocation = appSettings.Settings["ReadySound"].Value;
            }
            if (null != appSettings.Settings["GoSound"])
            {
                this.goSoundPlayer.SoundLocation = appSettings.Settings["GoSound"].Value;
            }
            if (null != appSettings.Settings["ReadyText"])
            {
                readyText = appSettings.Settings["ReadyText"].Value;
            }
            if (null != appSettings.Settings["GoText"])
            {
                goText = appSettings.Settings["GoText"].Value;
            }

            fw = new Fireworks(0, 0, 20);
            swGift1 = new ShowWord(giftFontFamily, giftColor, giftColor);
            swGift2 = new ShowWord(giftFontFamily, giftColor, giftColor);
            swPlayer1 = new ShowWord(playerFontFamily, playerColor, playerColor);
            swPlayer2 = new ShowWord(playerFontFamily, playerColor, playerColor);
        }

        private void initControl(double width, double height)
        {
            for (int i = 0; i < rotorPlates.Count; i++)
            {
                //重绘转盘
                double radius = Math.Min(width, height) / 2 - i * pieThickness - ROTOR_PLATE_PADING * 2;//圆半径
                initRotorPlate(rotorPlates[i], width, height, radius, pieThickness);
            }
            //中奖点
            initLuckyPiont(width, height);

            //圆盘中心图
            initRotorPlateImg(width, height);

            //烟花
            fw.Width = width;
            fw.Height = height;
            fw.PlayEnd += new EventHandler(sw_PlayCompleted);
            if (grid.Children.Contains(fw))
            {
                grid.Children.Remove(fw);
            }
            this.grid.Children.Add(fw);

            //显示3d字符
            swPlayer1.Width = centreX * 2;
            swPlayer1.Height = swPlayer1.Width * 0.1;
            swPlayer1.Margin = new Thickness(ROTOR_PLATE_PADING, centreY - swPlayer1.Width * 0.17 + swPlayer1.Height * 1, 0, 0);
            swPlayer1.HorizontalAlignment = HorizontalAlignment.Left;
            swPlayer1.VerticalAlignment = VerticalAlignment.Top;
            if (grid.Children.Contains(swPlayer1))
            {
                grid.Children.Remove(swPlayer1);
            }
            grid.Children.Add(swPlayer1);

            swGift1.Width = centreX * 2;
            swGift1.Height = swGift1.Width * 0.1;
            swGift1.Margin = new Thickness(ROTOR_PLATE_PADING, centreY - swGift1.Width * 0.17 + swPlayer1.Height * 2, 0, 0);
            swGift1.HorizontalAlignment = HorizontalAlignment.Left;
            swGift1.VerticalAlignment = VerticalAlignment.Top;
            if (grid.Children.Contains(swGift1))
            {
                grid.Children.Remove(swGift1);
            }
            grid.Children.Add(swGift1);

            swPlayer2.Width = centreX * 2;
            swPlayer2.Height = swPlayer2.Width * 0.1;
            swPlayer2.Margin = new Thickness(ROTOR_PLATE_PADING, centreY - swPlayer2.Width * 0.17 + swPlayer1.Height * 3, 0, 0);
            swPlayer2.HorizontalAlignment = HorizontalAlignment.Left;
            swPlayer2.VerticalAlignment = VerticalAlignment.Top;
            if (grid.Children.Contains(swPlayer2))
            {
                grid.Children.Remove(swPlayer2);
            }
            grid.Children.Add(swPlayer2);

            swGift2.Width = centreX * 2;
            swGift2.Height = swGift2.Width * 0.1;
            swGift2.Margin = new Thickness(ROTOR_PLATE_PADING, centreY - swGift2.Width * 0.17 + swPlayer1.Height * 4, 0, 0);
            swGift2.HorizontalAlignment = HorizontalAlignment.Left;
            swGift2.VerticalAlignment = VerticalAlignment.Top;
            if (grid.Children.Contains(swGift2))
            {
                grid.Children.Remove(swGift2);
            }
            grid.Children.Add(swGift2);

            //
            //if (gdViewPort3D.Children.Contains(vp3d))
            //{
            //    gdViewPort3D.Children.Remove(vp3d);
            //}
            //gdViewPort3D.Children.Add(vp3d);
            //if (gdViewPort3D.Children.Contains(vp3dPlayer))
            //{
            //    gdViewPort3D.Children.Remove(vp3dPlayer);
            //}
            //gdViewPort3D.Children.Add(vp3dPlayer);
        }

        private void initRotorPlateImg(double width, double height)
        {
            imgRotor.Width = (Math.Min(width, height) / 2 - rotorPlates.Count * pieThickness - ROTOR_PLATE_PADING * 2) * 2;
            imgRotor.Height = imgRotor.Width;
            imgRotor.VerticalAlignment = VerticalAlignment.Top;
            imgRotor.HorizontalAlignment = HorizontalAlignment.Left;
            double pading = rotorPlates.Count * pieThickness + ROTOR_PLATE_PADING;
            imgRotor.Margin = new Thickness(pading, pading, 0, 0);
            if (grid.Children.Contains(imgRotor))
            {
                grid.Children.Remove(imgRotor);
            }
            grid.Children.Add(imgRotor);
        }

        /// <summary>
        /// 初始化转盘
        /// </summary>
        /// <param name="rp">要初始化的转变对象</param>
        /// <param name="width">转盘所在布局的宽</param>
        /// <param name="height">转盘所在布局的高</param>
        /// <param name="plateWidth">转盘的厚度</param>
        private void initRotorPlate(RotorPlate rp, double width, double height, double radius, double plateThickNess)
        {

            if (rp.Count > 0)
            {
                for (int i = 0; i < rp.Count; i++)
                {
                    grid.Children.Remove(rp[i]);
                }
                for (int i = 0; i < rp.Count; i++)
                {
                    rp[i].Radius = radius;
                    rp[i].InnerRadius = radius - pieThickness;
                    rp[i].CentreX = centreX;
                    rp[i].CentreY = centreY;
                    grid.Children.Add(rp[i]);
                }
                return;
            }

            ObservableCollection<RealThing> employeeList = new ObservableCollection<RealThing>();
            foreach (RealThing t in rp.Step.Pies)
            {
                employeeList.Add(t.Clone() as RealThing);
            }
            //移除已中奖的
            foreach (RealThing rst in rp.Step.Players)
            {
                foreach (RealThing pie in employeeList)
                {
                    if (!String.IsNullOrEmpty(rst.ObtainedGift) && rst.ObtainedGift.Equals(pie.Name))
                    {
                        pie.Value -= rst.Value;
                    }
                }
            }
            for (int i = employeeList.Count; i > 0; i--)
            {
                if (employeeList[i - 1].Value <= 0)
                {
                    employeeList.RemoveAt(i - 1);
                }
            }

            //计算参数
            double allValue = 0;
            for (int i = 0; i < employeeList.Count; i++)
            {
                allValue += employeeList[i].Value;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;

            //构造转盘
            for (int i = 0; i < employeeList.Count; i++)
            {
                PiePieceElement pp;
                if (i % 3 == 0 && i != (employeeList.Count - 1))
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - plateThickNess, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor1, pieFontColor, pieFontFamily, true);
                }
                else if (i % 3 == 2)
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - plateThickNess, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor2, pieFontColor, pieFontFamily, true);
                }
                else
                {
                    pp = new PiePieceElement(employeeList[i].Name, radius, radius - plateThickNess, 0, perAngle * employeeList[i].Value,
                        endAngle, centreX, centreY, employeeList[i].Value, pieBorderColor, pieColor3, pieFontColor, pieFontFamily, true);
                }
                endAngle += perAngle * employeeList[i].Value;

                rp.Add(pp);
                grid.Children.Add(pp);
            }
        }

        /// <summary>
        /// 初始化中奖的点
        /// </summary>
        private void initLuckyPiont(double width, double height)
        {
            double radius = Math.Min(width, height) / 2 - ROTOR_PLATE_PADING;//圆半径
            luckyPoint = new Point(centreX + radius - ROTOR_PLATE_PADING, centreY);
            buildLuckyImg(width, height);
        }

        private void buildLuckyImg(double windowWidth, double windowHeight)
        {
            double rate = (imgLucky.Source as BitmapImage).Width / (imgLucky.Source as BitmapImage).Height;
            if (windowHeight / (imgLucky.Source as BitmapImage).Height > (windowWidth - luckyPoint.X) / (imgLucky.Source as BitmapImage).Width)
            {
                //以宽为标准
                imgLucky.Width = Math.Min(windowWidth - luckyPoint.X, (imgLucky.Source as BitmapImage).Width) * 1.5;
                imgLucky.Height = imgLucky.Width / rate;
            }
            else
            {
                //以高为标准
                imgLucky.Height = Math.Min(windowHeight, (imgLucky.Source as BitmapImage).Height);
                imgLucky.Width = imgLucky.Height * rate;
            }
            //imgLucky.Stretch = Stretch.None;
            imgLucky.VerticalAlignment = VerticalAlignment.Top;
            imgLucky.HorizontalAlignment = HorizontalAlignment.Left;
            imgLucky.Margin = new Thickness(luckyPoint.X - imgLucky.Width / 2, luckyPoint.Y - imgLucky.Height / 2, 0, 0);
            if (grid.Children.Contains(imgLucky))
            {
                grid.Children.Remove(imgLucky);
            }
            grid.Children.Add(imgLucky);
        }

        private void buildLuckyPolygon()
        {
            if (grid.Children.Contains(luckyPolygon))
            {
                grid.Children.Remove(luckyPolygon);
            }
            luckyPolygon = new Polygon();
            luckyPolygon.Fill = luckyPiontColor;
            Point Point1 = luckyPoint;
            Point Point2 = new Point(luckyPoint.X + 50, luckyPoint.Y - ROTOR_PLATE_PADING);
            Point Point3 = new Point(luckyPoint.X + 50, luckyPoint.Y + ROTOR_PLATE_PADING);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            luckyPolygon.Points = myPointCollection;
            grid.Children.Add(luckyPolygon);
        }

        /// <summary>
        /// 初始化3D场景
        /// </summary>
        private void init3DViewPort()
        {
            //double sliderAngle = 10.0;
            //double sliderX = 1.5;
            //double sliderY = -0.15;
            //double sliderZ = -4;
            //sliderField.Value = 50; 

            //dirLight = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(sliderX, sliderY, sliderZ));
            //transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, -1, 0), sliderAngle));
            //camera = new PerspectiveCamera(new Point3D(-1, -0.2, 8),
            //    new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), sliderField.Value);

            //ModelVisual3D mv3d = new ModelVisual3D();
            //Model3DGroup m3g = new Model3DGroup();
            //m3g.Children.Add(new AmbientLight(Color.FromRgb(0x40, 0x40, 0x40)));
            //m3g.Children.Add(dirLight);
            //mv3d.Content = m3g;

            //vp3d = new Viewport3D();
            //vp3d.Camera = camera;
            //vp3d.Children.Add(mv3d);

            //cameraPlayer = new PerspectiveCamera(new Point3D(-1, -0.2, 8),
            //    new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), sliderField.Value);
            //ModelVisual3D mv3dPlayer = new ModelVisual3D();
            //Model3DGroup m3gPlayer = new Model3DGroup();
            //m3gPlayer.Children.Add(new AmbientLight(Color.FromRgb(0x40, 0x40, 0x40)));
            //m3gPlayer.Children.Add(dirLight);
            //mv3dPlayer.Content = m3gPlayer;
            //vp3dPlayer = new Viewport3D();
            //vp3dPlayer.Camera = cameraPlayer;
            //vp3dPlayer.Children.Add(mv3dPlayer);
        }

        /// <summary>
        /// 展示幸运人
        /// </summary>
        /// <param name="luckyName">幸运人名称</param>
        private void addLuckyName(String luckyName)
        {
            //gift = draw3DText(luckyName.Trim(), transform, -0.2 - ((vp3d.Children.Count - 1) * 2));
            //vp3d.Children.Add(gift);

            //showLuckyName();
            if (curRotorPlate.Count == 1)
            {
                swGift2.Show(luckyName, 1000);
            }
            else
            {
                swGift1.Show(luckyName, 1000);
                swGift1.ShowEnd += new EventHandler(swGift_ShowEnd);
            }
        }

        void swGift_ShowEnd(object sender, EventArgs e)
        {
            playFireworks();
            (sender as ShowWord).ShowEnd -= swGift_ShowEnd;
        }

        private void showLuckyName()
        {
            //Storyboard showNameStoryboard;
            //DoubleAnimation doubleAnimation;

            //showNameStoryboard = new Storyboard();

            //doubleAnimation = new DoubleAnimation(170, 50, new Duration(TimeSpan.FromMilliseconds(1000)));
            //Storyboard.SetTarget(doubleAnimation, sliderField);
            //Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Slider.ValueProperty));
            //showNameStoryboard.Children.Add(doubleAnimation);

            //showNameStoryboard.Completed += new EventHandler(showNameStoryboard_Completed);
            //showNameStoryboard.Begin();

            try
            {
                this.luckySoundPlayer.Play();
            }
            catch
            {
            }
        }

        void showNameStoryboard_Completed(object sender, EventArgs e)
        {
            //
            playFireworks();
        }

        /// <summary>
        /// 获取幸运圆饼的索引号
        /// </summary>
        /// <param name="distanceAngle">圆饼走过的距离(角度值)</param>
        /// <returns>转动了 distanceAngle 角度值后的幸运圆饼的索引号</returns>
        private int GetLuckyPieIndex(RotorPlate rp, double distanceAngle)
        {
            int index = -1;

            for (int i = 0; i < rp.Count; i++)
            {
                PiePieceElement pp = rp[i];
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
        /// 移除某圆饼后重新绘制圆（动画）
        /// </summary>
        /// <param name="removeIndex">被移除的圆饼</param>
        private void reBuildRotorPlate(RotorPlate rp, int removeIndex)
        {
            double allValue = 0;
            for (int i = 0; i < rp.Count; i++)
            {
                allValue += rp[i].PieceValue;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;

            #region 变换动画
            DoubleAnimation doubleAnimation;
            Storyboard reBuildStoryboard = new Storyboard();
            reBuildStoryboard.FillBehavior = FillBehavior.Stop;

            for (int i = removeIndex; i < rp.Count; i++)
            {
                doubleAnimation = new DoubleAnimation(rp[i].WedgeAngle, perAngle * rp[i].PieceValue,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, rp[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.WedgeAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimation(rp[i].RotationAngle, 90 + endAngle,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, rp[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RotationAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                endAngle += perAngle * rp[i].PieceValue;
            }

            endAngle = 0;
            for (int i = removeIndex - 1; i >= 0; i--)
            {
                endAngle += perAngle * rp[i].PieceValue;

                doubleAnimation = new DoubleAnimation(rp[i].WedgeAngle, perAngle * rp[i].PieceValue,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, rp[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.WedgeAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimation(rp[i].RotationAngle, (90 - endAngle + 360) % 360,
                    new Duration(TimeSpan.FromMilliseconds(500)));
                Storyboard.SetTarget(doubleAnimation, rp[i]);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RotationAngleProperty));
                reBuildStoryboard.Children.Add(doubleAnimation);
            }

            reBuildStoryboard.Completed += new EventHandler(reBuildStoryboard_Completed);
            reBuildStoryboard.Begin();
            #endregion
        }

        private void reBuildStoryboard_Completed(object sender, EventArgs e)
        {
            //移除某圆饼后重新绘制圆（非动画）
            double allValue = 0;
            for (int i = 0; i < curRotorPlate.Count; i++)
            {
                allValue += curRotorPlate[i].PieceValue;
            }
            double perAngle = 360.0 / allValue;
            double endAngle = 0;

            for (int i = curPieSelecteId; i < curRotorPlate.Count; i++)
            {
                curRotorPlate[i].WedgeAngle = perAngle * curRotorPlate[i].PieceValue;
                curRotorPlate[i].RotationAngle = 90 + endAngle;

                endAngle += perAngle * curRotorPlate[i].PieceValue;
            }

            endAngle = 0;
            for (int i = curPieSelecteId - 1; i >= 0; i--)
            {
                endAngle += perAngle * curRotorPlate[i].PieceValue;

                curRotorPlate[i].WedgeAngle = perAngle * curRotorPlate[i].PieceValue;
                curRotorPlate[i].RotationAngle = (90 - endAngle + 360) % 360;
            }

            //OnGetingResult();

        }

        /// <summary>
        /// 重新计算权值
        /// </summary>
        /// <param name="pieId"></param>
        private void ReduceWeight(RotorPlate rp, int pieId)
        {
            rp[pieId].PieceValue -= valuePerTimes;
            if (rp[pieId].PieceValue <= 0)
            {
                //当权值小于0时，移除该圆饼
                pushOutPie(rp, pieId);
                smallPie(rp, pieId);
            }
            //重新构建转盘。
            reBuildRotorPlate(rp, pieId);
        }

        /// <summary>
        /// 移出被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void pushOutPie(RotorPlate rp, int index)
        {
            Storyboard pushOutStoryboard;
            DoubleAnimation doubleAnimation;

            pushOutStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(0, rp[index].Radius - rp[index].InnerRadius + 20,
                new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, rp[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.PushOutProperty));
            pushOutStoryboard.Children.Add(doubleAnimation);

            pushOutStoryboard.Begin();
        }

        /// <summary>
        /// 缩小被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void smallPie(RotorPlate rp, int index)
        {
            Storyboard smallerStoryboard;
            DoubleAnimation doubleAnimation;

            smallerStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(rp[index].Radius, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, rp[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.RadiusProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            doubleAnimation = new DoubleAnimation(rp[index].InnerRadius, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, rp[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.InnerRadiusProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            doubleAnimation = new DoubleAnimation(rp[index].CentreX, 
                rp[index].CentreX + rp[index].Radius + ROTOR_PLATE_PADING * 2,
                new Duration(TimeSpan.FromMilliseconds(500)));
            Storyboard.SetTarget(doubleAnimation, rp[index]);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(PiePieceElement.CentreXProperty));
            smallerStoryboard.Children.Add(doubleAnimation);

            smallerStoryboard.Completed += new EventHandler(smallerStoryboard_Completed);

            smallerStoryboard.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            smallerStoryboard.Begin();
        }

        private void smallerStoryboard_Completed(object sender, EventArgs e)
        {
            if (curPieSelecteId >= 0)
            {
                removePie(curRotorPlate, curPieSelecteId);
                if (curRotorPlate.Count == 1)
                {
                    curPieSelecteId = 0;
                    saveLuckyResult(curRotorPlate[0].PieName);
                    smallPie(curRotorPlate, 0);
                }
            }
        }

        /// <summary>
        /// 移除被选中的圆饼
        /// </summary>
        /// <param name="index">圆饼索引</param>
        private void removePie(RotorPlate rp, int index)
        {
            grid.Children.Remove(rp[index]);
            rp.RemoveAt(index);
        }

        /// <summary>
        /// 准备开始抽奖(显示当前抽奖项目)
        /// </summary>
        public void Ready()
        {
            curRotorPlate = rotorPlates[luckyTimes % rotorPlates.Count];
            showPlayer();
        }

        /// <summary>
        /// 显示当前抽奖项目
        /// </summary>
        private void showPlayer()
        {
            string showText = string.Empty;

            if (curRotorPlate.CurPlayer != null)
            {
                showText = curRotorPlate.CurPlayer.Name;
            }

            swPlayer1.Show(showText, 500);

            double value = 0;
            foreach(PiePieceElement p in curRotorPlate)
            {
                value += p.PieceValue;
            }
            if (value <= 2)
            {
                //
                swPlayer2.Show(curRotorPlate.Step.Players[curRotorPlate.Step.Players.Count - 1].Name, 500);            
            }
        }

        /// <summary>
        /// 开始抽奖
        /// </summary>
        public void LuckyTime()
        {
            curRotorPlate = rotorPlates[luckyTimes % rotorPlates.Count];
            if (curRotorPlate.Count <= 0)
            {
                OnGetingResult();
                OnStepCompleted();
                return;
            }

            showPlayer();
            swPlayer1.ShowEnd += new EventHandler(swPlayer1_ShowEnd);
        }

        void swPlayer1_ShowEnd(object sender, EventArgs e)
        {
            swGift1.ShowEnd += new EventHandler(sw_ready_ShowEnd);
            swGift1.HideEnd += new EventHandler(sw_ready_HideEnd);
            swGift1.Show(readyText, 300);//显示 Ready 3D 字符
            swPlayer1.ShowEnd -= swPlayer1_ShowEnd;
        }

        #region 显示 3D 字符 事件
        void sw_ready_ShowEnd(object sender, EventArgs e)
        {
            try
            {
                this.readySoundPlayer.Play();
            }
            catch
            {
            }

            swGift1.Hide(3000);
        }

        void sw_ready_HideEnd(object sender, EventArgs e)
        {
            swGift1.ShowEnd -= sw_ready_ShowEnd;
            swGift1.HideEnd -= sw_ready_HideEnd;
            swGift1.ShowEnd += new EventHandler(sw_go_ShowEnd);
            swGift1.HideEnd += new EventHandler(sw_go_HideEnd);
            swGift1.Show(goText, 300);
        }

        void sw_go_ShowEnd(object sender, EventArgs e)
        {
            try
            {
                this.goSoundPlayer.PlaySync();
            }
            catch
            {
            }

            swGift1.Hide(900);

            startMove();
        }

        void sw_go_HideEnd(object sender, EventArgs e)
        {
            swGift1.ShowEnd -= sw_go_ShowEnd;
            swGift1.HideEnd -= sw_go_HideEnd;
        }
        #endregion

        /// <summary>
        /// 开始转动转盘
        /// </summary>
        private void startMove()
        {
            power = getRandomPower();

            dispatcherTimer.Tick += new EventHandler(CompositionTarget_Rendering);

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(dispatcherTime); //重复间隔

            dispatcherTimer.Start();

            luckyTimes++;

            playMusic();
        }

        /// <summary>
        /// 获取随机量
        /// </summary>
        /// <returns>随机量</returns>
        private double getRandomPower()
        {
            double rstPower;//随机量
            RealThing curPlayer = curRotorPlate.CurPlayer;//当前抽奖项目            

            int seed = unchecked((int)(DateTime.Now.Ticks));
            Random rd = new Random(seed);

            rstPower = rd.NextDouble() * powerRange + minPower;
            double distanceAngle = getDistanceAngle(rstPower);
            int id = GetLuckyPieIndex(curRotorPlate, distanceAngle);
            if (null != lbTest && lbTest.Visibility == Visibility.Visible)
            {
                lbTest.Items.Add("power:" + rstPower.ToString() 
                    + ";DistanceAngle:" + distanceAngle.ToString() + ";ID:" + id.ToString() + "。");
            }
            //预测是否是受限结果
            while (StepInfo.ThingsContainName(curRotorPlate[id].PieName, curRotorPlate.Step.ExcludePies)
                || null != curPlayer && StepInfo.ThingsContainName(curRotorPlate[id].PieName, curPlayer.ExcludeThing))
            {
                rstPower = rd.NextDouble() * powerRange + minPower;
                distanceAngle = getDistanceAngle(rstPower);
                id = GetLuckyPieIndex(curRotorPlate, distanceAngle);
                if (null != lbTest && lbTest.Visibility == Visibility.Visible)
                {
                    lbTest.Items.Add("power:" + rstPower.ToString() 
                        + ";DistanceAngle:" + distanceAngle.ToString() + ";ID:" + id.ToString() + "。");
                }
            }

            return rstPower;
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
                if (tmpPower > minPower)
                {
                    tmpAngle += minPower *theRate;  //在大于某下限值时，圆盘匀速转动。
                }
                else
                {
                    tmpAngle += tmpPower *theRate;// 测试转动距离总量
                }
                tmpPower -= drap *theRate;
            }

            return tmpAngle;
        }

        private Brush preFillColor;
        private PiePieceElement prePie;
        private int tmpPieIndex = -1;
        private double totalPower = 0;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            double lastPower = power;
            double tmpPower = power * theRate;

            if (power > 0)
            {
                if (null != preFillColor && null != prePie)
                {
                    //还原原来的颜色
                    prePie.Fill = preFillColor;
                }

                if (power > minPower)
                {
                    //在大于某下限值时，圆盘匀速转动。
                    tmpPower = minPower * theRate;
                }
                //改变所有圆饼的角度
                for (int i = 0; i < curRotorPlate.Count; i++)
                {
                    curRotorPlate[i].RotationAngle += tmpPower;
                }
                totalPower += tmpPower;

                //记录当前选中的圆饼ID
                int id = GetLuckyPieIndex(curRotorPlate, 0);
                if (tmpPieIndex != id)
                {
                    tmpPieIndex = id;
                }
                //高亮选中的圆盘
                prePie = curRotorPlate[tmpPieIndex];
                preFillColor = prePie.Fill;
                highlightPie(prePie);

                //每转过15度
                if (totalPower % angleSouce < tmpPower)
                {
                    //playMusic();
                }

                power -= drap * theRate;
            }
            if (lastPower > 0 && power <= 0)
            {

                dispatcherTimer.Tick -= CompositionTarget_Rendering;

                //show messge and save lucky result
                try
                {
                    curPieSelecteId = GetLuckyPieIndex(curRotorPlate, 0);
                    string luckyName = curRotorPlate[curPieSelecteId].PieName;

                    if (null != lbTest && lbTest.Visibility == Visibility.Visible)
                    {
                        lbTest.Items.Add("ID:" + curPieSelecteId.ToString() 
                            + ";Name:" + luckyName + ";TotalAngle:" + totalPower.ToString() + "。");
                        totalPower = 0;
                    }

                    saveLuckyResult(luckyName);

                    ReduceWeight(curRotorPlate, curPieSelecteId);

                    showLuckyName();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// 
        /// 播放音乐
        /// </summary>
        private void playMusic()
        {
            try
            {
                this.movingSoundPlayer.PlayLooping();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 高亮显示圆饼对象
        /// </summary>
        /// <param name="pie">圆饼对象</param>
        private void highlightPie(PiePieceElement pie)
        {
            pie.Fill = highLightColor;
        }

        /// <summary>
        /// 保存抽奖结果
        /// </summary>
        private void saveLuckyResult(String gift)
        {
            addLuckyName(gift);

            if (curRotorPlate.CurPlayer != null)
            {
                if (null != lbTest && lbTest.Visibility == Visibility.Visible)
                {
                    lbTest.Items.Add("保存中奖项目" + gift);
                }
                curRotorPlate.CurPlayer.ObtainedGift = gift;
                //保存xml文件
                SaveSolution();
            }
        }

        private static void SaveSolution()
        {
            ObservableCollection<Solution> solutions = ((LuckyRotorPlateApp)Application.Current).Solutions;
            AppSettingsSection appSettings = ((LuckyRotorPlateApp)Application.Current).AppSettings;
            XmlHelper.SaveSolutions(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["SolutionsData"].Value, solutions);
        }

        /// <summary>
        /// 放烟花
        /// </summary>
        private void playFireworks()
        {
            if (null != lbTest && lbTest.Visibility == Visibility.Visible)
            {
                lbTest.Items.Add("放烟花");
            }
            //completedCount = 0;
            //Random r = new Random(unchecked((int)DateTime.Now.Ticks));
            //for (int i = 0; i < fireworkCount; i++)
            //{
            //    int x = r.Next(-(int)grid.Width / 2, (int)grid.Width / 2);
            //    int y = r.Next(-(int)grid.Height / 2, 0);
            //    StackWindow sw = new StackWindow(x, y);
            //    sw.PlayCompleted += new EventHandler(sw_PlayCompleted);
            //    sw.Height = 10;
            //    sw.Width = 10;
            //    grid.Children.Add(sw);
            //    sw.Play();
            //}
            fw.Play();
            try
            {
                this.fireSoundPlayer.Play();
            }
            catch
            {
            }
        }

        void sw_PlayCompleted(object sender, EventArgs e)
        {
            OnGetingResult();
            //if (++completedCount >= fireworkCount)
            {
                if (curRotorPlate.CurPlayer == null)
                {
                    OnStepCompleted();
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            centreX = Math.Min(e.NewSize.Width, e.NewSize.Height) / 2 - ROTOR_PLATE_PADING;
            centreY = centreX;

            gdViewPort3D.Margin = new Thickness(ROTOR_PLATE_PADING, ROTOR_PLATE_PADING, 0, 0);
            gdViewPort3D.Width = centreX * 2;
            gdViewPort3D.Height = centreX * 2;

            grid.Width = e.NewSize.Width;
            grid.Height = e.NewSize.Height;
            if (rotorPlates != null && rotorPlates.Count > 0)
            {
                //重绘
                if (null != lbTest && lbTest.Visibility == Visibility.Visible)
                {
                    lbTest.Items.Add("重绘");
                }
                initControl(e.NewSize.Width, e.NewSize.Height);
            }
        }
    }

    public class RotorPlate : ObservableCollection<PiePieceElement>
    {
        private StepInfo step;

        public RealThing CurPlayer
        {
            get 
            {
                return getCurPlayer(); 
            }
        }

        internal StepInfo Step
        {
            get { return step; }
            set { step = value; }
        }

        private RealThing getCurPlayer()
        {
            RealThing curPlayer = null;//当前抽奖项目
            foreach (RealThing t in Step.Players)
            {
                if (String.IsNullOrEmpty(t.ObtainedGift))
                {
                    curPlayer = t;
                    break;
                }
            }

            return curPlayer;
        }
    }
}
