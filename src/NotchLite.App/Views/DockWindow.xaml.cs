using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NotchLite.App.ViewModels;
using NotchLite.Platform.Win32;

namespace NotchLite.App.Views;

public partial class DockWindow : Window
{
    private readonly DockViewModel _viewModel;

    public DockWindow(DockViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        NativeMethods.SetToolWindowNoActivate(hwnd);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = (screenWidth - ActualWidth) / 2;
        Top = screenHeight - ActualHeight - 8;
    }

    private void OnWindowMouseEnter(object sender, MouseEventArgs e)
    {
        // Smooth slide up if autohide
        var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        DockTranslate.BeginAnimation(TranslateTransform.YProperty, anim);
    }

    private void OnWindowMouseLeave(object sender, MouseEventArgs e)
    {
        ResetAllMagnifications();
    }

    private void OnWindowMouseMove(object sender, MouseEventArgs e)
    {
        var mousePos = e.GetPosition(DockItemsControl);

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(DockItemsControl); i++)
        {
            var child = VisualTreeHelper.GetChild(DockItemsControl, i);
            FindAndMagnifyItems(child, mousePos.X);
        }
    }

    private void FindAndMagnifyItems(DependencyObject obj, double mouseX)
    {
        if (obj is Grid grid && grid.Tag is DockItem)
        {
            try
            {
                var pt = grid.TranslatePoint(new Point(grid.ActualWidth / 2, 0), DockItemsControl);
                var dist = Math.Abs(mouseX - pt.X);
                const double maxDist = 80.0;

                double targetScale = 1.0;
                if (dist < maxDist)
                {
                    var factor = Math.Cos((dist / maxDist) * (Math.PI / 2.0));
                    targetScale = 1.0 + (0.42 * factor);
                }

                if (grid.RenderTransform is TransformGroup group &&
                    group.Children.FirstOrDefault(t => t is ScaleTransform) is ScaleTransform st)
                {
                    var anim = new DoubleAnimation(targetScale, TimeSpan.FromMilliseconds(80))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
            }
            catch
            {
                // Fallback safe
            }
        }

        var count = VisualTreeHelper.GetChildrenCount(obj);
        for (var i = 0; i < count; i++)
        {
            FindAndMagnifyItems(VisualTreeHelper.GetChild(obj, i), mouseX);
        }
    }

    private void ResetAllMagnifications()
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(DockItemsControl); i++)
        {
            var child = VisualTreeHelper.GetChild(DockItemsControl, i);
            ResetScalesRecursively(child);
        }
    }

    private static void ResetScalesRecursively(DependencyObject obj)
    {
        if (obj is Grid grid && grid.Tag is DockItem &&
            grid.RenderTransform is TransformGroup group &&
            group.Children.FirstOrDefault(t => t is ScaleTransform) is ScaleTransform st)
        {
            var anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        var count = VisualTreeHelper.GetChildrenCount(obj);
        for (var i = 0; i < count; i++)
        {
            ResetScalesRecursively(VisualTreeHelper.GetChild(obj, i));
        }
    }

    private void OnDockItemClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is Grid grid && grid.Tag is DockItem item)
        {
            // Launch Bounce Animation
            if (grid.RenderTransform is TransformGroup group &&
                group.Children.FirstOrDefault(t => t is TranslateTransform) is TranslateTransform tt)
            {
                var bounceAnim = new DoubleAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromMilliseconds(360)
                };
                bounceAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0.0)));
                bounceAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-16, KeyTime.FromPercent(0.3), new CubicEase { EasingMode = EasingMode.EaseOut }));
                bounceAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0.6), new CubicEase { EasingMode = EasingMode.EaseIn }));
                bounceAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-7, KeyTime.FromPercent(0.8), new CubicEase { EasingMode = EasingMode.EaseOut }));
                bounceAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1.0), new CubicEase { EasingMode = EasingMode.EaseIn }));

                tt.BeginAnimation(TranslateTransform.YProperty, bounceAnim);
            }

            _viewModel.LaunchApp(item);
        }
    }
}
