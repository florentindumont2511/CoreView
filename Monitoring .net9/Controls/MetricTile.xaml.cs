using System.Windows;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace Monitoring_net9.Controls
{
    public partial class MetricTile : WpfUserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(MetricTile),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(MetricTile),
                new PropertyMetadata("--"));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(
                nameof(Unit),
                typeof(string),
                typeof(MetricTile),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register(
                nameof(ValueBrush),
                typeof(MediaBrush),
                typeof(MetricTile),
                new PropertyMetadata(MediaBrushes.White));

        public static readonly DependencyProperty TileBrushProperty =
            DependencyProperty.Register(
                nameof(TileBrush),
                typeof(MediaBrush),
                typeof(MetricTile),
                new PropertyMetadata(
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(37, 37, 37))));

        public static readonly DependencyProperty TitleBrushProperty =
            DependencyProperty.Register(
                nameof(TitleBrush),
                typeof(MediaBrush),
                typeof(MetricTile),
                new PropertyMetadata(
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(168, 168, 168))));

        public static readonly DependencyProperty UnitBrushProperty =
            DependencyProperty.Register(
                nameof(UnitBrush),
                typeof(MediaBrush),
                typeof(MetricTile),
                new PropertyMetadata(
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(192, 192, 192))));

        public MetricTile()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public MediaBrush ValueBrush
        {
            get => (MediaBrush)GetValue(ValueBrushProperty);
            set => SetValue(ValueBrushProperty, value);
        }

        public MediaBrush TileBrush
        {
            get => (MediaBrush)GetValue(TileBrushProperty);
            set => SetValue(TileBrushProperty, value);
        }

        public MediaBrush TitleBrush
        {
            get => (MediaBrush)GetValue(TitleBrushProperty);
            set => SetValue(TitleBrushProperty, value);
        }

        public MediaBrush UnitBrush
        {
            get => (MediaBrush)GetValue(UnitBrushProperty);
            set => SetValue(UnitBrushProperty, value);
        }
    }
}
