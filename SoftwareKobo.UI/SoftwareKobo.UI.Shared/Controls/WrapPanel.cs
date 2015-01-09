#if WINDOWS_APP||WINDOWS_PHONE_APP
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SoftwareKobo.UI.Controls
{
    /// <summary>
    /// 从左至右按顺序位置定位子元素，在包含框的边缘处将内容断开至下一行。后续排序按照从上至下或从右至左的顺序进行，具体取决于 Orientation 属性的值。
    /// </summary>
    public class WrapPanel : Panel
    {
        /// <summary>
        /// 标识 ItemHeight 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemWidthOrHeightChanged));

        /// <summary>
        /// 标识 ItemWidth 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(WrapPanel), new PropertyMetadata(double.NaN, OnItemWidthOrHeightChanged));

        /// <summary>
        /// 标识 Orientation 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WrapPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

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
            int firstInLine = 0;
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            double accumulatedIndirect = 0;
            double itemDirect = (Orientation == Orientation.Horizontal ? itemWidth : itemHeight);
            OrientedSize curLineSize = new OrientedSize(Orientation);
            OrientedSize uvFinalSize = new OrientedSize(Orientation, finalSize.Width, finalSize.Height);
            bool itemWidthSet = double.IsNaN(itemWidth) == false;
            bool itemHeightSet = double.IsNaN(itemHeight) == false;
            bool useItemDirect = (Orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet);

            UIElementCollection children = Children;

            for (int i = 0, count = children.Count; i < count; i++)
            {
                UIElement child = children[i] as UIElement;
                if (child == null)
                {
                    continue;
                }

                OrientedSize sz = new OrientedSize(
                    Orientation,
                    (itemWidthSet ? itemWidth : child.DesiredSize.Width),
                    (itemHeightSet ? itemHeight : child.DesiredSize.Height));

                if (curLineSize.Direct + sz.Direct > uvFinalSize.Direct) //need to switch to another line
                {
                    ArrangeLine(accumulatedIndirect, curLineSize.Indirect, firstInLine, i, useItemDirect, itemDirect);

                    accumulatedIndirect += curLineSize.Indirect;
                    curLineSize = sz;

                    if (sz.Direct > uvFinalSize.Direct) //the element is wider then the constraint - give it a separate line
                    {
                        //switch to next line which only contain one element
                        ArrangeLine(accumulatedIndirect, sz.Indirect, i, ++i, useItemDirect, itemDirect);

                        accumulatedIndirect += sz.Indirect;
                        curLineSize = new OrientedSize(Orientation);
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
                {
                    curLineSize.Direct += sz.Direct;
                    curLineSize.Indirect = Math.Max(sz.Indirect, curLineSize.Indirect);
                }
            }

            //arrange the last line, if any
            if (firstInLine < children.Count)
            {
                ArrangeLine(accumulatedIndirect, curLineSize.Indirect, firstInLine, children.Count, useItemDirect, itemDirect);
            }

            return finalSize;
        }

        /// <summary>
        /// 测量 WrapPanel 的子元素，以便准备在 ArrangeOverride 处理过程中排列它们。
        /// </summary>
        /// <param name="availableSize">不能超过的上限 Size。</param>
        /// <returns>Size，表示元素的所需大小。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            OrientedSize curLineSize = new OrientedSize(Orientation);
            OrientedSize panelSize = new OrientedSize(Orientation);
            OrientedSize uvConstraint = new OrientedSize(Orientation, availableSize.Width, availableSize.Height);
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool itemWidthSet = double.IsNaN(itemWidth) == false;
            bool itemHeightSet = double.IsNaN(itemHeight) == false;

            Size childConstraint = new Size(
                (itemWidthSet ? itemWidth : availableSize.Width),
                (itemHeightSet ? itemHeight : availableSize.Height));

            UIElementCollection children = Children;

            for (int i = 0, count = children.Count; i < count; i++)
            {
                UIElement child = children[i] as UIElement;
                if (child == null)
                {
                    continue;
                }

                //Flow passes its own constrint to children
                child.Measure(childConstraint);

                //this is the size of the child in UV space
                OrientedSize sz = new OrientedSize(
                    Orientation,
                    (itemWidthSet ? itemWidth : child.DesiredSize.Width),
                    (itemHeightSet ? itemHeight : child.DesiredSize.Height));

                if (curLineSize.Direct + sz.Direct > uvConstraint.Direct) //need to switch to another line
                {
                    panelSize.Direct = Math.Max(curLineSize.Direct, panelSize.Direct);
                    panelSize.Indirect += curLineSize.Indirect;
                    curLineSize = sz;

                    if (sz.Direct > uvConstraint.Direct) //the element is wider then the constrint - give it a separate line
                    {
                        panelSize.Direct = Math.Max(sz.Direct, panelSize.Direct);
                        panelSize.Indirect += sz.Indirect;
                        curLineSize = new OrientedSize(Orientation);
                    }
                }
                else //continue to accumulate a line
                {
                    curLineSize.Direct += sz.Direct;
                    curLineSize.Indirect = Math.Max(sz.Indirect, curLineSize.Indirect);
                }
            }

            //the last line size, if any should be added
            panelSize.Direct = Math.Max(curLineSize.Direct, panelSize.Direct);
            panelSize.Indirect += curLineSize.Indirect;

            //go from UV space to W/H space
            return new Size(panelSize.Width, panelSize.Height);
        }

        private static void OnItemWidthOrHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            double value = (double)e.NewValue;

            if (double.IsNaN(value) == false && (value <= 0.0d || double.IsPositiveInfinity(value)))
            {
                source.SetValue(e.Property, (double)e.OldValue);

                throw new ArgumentException("数值范围错误。");
            }

            source.InvalidateMeasure();
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            Orientation value = (Orientation)e.NewValue;

            if (Enum.IsDefined(typeof(Orientation), value) == false)
            {
                source.SetValue(e.Property, (Orientation)e.OldValue);

                throw new ArgumentException("方向未定义。");
            }

            source.InvalidateMeasure();
        }

        private void ArrangeLine(double indirect, double lineIndirect, int start, int end, bool useItemDirect, double itemDirect)
        {
            double direct = 0;
            bool isHorizontal = (Orientation == Orientation.Horizontal);

            UIElementCollection children = Children;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i] as UIElement;
                if (child != null)
                {
                    OrientedSize childSize = new OrientedSize(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                    double layoutSlotDirect = (useItemDirect ? itemDirect : childSize.Direct);
                    child.Arrange(new Rect(
                        (isHorizontal ? direct : indirect),
                        (isHorizontal ? indirect : direct),
                        (isHorizontal ? layoutSlotDirect : lineIndirect),
                        (isHorizontal ? lineIndirect : layoutSlotDirect)));
                    direct += layoutSlotDirect;
                }
            }
        }

        private struct OrientedSize
        {
            internal double Direct;

            internal double Indirect;

            private readonly Orientation _orientation;

            internal OrientedSize(Orientation orientation, double width, double height) : this(orientation)
            {
                Width = width;
                Height = height;
            }

            internal OrientedSize(Orientation orientation)
            {
                Direct = 0.0d;
                Indirect = 0.0d;
                _orientation = orientation;
            }

            internal double Height
            {
                get
                {
                    return (_orientation == Orientation.Horizontal ? Indirect : Direct);
                }
                set
                {
                    if (_orientation == Orientation.Horizontal)
                    {
                        Indirect = value;
                    }
                    else
                    {
                        Direct = value;
                    }
                }
            }

            internal double Width
            {
                get
                {
                    return (_orientation == Orientation.Horizontal ? Direct : Indirect);
                }
                set
                {
                    if (_orientation == Orientation.Horizontal)
                    {
                        Direct = value;
                    }
                    else
                    {
                        Indirect = value;
                    }
                }
            }
        }
    }
}
#endif