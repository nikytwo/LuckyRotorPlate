using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LuckyRotorPlate
{
    /// <summary>
    /// Fireworks.xaml 的交互逻辑
    /// </summary>
    public partial class Fireworks : UserControl
    {
        private int fireworkCount = 20;//烟花数量
        private int completedCount = 0;

        /// <summary>
        /// 开始放烟花事件
        /// </summary>
        public event EventHandler PlayBegin;

        protected void OnPlayBegin()
        {
            EventHandler handler = PlayBegin;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// 烟花结束事件
        /// </summary>
        public event EventHandler PlayEnd;

        protected void OnPlayEnd()
        {
            EventHandler handler = PlayEnd;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="count">烟花数量</param>
        public Fireworks(double width, double height, int count)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            gdFireworks.Width = width;
            gdFireworks.Height = height;
            fireworkCount = count;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="count">烟花数量</param>
        public Fireworks(double width, double height)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            gdFireworks.Width = width;
            gdFireworks.Height = height;
        }

        public Fireworks()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 放烟花
        /// </summary>
        public void Play()
        {
            completedCount = 0;
            Random r = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < fireworkCount; i++)
            {
                int x = r.Next(-(int)gdFireworks.Width / 2, (int)gdFireworks.Width / 2);
                int y = r.Next(-(int)gdFireworks.Height / 2, 0);
                StackWindow sw = new StackWindow(x, y);
                sw.PlayCompleted += new EventHandler(sw_PlayCompleted);
                sw.Height = 10;
                sw.Width = 10;
                gdFireworks.Children.Add(sw);
                sw.Play();
            }

            OnPlayBegin();
        }

        void sw_PlayCompleted(object sender, EventArgs e)
        {
            if (++completedCount >= fireworkCount)
            {
                OnPlayEnd();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gdFireworks.Width = e.NewSize.Width;
            gdFireworks.Height = e.NewSize.Height;
        }
    }
}
