using System.Windows;
using System;
using System.Speech.Synthesis;
using System.Windows.Media;

namespace LuckyRotorPlate
{
    /// <summary>
    /// testWindow.xaml 的交互逻辑
    /// </summary>
    public partial class testWindow : Window
    {
        private double minPower = 3;//1.199;//转盘初始量最小值
        private double powerRange = 1.2;//0.72;//0.87846102021178;//转盘初始量范围(初始量最大值减去初始量最小值)
        double drap = 0.005;

        public testWindow()
        {
            InitializeComponent();
        }

        private void test()
        {
            double s = minPower;
            double e = s + powerRange;
            double power = s;
            double tmpPower;
            double total = 0;
            while (power < e)
            {
                total = 0;
                tmpPower = power;
                while (tmpPower > 0)
                {
                    if (tmpPower > s)
                    {
                        total += s;
                    }
                    else
                    {
                        total += tmpPower;// 测试转动距离总量
                    }
                    tmpPower -= drap;
                }
                listBox1.Items.Add(power.ToString() + " : " + (total % 360).ToString() + " : " + (total).ToString());

                power += 0.01;
            }

            //int seed = unchecked((int)(DateTime.Now.Ticks));
            int seed = Guid.NewGuid().GetHashCode();
            Random rd = new Random(seed);
            double rstPower, distanceAngle;
            int count = int.Parse(textBox1.Text);
            DateTime d = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                rstPower = rd.NextDouble() * powerRange + minPower;
                distanceAngle = getDistanceAngle(rstPower);
                if (null != listBox1 && listBox1.Visibility == Visibility.Visible)
                {
                    listBox1.Items.Add("power:" + rstPower.ToString()
                        + ";DistanceAngle:" + distanceAngle.ToString() + ";ID:" + (distanceAngle % 360).ToString() + "。");
                }
                //System.Threading.Thread.Sleep(1000);
                //d = d.AddDays(1);
                //int td = unchecked((int)(d.Ticks));
                //listBox1.Items.Add(d.ToString("HH:mm:ss.fff") + "--" + td.ToString());
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
                if (tmpPower > minPower)
                {
                    tmpAngle += minPower;  //在大于某下限值时，圆盘匀速转动。
                }
                else
                {
                    tmpAngle += tmpPower;// 测试转动距离总量
                }
                tmpPower -= drap;
            }

            return tmpAngle;
        }

        ShowWord sw;
        int tmpTop = 0;
        private void test3DText()
        {
            tmpTop += 100;
            sw = new ShowWord("宋体", Brushes.Blue, Brushes.Blue);
            gdTest.Children.Add(sw);
            sw.Width = 800;
            sw.Height = tmpTop;
            sw.Margin = new Thickness(0, 0, 0, 0);
            sw.HorizontalAlignment = HorizontalAlignment.Left;
            sw.VerticalAlignment = VerticalAlignment.Top;
            sw.Show("Ready", 100);
            sw.ShowEnd += new EventHandler(sw_ShowEnd);
            sw.HideEnd += new EventHandler(sw_HideEnd);
        }

        void sw_ShowEnd(object sender, EventArgs e)
        {
            sw.Hide(900);
        }

        void sw_HideEnd(object sender, EventArgs e)
        {
            sw.Show("行政科(含离岗退养干部)、技术保障科", 100);
            sw.ShowEnd -= new EventHandler(sw_ShowEnd);
            sw.HideEnd -= new EventHandler(sw_HideEnd);
        }

        private void playFireworks()
        {
            Fireworks fw = new Fireworks(this.Width, this.Height, 20);
            gdTest.Children.Add(fw);
            fw.PlayEnd += new EventHandler(fw_PlayEnd);
            fw.Play();
        }

        void fw_PlayEnd(object sender, EventArgs e)
        {
            MessageBox.Show("sdfas");
        }

        void speck(string word)
        {
            SpeechSynthesizer sy = new SpeechSynthesizer();
            sy.SelectVoice("Microsoft Mary");
            sy.SpeakAsync(word);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //test();
            test3DText();
            //playFireworks();
            //speck(textBox1.Text);
        }
    }
}
