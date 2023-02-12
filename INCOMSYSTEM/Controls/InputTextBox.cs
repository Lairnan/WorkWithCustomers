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

            inputTextBox._isChanged = false;
            if ((bool)e.NewValue)
            {
                var start = inputTextBox.SelectionStart;
                inputTextBox.Text = inputTextBox.Value;
                inputTextBox.SelectionStart = start;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(inputTextBox.Value)) return;
                var start = inputTextBox.SelectionStart;
                var text = inputTextBox.Text;
                foreach (var c in text)
                {
                    text = text.Replace(c, '●');
                }
                inputTextBox.Text = text;
                inputTextBox.SelectionStart = start;
            }
            inputTextBox._isChanged = true;
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
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    if (SelectionLength > 0)
                    {
                        _isRemove = true;
                        RemoveFromSecureString(SelectionStart, SelectionLength);
                    }
                    else switch (e.Key)
                    {
                        case Key.Delete when SelectionStart < Text.Length:
                            _isRemove = true;
                            RemoveFromSecureString(SelectionStart, 1);
                            break;
                        case Key.Back when SelectionStart > 0:
                            _isRemove = true;
                            var caretIndex = SelectionStart;
                            if (SelectionStart > 0 && SelectionStart < Text.Length)
                                caretIndex -= 1;
                            RemoveFromSecureString(SelectionStart - 1, 1);
                            SelectionStart = caretIndex;
                            break;
                        default:
                            _isRemove = false;
                            break;
                    }

                    e.Handled = true;
                    break;

                default:
                    _isChanged = true;
                    _isRemove = false;
                    break;
            }
        }

        private bool _isRemove = false;
        private bool _isChanged = false;

        private readonly object _lock = new object();

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            lock (Value)
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
                    if (!_isRemove && _isChanged)
                    {
                        _isChanged = false;
                        var start = SelectionStart;
                        var r = base.Text[start - 1];
                        Value = Value.Insert(start - 1, r.ToString());
                        base.Text = base.Text.Replace(r, '●');
                        base.SelectionStart = start;
                    }

                }

            }
        }

        private void RemoveFromSecureString(int startIndex, int trimLength)
        {
            if (string.IsNullOrWhiteSpace(Value)) return;
            _isChanged = false;
            _isRemove = true;

            var caretIndex = SelectionStart;
            Value = Value.Remove(startIndex, trimLength);
            Text = Text.Remove(startIndex, trimLength);
            
            SelectionStart = caretIndex;
        }

        private void GotFocusText(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(base.Text))
            {
                _isChanged = false;
                Value = string.Empty;
                base.Text = PlaceHolder;
                IsPlaceHolder = true;

                IsNull = true;
            }
            else IsNull = false;
        }

        private void LostFocusText(object sender, RoutedEventArgs e)
        {
            if (IsPlaceHolder)
            {
                _isChanged = false;
                base.Text = string.Empty;
                IsPlaceHolder = false;
            }

            IsNull = string.IsNullOrWhiteSpace(base.Text);
        }
    }
}
