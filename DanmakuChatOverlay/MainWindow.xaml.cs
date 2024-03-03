using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;
using Control = System.Windows.Controls.Control;

namespace DankmakuChatOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Grid grid;
        Window window;
        Screen screen;
        TwitchBot twitch;

        Random rand = new Random();

        System.Windows.Size windowSize = new System.Windows.Size(0,0);

        public MainWindow()
        {
            InitializeComponent();
            grid = this.FindName("Grid") as Grid ?? throw new Exception();

            try
            {
                twitch = new TwitchBot(this);
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to start Twitch client. This probably means your config file is wrong.");
                this.Close();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            window = (Window)sender;
            screen = WindowHelpers.CurrentScreen(window);

            window.Height = screen.Bounds.Height;
            window.Width = screen.Bounds.Width;

            windowSize = new System.Windows.Size(window.Width, window.Height);
            Debug.WriteLine(screen.Bounds);
            CreateDankmaku("Overlay Starting!");
        }

        private void Window_Resized(object sender, SizeChangedEventArgs e)
        {
            windowSize = e.NewSize;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.Close(); }
        }

        public void CreateDankmaku(String text)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = text;
            label.Loaded += AnimateLabelCallback;
            ApplyLabelStyle(label);
            grid.Children.Add(label);

        }

        private void AnimateLabelCallback(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Label label = sender as System.Windows.Controls.Label ?? throw new Exception();
            ValueTuple<double, double> startPos = PickStartingPosition(label);
            double endPos = PickEndingPosition() - label.ActualWidth;
            Duration duration = PickDuration();
            label.RenderTransform = new TranslateTransform(startPos.Item1, startPos.Item2);
            ((Control)sender).Loaded -= AnimateLabelCallback;
            GenerateDoubleAnimation(label, startPos, endPos, duration);
            ScheduleCleanup(label, duration);
        }

        private void ScheduleCleanup(System.Windows.Controls.Control control, Duration duration)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = duration.TimeSpan;
            EventHandler d = null; // must be declared prior to assignment to allow for self-reference
            d = new EventHandler(((object? sender, EventArgs e) =>
                {
                    bool found = false;
                    foreach (Control c in grid.Children)
                    {
                        if (c == control)
                        {
                            grid.Children.Remove(control);
                            found = true;
                            break;
                        }
                    }
                    if (!found) { throw new Exception(); }
                    grid.Children.Remove(control);
                    timer.Stop();
                    timer.Tick -= d;
                }));
            timer.Tick += d;
            timer.Start();
        }

        private void ApplyLabelStyle(System.Windows.Controls.Label label) 
        {
            label.FontSize = 72;
            label.Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff));
            label.IsHitTestVisible = false;
        }
        private ValueTuple<double, double> PickStartingPosition(System.Windows.Controls.Label label)
        {

            return (windowSize.Width,  (rand.NextDouble() - 0.5) * (windowSize.Height - label.ActualHeight));

        }

        private double PickEndingPosition() 
        {
            return rand.NextDouble() * -100.0;
        }

        private Duration PickDuration() { return new Duration(TimeSpan.FromSeconds(3 + 4 * rand.NextDouble())); }

        private void GenerateDoubleAnimation(System.Windows.Controls.Control control, ValueTuple<double, double> startPos, double endX, Duration duration)
        {
            control.RenderTransform = new TranslateTransform(startPos.Item1, startPos.Item2);
            DoubleAnimation anim = new DoubleAnimation(endX, duration);
            control.RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private ThicknessAnimation GenerateAnimation(Thickness startThickness, double endX, Duration duration) 
        {
            Thickness from = new Thickness(startThickness.Left, startThickness.Top, startThickness.Right, startThickness.Bottom);
            Thickness to = new Thickness(endX, startThickness.Top, startThickness.Right, startThickness.Bottom);
            ThicknessAnimation anim = new ThicknessAnimation();
            anim.From = from;
            anim.To = to;
            anim.Duration = duration;
            return anim;
        }

        private Storyboard GenerateStoryboard(AnimationTimeline anim, System.Windows.Controls.Control control)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anim);
            Storyboard.SetTargetName(anim, control.Name);
            Storyboard.SetTargetProperty(anim, new PropertyPath(control.RenderTransform));
            return storyboard;
        }

    }


    public static class WindowHelpers
    {
        public static Screen CurrentScreen(Window window)
        {
            return Screen.FromPoint(new System.Drawing.Point((int)window.Left, (int)window.Top));
        }
    }
}

// https://github.com/PythonistaGuild/TwitchIO?tab=readme-ov-file