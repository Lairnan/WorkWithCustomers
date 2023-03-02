using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace INCOMSYSTEM.Controls
{
    public class BorderEx : Border
    {
        public BorderEx()
        {
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            timer.Interval = TimeSpan.FromSeconds(0.5d);
            timer.Tick += TimerOnTick;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            IsMouseEnter = _isMouseOver;
            timer.Stop();
        }

        private DispatcherTimer timer = new DispatcherTimer();
        private bool _isMouseOver = false;

        public bool IsMouseEnter
        {
            get => (bool)GetValue(IsMouseEnterProperty);
            set => SetValue(IsMouseEnterProperty, value);
        }

        public static readonly DependencyProperty IsMouseEnterProperty =
            DependencyProperty.Register(nameof(IsMouseEnter), typeof(bool), typeof(BorderEx),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged));

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOver = true;
            timer.Start();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseEnter = false;
            timer.Stop();
        }
    }
}