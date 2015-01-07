using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SoftwareKobo.UI.WinRT.Controls
{
    public enum Dock
    {
        Left,
        Top,
        Right,
        Bottom
    }

    public class DockPanel : Panel
    {
        public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(nameof(Dock), typeof(Dock), typeof(DockPanel), new PropertyMetadata(Dock.Left, OnDockPropertyChanged));

        public static readonly DependencyProperty LastChildFillProperty = DependencyProperty.Register(nameof(LastChildFill), typeof(bool), typeof(DockPanel), new PropertyMetadata(true, OnLastChildFillPropertyChanged));

        public DockPanel()
            : base()
        {
        }

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

        public static Dock GetDock(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return (Dock)element.GetValue(DockProperty);
        }

        public static void SetDock(UIElement element, Dock dock)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            element.SetValue(DockProperty, dock);
        }

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
                Rect rcChild = new Rect(
                    accumulatedLeft,
                    accumulatedTop,
                    Math.Max(0.0, finalSize.Width - (accumulatedLeft + accumulatedRight)),
                    Math.Max(0.0, finalSize.Height - (accumulatedTop + accumulatedBottom)));

                if (i < nonFillChildrenCount)
                {
                    switch (DockPanel.GetDock(child))
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

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElementCollection children = Children;

            double parentWidth = 0;
            double parentHeight = 0;
            double accumulatedWidth = 0;
            double accumulatedHeight = 0;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];
                Size childConstraint;
                Size childDesiredSize;

                if (child == null)
                {
                    continue;
                }

                childConstraint = new Size(Math.Max(0.0, availableSize.Width - accumulatedWidth),
                                           Math.Max(0.0, availableSize.Height - accumulatedHeight));

                child.Measure(childConstraint);
                childDesiredSize = child.DesiredSize;

                switch (DockPanel.GetDock(child))
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

                throw new InvalidOperationException("Dock 未定义。");
            }

            UIElement uie = d as UIElement;
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