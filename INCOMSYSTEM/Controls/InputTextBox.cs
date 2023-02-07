using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace INCOMSYSTEM.Controls
{
    class InputTextBox : TextBox
    {
        public bool IsNull
        {
            get => (bool)GetValue(IsNullProperty);
            set => SetValue(IsNullProperty, value);
        }

        public static readonly DependencyProperty IsNullProperty =
            DependencyProperty.Register(nameof(IsNull), typeof(bool), typeof(InputTextBox),
                new PropertyMetadata(true));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(InputTextBox),
                new PropertyMetadata(""));

        public SecureString Password
        {
            get => (SecureString)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(SecureString), typeof(InputTextBox),
                new FrameworkPropertyMetadata(new SecureString(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged));

        public string RealPassword
        {
            get => (string)GetValue(RealPasswordProperty);
            set => SetValue(RealPasswordProperty, value);
        }

        public static readonly DependencyProperty RealPasswordProperty =
            DependencyProperty.Register(nameof(RealPassword), typeof(string), typeof(InputTextBox),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged));

        public bool IsPlaceHolder
        {
            get => (bool)GetValue(IsPlaceHolderProperty);
            set => SetValue(IsPlaceHolderProperty, value);
        }

        public static readonly DependencyProperty IsPlaceHolderProperty =
            DependencyProperty.Register(nameof(IsPlaceHolder), typeof(bool), typeof(InputTextBox),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged));

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register(nameof(IsPassword), typeof(bool), typeof(InputTextBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null, null, false, UpdateSourceTrigger.PropertyChanged));

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(InputTextBox),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPropertyTextChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        private static void OnPropertyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is InputTextBox inputTextBox)) return;
            inputTextBox.Value = (string)e.NewValue;
        }

        public InputTextBox()
        {
            GotFocus += LostFocusText;
            LostFocus += GotFocusText;
            
            TextChanged += OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsPlaceHolder) return;

            Value = base.Text;
        }

        private void GotFocusText(object sender, RoutedEventArgs e) => SetPlaceHolder();

        private void LostFocusText(object sender, RoutedEventArgs e) => RemovePlaceHolder();

        private void SetPlaceHolder()
        {
            if (string.IsNullOrWhiteSpace(base.Text))
            {
                Value = string.Empty;
                base.Text = PlaceHolder;
                IsPlaceHolder = true;

                IsNull = true;
            }
            else IsNull = false;
        }

        private void RemovePlaceHolder()
        {
            if (IsPlaceHolder)
            {
                base.Text = string.Empty;
                IsPlaceHolder = false;
            }

            IsNull = string.IsNullOrWhiteSpace(base.Text);
        }
        
        // ●
    }
}
