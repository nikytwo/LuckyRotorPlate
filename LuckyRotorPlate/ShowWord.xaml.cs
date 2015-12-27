using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using NKLogic.Util;
using Petzold.Text3D;

namespace LuckyRotorPlate
{
    /// <summary>
    /// ShowWord.xaml 的交互逻辑
    /// </summary>
    public partial class ShowWord : UserControl
    {
        private double angle = 0;
        private double lightX = 0;
        private double lightY = -0.4;
        private double lightZ = -0.4;
        private Brush showTextColor = Brushes.Red;
        private Brush showTextSideColor = Brushes.Red;
        private string fontFamily = "Times New Roman";
        private int showTime = 1000;//显示的时间长度(毫秒)
        private int minFieldView = 95;
        private int maxFieldView = 170;

        private RotateTransform3D transform;
        private DirectionalLight dirLight;
        private PerspectiveCamera camera;
        private Viewport3D vp3d;
        private DiffuseMaterial textMaterial;
        private DiffuseMaterial textSideMaterial;

        /// <summary>
        /// 开始显示事件
        /// </summary>
        public event EventHandler ShowBegin;
        /// <summary>
        /// 显示结束
        /// </summary>
        public event EventHandler ShowEnd;
        /// <summary>
        /// 隐藏结束
        /// </summary>
        public event EventHandler HideEnd;

        protected void OnShowBegin()
        {
            EventHandler handler = ShowBegin;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void OnShowEnd()
        {
            EventHandler handler = ShowEnd;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void OnHideEnd()
        {
            EventHandler handler = HideEnd;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="family">字体</param>
        /// <param name="textColor">字体的正面颜色</param>
        /// <param name="sideColor">字体的侧面颜色</param>
        public ShowWord(string family, Brush textColor, Brush sideColor)
        {
            InitializeComponent();

            this.fontFamily = family;
            this.showTextColor = textColor;
            this.showTextSideColor = sideColor;

            init3DViewPort();
        }

        private void sliderField_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != camera)
            {
                camera.FieldOfView = e.NewValue;
            }
        }

        /// <summary>
        /// 初始化3D场景
        /// </summary>
        private void init3DViewPort()
        {
            sliderField.Value = 50;

            //dirLight = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(lightX, lightY, lightZ));
            //DirectionalLight d2 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(2, -2, 6));
            //DirectionalLight d3 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(-5, -5, 6));
            //DirectionalLight d4 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(2, 0, -3));
            //DirectionalLight d5 = new DirectionalLight(Color.FromRgb(0xc0, 0xc0, 0xc0),
            //    new Vector3D(-2, 0, -3));
            transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, -1, 0), angle));
            camera = new PerspectiveCamera(new Point3D(-1, -0.2, 8),
                new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), sliderField.Value);

            ModelVisual3D mv3d = new ModelVisual3D();
            Model3DGroup m3g = new Model3DGroup();
            m3g.Children.Add(new AmbientLight(Color.FromRgb(0xFF, 0xFF, 0xFF)));
            //m3g.Children.Add(d2);
            //m3g.Children.Add(d3);
            //m3g.Children.Add(d4);
            //m3g.Children.Add(d5);
            mv3d.Content = m3g;

            vp3d = new Viewport3D();
            vp3d.Camera = camera;
            vp3d.Children.Add(mv3d);

            gd3DViewPord.Children.Add(vp3d);
        }

        /// <summary>
        /// 绘制3D字体
        /// </summary>
        /// <param name="text">需要绘制的字符串</param>
        /// <param name="transform">RotateTransform3D 对象</param>
        /// <param name="top">字符上边离中点距离</param>
        /// <param name="depth">3D字深(厚)度</param>
        /// <returns>3D字体对象</returns>
        private SolidText draw3DText(string text, RotateTransform3D transform, double top, double depth)
        {
            const double SPECULAR_POWER = 85;
            const double FONT_WIDTH = 0.25;

            int wordLength = Utils.GetLength(text);
            SolidText st;
            st = new SolidText();
            st.Text = text;
            st.FontFamily = new FontFamily(fontFamily);
            st.FontWeight = FontWeights.Bold;
            st.FontSize = 1;
            st.Origin = new Point(-(wordLength * FONT_WIDTH) - 1.2, top);
            st.Depth = depth;

            MaterialGroup mg;
            mg = new MaterialGroup();
            textMaterial = new DiffuseMaterial(showTextColor);
            mg.Children.Add(textMaterial);
            SpecularMaterial sm = new SpecularMaterial();
            sm.Brush = Brushes.White;
            sm.SpecularPower = SPECULAR_POWER;
            mg.Children.Add(sm);
            st.Material = mg;

            mg = new MaterialGroup();
            textSideMaterial = new DiffuseMaterial(showTextSideColor);
            mg.Children.Add(textSideMaterial);
            sm = new SpecularMaterial();
            sm.Brush = Brushes.White;
            mg.Children.Add(sm);
            st.SideMaterial = mg;

            st.Transform = transform;

            return st;
        }

        /// <summary>
        /// 将要显示的3D字符添加到场景中
        /// </summary>
        /// <param name="luckyName">幸运人名称</param>
        private void addWord(String word)
        {
            SolidText stext = draw3DText(word.Trim(), transform, 0.5, 0);
            for (int i = vp3d.Children.Count - 1; i >= 0;i--)
            {
                Visual3D v3d = vp3d.Children[i];
                if (v3d.GetType() == typeof(SolidText))
                {
                    vp3d.Children.Remove(v3d);
                }
            }
            vp3d.Children.Add(stext);
        }

        /// <summary>
        /// 动画显示3D字
        /// </summary>
        /// <param name="showTime">持续时间</param>
        /// <param name="word">显示的字符</param>
        public void Show(String word, int showTime)
        {
            addWord(word);

            //vp3d.Opacity = 1;
            Storyboard showStoryboard;
            DoubleAnimation doubleAnimation;

            showStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(maxFieldView, minFieldView, new Duration(TimeSpan.FromMilliseconds(showTime)));
            Storyboard.SetTarget(doubleAnimation, sliderField);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Slider.ValueProperty));
            showStoryboard.Children.Add(doubleAnimation);

            doubleAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(showTime)));
            Storyboard.SetTarget(doubleAnimation, vp3d);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Viewport3D.OpacityProperty));
            showStoryboard.Children.Add(doubleAnimation);

            showStoryboard.Completed += new EventHandler(showStoryboard_Completed);
            showStoryboard.Begin();

            OnShowBegin();
        }

        private void showStoryboard_Completed(object sender, EventArgs e)
        {
            //
            OnShowEnd();
        }

        /// <summary>
        /// 动画隐藏3D字
        /// </summary>
        /// <param name="time">持续时间</param>
        public void Hide(int time)
        {
            Storyboard hideStoryboard;
            DoubleAnimation doubleAnimation;

            hideStoryboard = new Storyboard();

            doubleAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(showTime)));
            Storyboard.SetTarget(doubleAnimation, vp3d);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Viewport3D.OpacityProperty));
            hideStoryboard.Children.Add(doubleAnimation);

            hideStoryboard.Completed += new EventHandler(hideStoryboard_Completed);
            hideStoryboard.Begin();
        }

        private void hideStoryboard_Completed(object sender, EventArgs e)
        {
            //
            OnHideEnd();
        }
    }
}
