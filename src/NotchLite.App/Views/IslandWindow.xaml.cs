using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using NotchLite.App.ViewModels;
using NotchLite.Core.Models;
using NotchLite.Platform.Win32;

namespace NotchLite.App.Views;

public partial class IslandWindow : Window
{
    private readonly IslandViewModel _viewModel;
    private bool _isDraggingBrightness;
    private bool _isDraggingVolume;

    public IslandWindow(IslandViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // 1. Make window non-activating toolwindow (never steals focus from games or apps)
        var hwnd = new WindowInteropHelper(this).Handle;
        NativeMethods.SetToolWindowNoActivate(hwnd);

        // 2. Position top-center of primary screen
        UpdateWindowPosition();

        // 3. Sync hardware capsule visuals
        UpdateBrightnessCapsuleUI(_viewModel.BrightnessPercent);
        UpdateVolumeCapsuleUI(_viewModel.Volume);
    }

    public void UpdateWindowPosition()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        Left = (screenWidth - Width) / 2;
        Top = _viewModel.DistanceFromTop;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IslandViewModel.IsExpanded))
        {
            ApplyExpansionState(_viewModel.IsExpanded);
        }
        else if (e.PropertyName == nameof(IslandViewModel.CurrentTab))
        {
            UpdateActiveTabContent(_viewModel.CurrentTab);
        }
        else if (e.PropertyName == nameof(IslandViewModel.DistanceFromTop))
        {
            UpdateWindowPosition();
        }
        else if (e.PropertyName == nameof(IslandViewModel.BrightnessPercent))
        {
            UpdateBrightnessCapsuleUI(_viewModel.BrightnessPercent);
        }
        else if (e.PropertyName == nameof(IslandViewModel.Volume))
        {
            UpdateVolumeCapsuleUI(_viewModel.Volume);
        }
        else if (e.PropertyName == nameof(IslandViewModel.IsCapsLockHudVisible))
        {
            if (!_viewModel.IsExpanded)
            {
                var targetWidth = _viewModel.IsCapsLockHudVisible ? 160.0 : (_viewModel.HasActiveMedia ? 240.0 : 190.0);
                var anim = new DoubleAnimation(targetWidth, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                MainShellBorder.BeginAnimation(WidthProperty, anim);
            }
        }
    }

    private void ApplyExpansionState(bool expanded)
    {
        var duration = TimeSpan.FromMilliseconds(280);
        var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
        var easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };

        if (expanded)
        {
            UpdateActiveTabContent(_viewModel.CurrentTab);
            ExpandedGrid.Visibility = Visibility.Visible;

            var widthAnim = new DoubleAnimation(640, duration) { EasingFunction = easeOut };
            var heightAnim = new DoubleAnimation(280, duration) { EasingFunction = easeOut };
            var restingFade = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(100)) { EasingFunction = easeOut };
            var expandedFade = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(240)) { EasingFunction = easeOut };

            restingFade.Completed += (_, _) =>
            {
                if (_viewModel.IsExpanded)
                {
                    RestingGrid.Visibility = Visibility.Collapsed;
                }
            };

            MainShellBorder.BeginAnimation(WidthProperty, widthAnim);
            MainShellBorder.BeginAnimation(HeightProperty, heightAnim);
            ShadowBorder.BeginAnimation(WidthProperty, widthAnim);
            ShadowBorder.BeginAnimation(HeightProperty, heightAnim);

            RestingGrid.BeginAnimation(OpacityProperty, restingFade);
            ExpandedGrid.BeginAnimation(OpacityProperty, expandedFade);
        }
        else
        {
            RestingGrid.Visibility = Visibility.Visible;

            var widthAnim = new DoubleAnimation(220, TimeSpan.FromMilliseconds(220)) { EasingFunction = easeOut };
            var heightAnim = new DoubleAnimation(38, TimeSpan.FromMilliseconds(220)) { EasingFunction = easeOut };
            var restingFade = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(180)) { EasingFunction = easeOut };
            var expandedFade = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(120)) { EasingFunction = easeIn };

            expandedFade.Completed += (_, _) =>
            {
                if (!_viewModel.IsExpanded)
                {
                    ExpandedGrid.Visibility = Visibility.Collapsed;
                }
            };

            MainShellBorder.BeginAnimation(WidthProperty, widthAnim);
            MainShellBorder.BeginAnimation(HeightProperty, heightAnim);
            ShadowBorder.BeginAnimation(WidthProperty, widthAnim);
            ShadowBorder.BeginAnimation(HeightProperty, heightAnim);

            ExpandedGrid.BeginAnimation(OpacityProperty, expandedFade);
            RestingGrid.BeginAnimation(OpacityProperty, restingFade);
        }
    }

    private void UpdateActiveTabContent(IslandTab tab)
    {
        TabMusicContent.Visibility = tab == IslandTab.Music ? Visibility.Visible : Visibility.Collapsed;
        TabWebContent.Visibility = tab == IslandTab.Web ? Visibility.Visible : Visibility.Collapsed;
        TabShelfContent.Visibility = tab == IslandTab.Shelf ? Visibility.Visible : Visibility.Collapsed;
        TabClipboardContent.Visibility = tab == IslandTab.Clipboard ? Visibility.Visible : Visibility.Collapsed;
        TabWidgetsContent.Visibility = tab == IslandTab.Widgets ? Visibility.Visible : Visibility.Collapsed;
        TabControlsContent.Visibility = tab == IslandTab.Controls ? Visibility.Visible : Visibility.Collapsed;
        TabActionsContent.Visibility = tab == IslandTab.Actions ? Visibility.Visible : Visibility.Collapsed;
        TabDeckContent.Visibility = tab == IslandTab.Deck ? Visibility.Visible : Visibility.Collapsed;
        TabCallsContent.Visibility = tab == IslandTab.Calls ? Visibility.Visible : Visibility.Collapsed;

        // Reset and update active indicator dots
        DotMusic.Visibility = tab == IslandTab.Music ? Visibility.Visible : Visibility.Collapsed;
        DotWeb.Visibility = tab == IslandTab.Web ? Visibility.Visible : Visibility.Collapsed;
        DotNotices.Visibility = tab == IslandTab.Notices ? Visibility.Visible : Visibility.Collapsed;
        DotShelf.Visibility = tab == IslandTab.Shelf ? Visibility.Visible : Visibility.Collapsed;
        DotClipboard.Visibility = tab == IslandTab.Clipboard ? Visibility.Visible : Visibility.Collapsed;
        DotWidgets.Visibility = tab == IslandTab.Widgets ? Visibility.Visible : Visibility.Collapsed;
        DotControls.Visibility = tab == IslandTab.Controls ? Visibility.Visible : Visibility.Collapsed;
        DotActions.Visibility = tab == IslandTab.Actions ? Visibility.Visible : Visibility.Collapsed;
        DotDeck.Visibility = tab == IslandTab.Deck ? Visibility.Visible : Visibility.Collapsed;
        DotCalls.Visibility = tab == IslandTab.Calls ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnRestingGridMouseDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.Expand();
    }

    private void OnWindowMouseEnter(object sender, MouseEventArgs e)
    {
        _viewModel.CancelCollapseTimer();
    }

    private void OnWindowMouseLeave(object sender, MouseEventArgs e)
    {
        _isDraggingBrightness = false;
        _isDraggingVolume = false;

        if (_viewModel.IsExpanded)
        {
            _viewModel.StartCollapseTimer();
        }
    }

    // Interactive Capsule Slider Handlers
    private void OnBrightnessCapsuleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement el)
        {
            _isDraggingBrightness = true;
            el.CaptureMouse();
            UpdateBrightnessFromPosition(e.GetPosition(el).Y, el.ActualHeight);
        }
    }

    private void OnBrightnessCapsuleMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingBrightness && sender is FrameworkElement el)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateBrightnessFromPosition(e.GetPosition(el).Y, el.ActualHeight);
            }
            else
            {
                _isDraggingBrightness = false;
                el.ReleaseMouseCapture();
            }
        }
    }

    private void UpdateBrightnessFromPosition(double y, double totalHeight)
    {
        if (totalHeight <= 0) return;
        var fraction = 1.0 - Math.Clamp(y / totalHeight, 0.0, 1.0);
        _viewModel.SetBrightness(fraction * 100);
    }

    private void UpdateBrightnessCapsuleUI(int percent)
    {
        var height = (percent / 100.0) * 140.0;
        BrightnessFillBorder.Height = Math.Max(18, height);
    }

    private void OnVolumeCapsuleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement el)
        {
            _isDraggingVolume = true;
            el.CaptureMouse();
            UpdateVolumeFromPosition(e.GetPosition(el).Y, el.ActualHeight);
        }
    }

    private void OnVolumeCapsuleMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingVolume && sender is FrameworkElement el)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateVolumeFromPosition(e.GetPosition(el).Y, el.ActualHeight);
            }
            else
            {
                _isDraggingVolume = false;
                el.ReleaseMouseCapture();
            }
        }
    }

    private void UpdateVolumeFromPosition(double y, double totalHeight)
    {
        if (totalHeight <= 0) return;
        var fraction = 1.0 - Math.Clamp(y / totalHeight, 0.0, 1.0);
        _viewModel.SetVolume(fraction * 100);
    }

    private void UpdateVolumeCapsuleUI(int percent)
    {
        var height = (percent / 100.0) * 140.0;
        VolumeFillBorder.Height = Math.Max(18, height);
    }

    // Drag-and-Drop for Shelf
    private void OnShelfDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private void OnShelfDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                _viewModel.DropFiles(files);
            }
        }
    }
}
