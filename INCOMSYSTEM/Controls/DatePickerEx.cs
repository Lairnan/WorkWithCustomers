using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace INCOMSYSTEM.Controls
{
    public class DatePickerEx : DatePicker
    {
        static DatePickerEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DatePickerEx), new FrameworkPropertyMetadata(typeof(DatePickerEx)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var popup = GetTemplateChild("PART_Popup") as Popup;
            if (popup != null)
                ApplyCustomTemplate(popup);
        }

        public bool IsWrong
        {
            get => (bool)GetValue(IsWrongProperty);
            set => SetValue(IsWrongProperty, value);
        }

        public static readonly DependencyProperty IsWrongProperty =
            DependencyProperty.Register(nameof(IsWrong), typeof(bool),
                typeof(DatePickerEx), new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null,
                null, false, UpdateSourceTrigger.PropertyChanged));

        void ApplyCustomTemplate(Popup popup)
        {
            var calendar = popup.Child as Calendar;
            if (calendar == null) return;
            calendar.SetResourceReference(Calendar.StyleProperty, "DatePickerEx_CustomPopup");
        }
    }
}
