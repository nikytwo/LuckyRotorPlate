using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LuckyRotorPlate
{
    public partial class StackWindow : UserControl
    {
        private const int INIT_SIZE = 1;
        private const int MIN_SIZE = 150;
        private const int MAX_SIZE = 250;
        private const int DURATION_TIME = 1000;

        private int startTime = 6000;
        private double x;
        private double y;
        static private Random random = new Random();
        private Ellipse ellipse = null;
        private String fireworksPath = "img/Fireworks/";
        private int fireworksCount = 9;
        private Storyboard mystoryboard;

        public event EventHandler PlayCompleted;

        protected void OnPlayCompleted()
        {
            EventHandler handler = PlayCompleted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">烟花坐标X</param>
        /// <param name="y">烟花坐标Y</param>
        public StackWindow(double x, double y)
        {
            InitializeComponent();
            X = x;
            Y = y;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">烟花坐标X</param>
        /// <param name="y">烟花坐标Y</param>
        /// <param name="time">烟花持续时间</param>
        public StackWindow(double x, double y, int time)
        {
            InitializeComponent();
            X = x;
            Y = y;
            startTime = time;
        }

        /// <summary>
        /// 烟花燃放
        /// </summary>
        public void Play()
        {
            ellipse = new Ellipse();
            ImageBrush imagebrush = new ImageBrush();
            Uri uri = new Uri(fireworksPath + random.Next(1, fireworksCount) + ".png", UriKind.Relative);
            imagebrush.ImageSource = new BitmapImage(uri);
            ellipse.Fill = imagebrush;
            ellipse.Visibility = Visibility.Visible;
            ellipse.Width = INIT_SIZE;
            ellipse.Height = INIT_SIZE;
            ellipse.SetValue(Canvas.LeftProperty, this.X - ellipse.Width / 2);
            ellipse.SetValue(Canvas.TopProperty, this.Y - ellipse.Height / 2);
            LayoutRoot.Children.Add(ellipse);
            PlayAnimation(ellipse, random.Next(MIN_SIZE, MAX_SIZE));
        }

        /// <summary>
        /// 播放烟花的动画效果
        /// </summary>
        /// <param name="ellipse">动画对象</param>
        /// <param name="size">烟花大小(100-250)</param>
        public void PlayAnimation(Ellipse ellipse, int size)
        {
            mystoryboard = new Storyboard();
            mystoryboard.Completed += new EventHandler(mystoryboard_Completed);
            Duration dura = new Duration(new TimeSpan(00, 00, 00, 00, DURATION_TIME));

            #region 放大渐变
            DoubleAnimation dua1 = new DoubleAnimation();
            dua1.Duration = dura;
            dua1.From = ellipse.Width;
            dua1.To = size;

            DoubleAnimation dua2 = new DoubleAnimation();
            dua2.Duration = dura;
            dua2.From = ellipse.Height;
            dua2.To = size;

            Storyboard.SetTarget(dua1, ellipse);
            Storyboard.SetTargetProperty(dua1, new PropertyPath("(Ellipse.Width)"));
            Storyboard.SetTarget(dua2, ellipse);
            Storyboard.SetTargetProperty(dua2, new PropertyPath("(Ellipse.Height)"));
            mystoryboard.Children.Add(dua1);
            mystoryboard.Children.Add(dua2);
            #endregion

            #region 透明渐变
            DoubleAnimation dua3 = new DoubleAnimation();
            dua3.Duration = dura;
            dua3.From = 1;
            dua3.To = 0;
            Storyboard.SetTarget(dua3, ellipse);
            Storyboard.SetTargetProperty(dua3, new PropertyPath("(Ellipse.Opacity)"));
            mystoryboard.Children.Add(dua3);
            #endregion

            #region 位置渐变
            DoubleAnimation dua4 = new DoubleAnimation();
            dua4.Duration = dura;
            dua4.From = this.X - ellipse.Width / 2;
            dua4.To = this.X - size / 2;
            Storyboard.SetTarget(dua4, ellipse);
            Storyboard.SetTargetProperty(dua4, new PropertyPath("(Canvas.Left)"));
            mystoryboard.Children.Add(dua4);

            DoubleAnimation dua5 = new DoubleAnimation();
            dua5.Duration = dura;
            dua5.From = this.Y - ellipse.Width / 2;
            dua5.To = this.Y - size / 2;
            Storyboard.SetTarget(dua5, ellipse);
            Storyboard.SetTargetProperty(dua5, new PropertyPath("(Canvas.Top)"));
            mystoryboard.Children.Add(dua5);
            #endregion

            mystoryboard.BeginTime = new TimeSpan(0, 0, 0, 0, random.Next(0, startTime));
            mystoryboard.Begin();
        }

        #region Event mystoryboard_Completed
        void mystoryboard_Completed(object sender, EventArgs e)
        {
            //mystoryboard.Stop();
            LayoutRoot.Children.Remove(ellipse);
            OnPlayCompleted();
        }
        #endregion

        #region Property Implement
        private double X
        {
            get { return x; }
            set
            {
                this.x = value;
                this.SetValue(Canvas.LeftProperty, value);

            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                this.y = value;
                this.SetValue(Canvas.TopProperty, y);
            }
        }
        #endregion
    }
}
