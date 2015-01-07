using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SoftwareKobo.UI.WinRT.Controls
{
    /// <summary>
    /// 从左至右按顺序位置定位子元素，在包含框的边缘处将内容断开至下一行。后续排序按照从上至下或从右至左的顺序进行，具体取决于 WrapPanel.Orientation 属性的值。
    /// </summary>
    public class WrapPanel : Panel
    {
        /// <summary>
        /// 标识 WrapPanel.ItemHeight 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemWidthOrHeightPropertyChanged));

        /// <summary>
        /// 标识 WrapPanel.ItemWidth 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemWidthOrHeightPropertyChanged));

        /// <summary>
        /// 标识 WrapPanel.Orientation 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WrapPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        /// <summary>
        /// 初始化 WrapPanel 类的新实例。
        /// </summary>
        public WrapPanel()
            : base()
        {
        }

        /// <summary>
        /// 获取或设置一个值，该值指定 WrapPanel 中所含全部项的高度。
        /// </summary>
        public double ItemHeight
        {
            get
            {
                return (double)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(ItemHeightProperty, value);
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指定 WrapPanel 中所含全部项的宽度。
        /// </summary>
        public double ItemWidth
        {
            get
            {
                return (double)GetValue(ItemWidthProperty);
            }
            set
            {
                SetValue(ItemWidthProperty, value);
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指定子内容的排列方向。
        /// </summary>
        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        /// <summary>
        /// 排列 WrapPanel 元素的内容。
        /// </summary>
        /// <param name="finalSize">Size，此元素应使用它来排列其子元素。</param>
        /// <returns>Size，表示此 WrapPanel 元素及其子元素的排列大小。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            OrientedSize currentLineSize = new OrientedSize(Orientation);
            OrientedSize maximumSize = new OrientedSize(Orientation, finalSize.Width, finalSize.Height);

            bool hasFixedWidth = double.IsNaN(ItemWidth) == false;
            bool hasFixedHeight = double.IsNaN(ItemHeight) == false;

            double indirectOffset = 0.0d;
            double? directDelta = (Orientation == Orientation.Horizontal) ?
                (hasFixedWidth ? (double?)ItemWidth : null) :
                (hasFixedHeight ? (double?)ItemHeight : null);

            int lineStart = 0;

            for (int lineEnd = 0; lineEnd < Children.Count; lineEnd++)
            {
                UIElement element = Children[lineEnd];

                OrientedSize elementSize = new OrientedSize(Orientation, hasFixedWidth ? ItemWidth : element.DesiredSize.Width, hasFixedHeight ? ItemHeight : element.DesiredSize.Height);

                if (currentLineSize.Direct + elementSize.Direct > maximumSize.Direct)
                {
                    ArrangeLine(lineStart, lineEnd, directDelta, indirectOffset, currentLineSize.Indirect);

                    indirectOffset += currentLineSize.Indirect;
                    currentLineSize = elementSize;

                    if (elementSize.Direct > maximumSize.Direct)
                    {
                        ArrangeLine(lineEnd, ++lineEnd, directDelta, indirectOffset, elementSize.Indirect);

                        indirectOffset += currentLineSize.Indirect;
                        currentLineSize = new OrientedSize(Orientation);
                    }
                    lineStart = lineEnd;
                }
                else
                {
                    currentLineSize.Direct += elementSize.Direct;
                    currentLineSize.Indirect = Math.Max(currentLineSize.Indirect, elementSize.Indirect);
                }
            }

            if (lineStart < Children.Count)
            {
                ArrangeLine(lineStart, Children.Count, directDelta, indirectOffset, currentLineSize.Indirect);
            }

            return finalSize;
        }

        /// <summary>
        /// 测量 WrapPanel 的子元素，以便准备在 WrapPanel.ArrangeOverride(Size) 处理过程中排列它们。
        /// </summary>
        /// <param name="availableSize">不能超过的上限 Size。</param>
        /// <returns>Size，表示元素的所需大小。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            OrientedSize currentLineSize = new OrientedSize(Orientation);
            OrientedSize totalSize = new OrientedSize(Orientation);
            OrientedSize maximumSize = new OrientedSize(Orientation, availableSize.Width, availableSize.Height);

            bool hasFixedWidth = double.IsNaN(ItemWidth) == false;
            bool hasFixedHeight = double.IsNaN(ItemHeight) == false;

            Size itemSize = new Size(hasFixedWidth ? ItemWidth : availableSize.Width, hasFixedHeight ? ItemHeight : availableSize.Height);

            foreach (UIElement element in Children)
            {
                element.Measure(itemSize);
                OrientedSize elementSize = new OrientedSize(Orientation, hasFixedWidth ? ItemWidth : element.DesiredSize.Width, hasFixedHeight ? ItemHeight : element.DesiredSize.Height);

                if (currentLineSize.Direct + elementSize.Direct > maximumSize.Direct)
                {
                    totalSize.Direct = Math.Max(currentLineSize.Direct, totalSize.Direct);
                    totalSize.Indirect += currentLineSize.Indirect;

                    currentLineSize = elementSize;

                    if (elementSize.Direct > maximumSize.Direct)
                    {
                        totalSize.Direct = Math.Max(elementSize.Direct, totalSize.Direct);
                        totalSize.Indirect += elementSize.Indirect;

                        currentLineSize = new OrientedSize(Orientation);
                    }
                }
                else
                {
                    currentLineSize.Direct += elementSize.Direct;
                    currentLineSize.Indirect = Math.Max(currentLineSize.Indirect, elementSize.Indirect);
                }
            }

            totalSize.Direct = Math.Max(currentLineSize.Direct, totalSize.Direct);
            totalSize.Indirect += currentLineSize.Indirect;

            return new Size(totalSize.Width, totalSize.Height);
        }

        private static void OnItemWidthOrHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            double value = (double)e.NewValue;

            if (double.IsNaN(value) == false && (value <= 0.0d || double.IsPositiveInfinity(value)))
            {
                source.SetValue(e.Property, (double)e.OldValue);

                throw new InvalidOperationException("数值范围错误。");
            }

            source.InvalidateMeasure();
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            Orientation value = (Orientation)e.NewValue;

            if (Enum.IsDefined(typeof(Orientation), value) == false)
            {
                source.SetValue(e.Property, (Orientation)e.OldValue);

                throw new InvalidOperationException("方向未定义。");
            }

            source.InvalidateMeasure();
        }

        private void ArrangeLine(int lineStart, int lineEnd, double? directDelta, double indirectOffset, double indirectGrowth)
        {
            double directOffset = 0.0d;

            for (int index = lineStart; index < lineEnd; index++)
            {
                OrientedSize elementSize = new OrientedSize(Orientation, Children[index].DesiredSize.Width, Children[index].DesiredSize.Height);

                double directGrowth = directDelta != null ? directDelta.Value : elementSize.Direct;

                Rect bounds = Orientation == Orientation.Horizontal
                    ? new Rect(directOffset, indirectOffset, directGrowth, indirectGrowth)
                    : new Rect(indirectOffset, directOffset, indirectGrowth, directGrowth);
                Children[index].Arrange(bounds);

                directOffset += directGrowth;
            }
        }

        private struct OrientedSize
        {
            internal double Direct;
            internal double Indirect;
            private readonly Orientation _orientation;

            internal OrientedSize(Orientation orientation)
            {
                // 初始化所有字段。
                this._orientation = orientation;
                Direct = 0.0d;
                Indirect = 0.0d;
            }

            internal OrientedSize(Orientation orientation, double width, double height)
                : this(orientation)
            {
                this.Width = width;
                this.Height = height;
            }

            internal double Height
            {
                get
                {
                    switch (_orientation)
                    {
                        case Orientation.Horizontal:
                            return Indirect;

                        case Orientation.Vertical:
                            return Direct;

                        default:
                            throw new InvalidOperationException();
                    }
                }
                private set
                {
                    switch (_orientation)
                    {
                        case Orientation.Horizontal:
                            Indirect = value;
                            break;

                        case Orientation.Vertical:
                            Direct = value;
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            internal double Width
            {
                get
                {
                    switch (_orientation)
                    {
                        case Orientation.Horizontal:
                            return Direct;

                        case Orientation.Vertical:
                            return Indirect;

                        default:
                            throw new InvalidOperationException();
                    }
                }
                private set
                {
                    switch (_orientation)
                    {
                        case Orientation.Horizontal:
                            Direct = value;
                            break;

                        case Orientation.Vertical:
                            Indirect = value;
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }
    }
}