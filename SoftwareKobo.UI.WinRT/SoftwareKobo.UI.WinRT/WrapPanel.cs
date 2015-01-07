using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SoftwareKobo.UI.WinRT
{
    /// <summary>
    /// 从左至右按顺序位置定位子元素，在包含框的边缘处将内容断开至下一行。后续排序按照从上至下或从右至左的顺序进行，具体取决于 WrapPanel.Orientation 属性的值。
    /// </summary>
    public class WrapPanel : Panel
    {
        private Orientation _orientation;

        /// <summary>
        /// 初始化 WrapPanel 类的新实例。
        /// </summary>
        public WrapPanel()
            : base()
        {
            // TODO
            _orientation = Orientation.Horizontal;
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(WrapPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO
        }

        public Orientation Orientation
        {
            get;
            set;
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
        /// 标识 WrapPanel.ItemHeight 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("", typeof(double),
            typeof(WrapPanel), new PropertyMetadata(0));

        /// <summary>
        /// 标识 WrapPanel.ItemWidth 依赖项属性。
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("", typeof(double),
            typeof(WrapPanel), new PropertyMetadata(0));
    }
}