using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SoftwareKobo.UI.WinRT.Controls
{
    /// <summary>
    /// 指定 DockPanel 内子元素的 Dock 位置。
    /// </summary>
    public enum Dock
    {
        /// <summary>
        /// 位于 DockPanel 左侧的子元素。
        /// </summary>
        Left,
        /// <summary>
        /// 位于 DockPanel 顶部的子元素。
        /// </summary>
        Top,
        /// <summary>
        /// 位于 DockPanel 右侧的子元素。
        /// </summary>
        Right,
        /// <summary>
        /// 位于 DockPanel 底部的子元素。
        /// </summary>
        Bottom
    }

    /// <summary>
    /// 定义一个区域，从中可以相对于彼此水平或垂直排列子元素。
    /// </summary>
    public class DockPanel : Panel
    {
        /// <summary>
        /// 标识 Dock 附加属性。
        /// </summary>
        public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(nameof(Dock), typeof(Dock), typeof(DockPanel), new PropertyMetadata(Dock.Left, OnDockPropertyChanged));

        /// <summary>
        /// 标识 LastChildFill 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty LastChildFillProperty = DependencyProperty.Register(nameof(LastChildFill), typeof(bool), typeof(DockPanel), new PropertyMetadata(true, OnLastChildFillPropertyChanged));

        /// <summary>
        /// 初始化 DockPanel 类的新实例。
        /// </summary>
        public DockPanel()
            : base()
        {
        }

        /// <summary>
        /// 获取或设置一个值，该值指示 DockPanel 中的最后一个子元素是否拉伸以填满剩余可用空间。
        /// </summary>
        public bool LastChildFill
        {
            get
            {
                return (bool)GetValue(LastChildFillProperty);
            }
            set
            {
                SetValue(LastChildFillProperty, value);
            }
        }

        /// <summary>
        /// 获取指定 UIElement 的 Dock 附加属性的值。
        /// </summary>
        /// <param name="element">从中读取属性值的元素。</param>
        /// <returns>该元素的 Dock 属性值。</returns>
        public static Dock GetDock(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return (Dock)element.GetValue(DockProperty);
        }

        /// <summary>
        /// 将 Dock 附加属性的值设置为指定元素。
        /// </summary>
        /// <param name="element">附加属性所写入的元素。</param>
        /// <param name="dock">所需的 Dock 值。</param>
        public static void SetDock(UIElement element, Dock dock)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            element.SetValue(DockProperty, dock);
        }

        /// <summary>
        /// 排列 DockPanel 元素的内容（子元素）。
        /// </summary>
        /// <param name="finalSize">Size，此元素使用它来排列其子元素。</param>
        /// <returns>Size，表示此 DockPanel 元素的排列大小。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElementCollection children = Children;
            int totalChildrenCount = children.Count;
            int nonFillChildrenCount = totalChildrenCount - (LastChildFill ? 1 : 0);

            double accumulatedLeft = 0;
            double accumulatedTop = 0;
            double accumulatedRight = 0;
            double accumulatedBottom = 0;

            for (int i = 0; i < totalChildrenCount; ++i)
            {
                UIElement child = children[i];
                if (child == null)
                {
                    continue;
                }

                Size childDesiredSize = child.DesiredSize;
                Rect rcChild = new Rect(accumulatedLeft, accumulatedTop, Math.Max(0.0, finalSize.Width - (accumulatedLeft + accumulatedRight)), Math.Max(0.0, finalSize.Height - (accumulatedTop + accumulatedBottom)));

                if (i < nonFillChildrenCount)
                {
                    switch (GetDock(child))
                    {
                        case Dock.Left:
                            accumulatedLeft += childDesiredSize.Width;
                            rcChild.Width = childDesiredSize.Width;
                            break;

                        case Dock.Right:
                            accumulatedRight += childDesiredSize.Width;
                            rcChild.X = Math.Max(0.0, finalSize.Width - accumulatedRight);
                            rcChild.Width = childDesiredSize.Width;
                            break;

                        case Dock.Top:
                            accumulatedTop += childDesiredSize.Height;
                            rcChild.Height = childDesiredSize.Height;
                            break;

                        case Dock.Bottom:
                            accumulatedBottom += childDesiredSize.Height;
                            rcChild.Y = Math.Max(0.0, finalSize.Height - accumulatedBottom);
                            rcChild.Height = childDesiredSize.Height;
                            break;
                    }
                }

                child.Arrange(rcChild);
            }

            return finalSize;
        }

        /// <summary>
        /// 测量 DockPanel 的子元素，以便准备在 ArrangeOverride 过程中排列它们。
        /// </summary>
        /// <param name="availableSize">不能超过的最大 Size。</param>
        /// <returns>一个 Size，表示所需的元素大小。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            UIElementCollection children = Children;

            double parentWidth = 0;// 存放所有子元素所需的宽度。
            double parentHeight = 0;// 存放所有子元素所需的高度。
            double accumulatedWidth = 0;// 所有子元素消耗的宽度。
            double accumulatedHeight = 0;// 所有子元素消耗的高度。

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];
                Size childConstraint;// 子元素大小的建议范围。
                Size childDesiredSize;// 子元素大小请求的范围。

                if (child == null)
                {
                    continue;
                }

                childConstraint = new Size(Math.Max(0.0, availableSize.Width - accumulatedWidth), Math.Max(0.0, availableSize.Height - accumulatedHeight));

                child.Measure(childConstraint);
                childDesiredSize = child.DesiredSize;

                switch (GetDock(child))
                {
                    case Dock.Left:
                    case Dock.Right:
                        parentHeight = Math.Max(parentHeight, accumulatedHeight + childDesiredSize.Height);
                        accumulatedWidth += childDesiredSize.Width;
                        break;

                    case Dock.Top:
                    case Dock.Bottom:
                        parentWidth = Math.Max(parentWidth, accumulatedWidth + childDesiredSize.Width);
                        accumulatedHeight += childDesiredSize.Height;
                        break;
                }
            }

            parentWidth = Math.Max(parentWidth, accumulatedWidth);
            parentHeight = Math.Max(parentHeight, accumulatedHeight);

            return (new Size(parentWidth, parentHeight));
        }

        private static void OnDockPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Dock value = (Dock)e.NewValue;
            if (Enum.IsDefined(typeof(Dock), value) == false)
            {
                d.SetValue(e.Property, (Dock)e.OldValue);

                throw new ArgumentException("Dock 未定义。");
            }

            UIElement uie = d as UIElement;// d 有可能为非 UIElement，例如 Run。
            if (uie != null)
            {
                DockPanel p = VisualTreeHelper.GetParent(uie) as DockPanel;
                p?.InvalidateMeasure();
            }
        }

        private static void OnLastChildFillPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DockPanel source = (DockPanel)d;
            source.InvalidateArrange();
        }
    }
}