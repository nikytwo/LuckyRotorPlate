using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using NKLogic.Shapes;
using NKLogic.Util;
using System.Collections.Generic;

namespace NKLogic.UIElements
{
    /// <summary>
    /// 圆饼组合图形对象
    /// </summary>
    public class PiePieceElement : FrameworkElement
    {
        private const int SIZE_TIMES = 2;
        // 创建一个组合图形对象.
        private VisualCollection _children;
        private Brush fillColor;//圆饼的填充色
        private Brush borderColor;//圆饼边的颜色
        private Brush pieFontColor = Brushes.Black;//圆饼内文字的颜色
        private string fontFamily = "宋体";
        private bool isCN = false;//圆饼内为中文字

        public PiePieceElement(string name, double radius, double innerRadius, double pushOut,
            double wedgeAngle, double rotationAngle, double centreX, double centreY, double value, 
            Brush bColor, Brush fColor,Brush fontColor, string fontFamily, bool isCn)
        {
            PieName = name;
            Radius = radius;
            InnerRadius = innerRadius;
            PushOut = pushOut;
            WedgeAngle = wedgeAngle;
            RotationAngle = rotationAngle;
            CentreX = centreX;
            CentreY = centreY;
            PieceValue = value;
            fillColor = fColor;
            borderColor = bColor;
            pieFontColor = fontColor;
            this.fontFamily = fontFamily;
            this.isCN = isCn;

            _children = new VisualCollection(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            CreateDrawingPiePiece(drawingContext);
            CreateDrawingVisualText(drawingContext);
        }

        private void CreateDrawingVisualText(DrawingContext drawingContext)
        {
            // 创建并绘制圆饼图形的名称.
            double fontSize = GetFontSize();
            Point centrePoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle / 2, (Radius + InnerRadius) / 2);
            centrePoint.Offset(CentreX, CentreY);

            List<String> texts = new List<string>();
            int index = 0;
            int c = 1;
            string text = string.Empty;
            for (int i = 0; i < this.PieName.Length; i++)
            {
                text += this.PieName[i];
                c++;
                if (fontSize * c > (Radius - InnerRadius) || i == PieName.Length - 1)
                {
                    index++;
                    texts.Add(text);
                    text = string.Empty;
                    c = 1;
                }
            }

            if (fontSize > 0)
            {
                RotateTransform rt = new RotateTransform(RotationAngle + WedgeAngle / 2 - 90, centrePoint.X, centrePoint.Y);
                drawingContext.PushTransform(rt);

                for (int i = 0; i < texts.Count; i++)
                {
                    FormattedText ft = new FormattedText(texts[i],
                          CultureInfo.GetCultureInfo("zh-CN"),
                          FlowDirection.LeftToRight,
                          new Typeface(fontFamily),
                          fontSize, pieFontColor);

                    Point fontPoint = new Point(centrePoint.X, centrePoint.Y);
                    fontPoint.X -= ft.Width / 2;
                    fontPoint.Y -= ft.Height * texts.Count / 2 - i * ft.Height;
                    if (PushOut > 0)
                    {
                        //若存在偏移量，重新计算偏移后的点
                        Point offset = Utils.ComputeCartesianCoordinate(0, PushOut);
                        fontPoint.Offset(offset.X, offset.Y);
                    }
                    drawingContext.DrawText(ft, fontPoint);
                }
            }
        }

        private double GetFontSize()
        {
            Point innerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, InnerRadius);
            innerArcStartPoint.Offset(CentreX, CentreY);
            Point innerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius);
            innerArcEndPoint.Offset(CentreX, CentreY);
            double minRadius = Math.Min(Math.Min(GetDistance(innerArcStartPoint, innerArcEndPoint) / 2, (Radius - InnerRadius) / 2), 10);
            double fontSize = minRadius * SIZE_TIMES;//Math.Sqrt(minRadius * minRadius / 2) * SIZE_TIMES;
            return fontSize;
        }

        private void CreateDrawingPiePiece(DrawingContext drawingContext)
        {
            // 创建并绘制圆饼图形.
            PiePiece pp = new PiePiece
            {
                CentreX = this.CentreX,
                CentreY = this.CentreY,
                RotationAngle = this.RotationAngle,
                WedgeAngle = this.WedgeAngle,
                Radius = this.Radius,
                InnerRadius = this.InnerRadius,
                PushOut = this.PushOut,
                PieceValue = this.PieceValue
            };
            Pen myPen = new Pen(borderColor, 1);

            drawingContext.DrawGeometry(fillColor, myPen, pp.RenderedGeometry);
        }

        /// <summary>
        /// 圆饼填充色
        /// </summary>
        public Brush Fill
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        public static readonly DependencyProperty PieNameProperty =
            DependencyProperty.Register("PieNameProperty", typeof(string), typeof(PiePieceElement),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆饼对象名称
        /// </summary>
        public string PieName
        {
            get { return (string)GetValue(PieNameProperty); }
            set { SetValue(PieNameProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("RadiusProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆的外圈半径
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty PushOutProperty =
            DependencyProperty.Register("PushOutProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆饼（被选择时）的偏移量（相对于圆心）
        /// </summary>
        public double PushOut
        {
            get { return (double)GetValue(PushOutProperty); }
            set { SetValue(PushOutProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadiusProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 内圆半径
        /// </summary>
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty WedgeAngleProperty =
            DependencyProperty.Register("WedgeAngleProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆饼（所跨越）的角度
        /// </summary>
        public double WedgeAngle
        {
            get { return (double)GetValue(WedgeAngleProperty); }
            set
            {
                SetValue(WedgeAngleProperty, value);
                this.Percentage = (value / 360.0);
            }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register("RotationAngleProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆饼起始角度(从Y轴正方向开始计算)
        /// </summary>
        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value % 360); }
        }

        public static readonly DependencyProperty CentreXProperty =
            DependencyProperty.Register("CentreXProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆坐标X.
        /// </summary>
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        public static readonly DependencyProperty CentreYProperty =
            DependencyProperty.Register("CentreYProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆坐标Y
        /// </summary>
        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("PercentageProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// 圆饼所占比率(相对整个圆)
        /// </summary>
        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            private set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PieceValueProperty =
            DependencyProperty.Register("PieceValueProperty", typeof(double), typeof(PiePieceElement),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// 圆饼的值
        /// </summary>
        public double PieceValue
        {
            get { return (double)GetValue(PieceValueProperty); }
            set { SetValue(PieceValueProperty, value); }
        }

        /// <summary>
        /// 获取两点间的距离
        /// </summary>
        /// <param name="p1">点1</param>
        /// <param name="p2">点2</param>
        /// <returns>两点距离</returns>
        private double GetDistance(Point p1, Point p2)
        {
            double x = System.Math.Abs(p1.X - p2.X);
            double y = System.Math.Abs(p1.Y - p2.Y);
            return Math.Sqrt(x * x + y * y);
        }

        // 获取 Visual 子对象的数量.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // 根据索引获取 Visual 子对象.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}
