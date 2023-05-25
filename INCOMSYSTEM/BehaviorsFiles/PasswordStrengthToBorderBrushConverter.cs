using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public class PasswordStrengthToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var password = value as string;
            if (string.IsNullOrWhiteSpace(password))
            {
                return Brushes.White;
            }
            
            var passwordStrength = PasswordDifficulty.CheckDifficultyPassword(password);
            switch (passwordStrength)
            {
                case DifficultyPassword.Hard:
                    return Brushes.Green;
                case DifficultyPassword.Medium:
                    return Brushes.Yellow;
                case DifficultyPassword.Easy:
                default:
                    return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}