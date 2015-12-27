using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using NKLogic.Util;

namespace NKLogic.Shapes
{
    /// <summary>
    /// 圆饼图形对象
    /// </summary>
    class PiePiece : Shape
    {
        #region dependency properties

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("RadiusProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 外圆半径
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty PushOutProperty =
            DependencyProperty.Register("PushOutProperty", typeof(double), typeof(PiePiece),
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
            DependencyProperty.Register("InnerRadiusProperty", typeof(double), typeof(PiePiece),
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
            DependencyProperty.Register("WedgeAngleProperty", typeof(double), typeof(PiePiece),
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
            DependencyProperty.Register("RotationAngleProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆饼起始角度(从Y轴正方向开始计算)
        /// </summary>
        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty CentreXProperty =
            DependencyProperty.Register("CentreXProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 圆坐标X
        /// </summary>
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        public static readonly DependencyProperty CentreYProperty =
            DependencyProperty.Register("CentreYProperty", typeof(double), typeof(PiePiece),
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
            DependencyProperty.Register("PercentageProperty", typeof(double), typeof(PiePiece),
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
            DependencyProperty.Register("PieceValueProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// 圆饼的值
        /// </summary>
        public double PieceValue
        {
            get { return (double)GetValue(PieceValueProperty); }
            set { SetValue(PieceValueProperty, value); }
        }


        #endregion

        protected override Geometry DefiningGeometry
        {
            get
            {
                // 创建一个 StreamGeometry 对象来描绘图形
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }

        public override Geometry RenderedGeometry
        {
            get
            {
                return DefiningGeometry;
            }
        }

        /// <summary>
        /// 根据属性绘制图形
        /// </summary>
        private void DrawGeometry(StreamGeometryContext context)
        {
            Point startPoint = new Point(CentreX, CentreY);

            //内圆起始点
            Point innerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, InnerRadius);
            innerArcStartPoint.Offset(CentreX, CentreY);

            //内圆结束点
            Point innerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius);
            innerArcEndPoint.Offset(CentreX, CentreY);

            //外圆起始点
            Point outerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, Radius);
            outerArcStartPoint.Offset(CentreX, CentreY);

            //外圆结束点
            Point outerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius);
            outerArcEndPoint.Offset(CentreX, CentreY);

            bool largeArc = WedgeAngle > 180.0;

            if (PushOut > 0)
            {
                //若存在偏移量，重新计算偏移后的4点
                Point offset = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle / 2, PushOut);
                innerArcStartPoint.Offset(offset.X, offset.Y);
                innerArcEndPoint.Offset(offset.X, offset.Y);
                outerArcStartPoint.Offset(offset.X, offset.Y);
                outerArcEndPoint.Offset(offset.X, offset.Y);

            }

            //画圆饼
            Size outerArcSize = new Size(Radius, Radius);
            Size innerArcSize = new Size(InnerRadius, InnerRadius);

            context.BeginFigure(innerArcStartPoint, true, true);
            context.LineTo(outerArcStartPoint, true, true);
            context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
            context.LineTo(innerArcEndPoint, true, true);
            context.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise, true, true);

            //画圆饼内的小圆
            //double minRadius = Math.Min(GetDistance(innerArcStartPoint, innerArcEndPoint) / 2, (Radius - InnerRadius) / 2);
            //Point minCentrePiont = GetMinCentrePoint(RotationAngle, WedgeAngle, Radius, InnerRadius, new Point(CentreX, CentreY));
            //Point minStartRadiusPoint = new Point(minCentrePiont.X + minRadius, minCentrePiont.Y);
            //Point minEndRadiusPoint = new Point(minCentrePiont.X - minRadius, minCentrePiont.Y);
            //Size minArcSize = new Size(minRadius, minRadius);
            //if (PushOut > 0)
            //{
            //    //若存在偏移量，重新计算偏移后的4点
            //    Point offset = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle / 2, PushOut);
            //    minEndRadiusPoint.Offset(offset.X, offset.Y);
            //    minStartRadiusPoint.Offset(offset.X, offset.Y);
            //}

            //context.BeginFigure(minStartRadiusPoint, true, true);
            //context.ArcTo(minEndRadiusPoint, minArcSize, 0, true, SweepDirection.Clockwise, true, true);
            //context.ArcTo(minStartRadiusPoint, minArcSize, 0, true, SweepDirection.Clockwise, true, true);

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

        /// <summary>
        /// 获取小圆心的点
        /// </summary>
        /// <param name="rotationAngle">起始角度</param>
        /// <param name="wedgeAngle">圆角(圆饼的角度)</param>
        /// <param name="outerRadius">外圆半径</param>
        /// <param name="innerRadius">内圆半径</param>
        /// <param name="orgPoint">圆饼中点坐标</param>
        /// <returns>返回小圆心的点坐标</returns>
        private Point GetMinCentrePoint(double rotationAngle, double wedgeAngle, double outerRadius, double innerRadius, Point orgPoint)
        {
            Point point = Utils.ComputeCartesianCoordinate(rotationAngle + wedgeAngle / 2, (outerRadius + innerRadius) / 2);
            point.Offset(orgPoint.X, orgPoint.Y);
            return point;
        }
    }

}
