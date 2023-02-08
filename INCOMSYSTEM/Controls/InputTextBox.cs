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

        public bool IsShowed
        {
            get => (bool)GetValue(IsShowedProperty);
            set => SetValue(IsShowedProperty, value);
        }

        public static readonly DependencyProperty IsShowedProperty =
            DependencyProperty.Register(nameof(IsShowed), typeof(bool), typeof(InputTextBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    IsShowedPropertyChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        private static void IsShowedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is InputTextBox inputTextBox)) return;

            inputTextBox.isChanged = true;
            if ((bool)e.NewValue) inputTextBox.Text = inputTextBox.Value;
            else
            {
                var text = inputTextBox.Text;
                foreach(var c in text)
                {
                    text = text.Replace(c, '●');
                }
                inputTextBox.Text = text;
            }
        }

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(InputTextBox),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PlaceHolderPropertyChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        private static void PlaceHolderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is InputTextBox inputTextBox)) return;
            inputTextBox.IsPlaceHolder = !string.IsNullOrWhiteSpace((string)e.NewValue);
            inputTextBox.Text = (string)e.NewValue;
        }

        public InputTextBox()
        {
            GotFocus += LostFocusText;
            LostFocus += GotFocusText;

            TextChanged += OnTextChanged;
        }

        public bool isChanged = false;

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsPlaceHolder) return;
            if (IsShowed)
            {
                Value = base.Text;
                return;
            }
            if (!IsPassword) Value = base.Text;
            else
            {
                isChanged = !isChanged;
                if (string.IsNullOrWhiteSpace(Text)) return;
                if (isChanged)
                {
                    var start = SelectionStart;
                    var r = base.Text[start - 1];
                    Value = Value.Insert(start - 1, r.ToString());
                    base.Text = base.Text.Replace(r, '●');
                    base.SelectionStart = start;
                }
            }
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
