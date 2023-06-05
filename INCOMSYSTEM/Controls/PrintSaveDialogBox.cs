using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace INCOMSYSTEM.Controls
{
    public enum PrintSaveDialogBoxResult { None, Print, Save, Cancel };

    public static class PrintSaveDialogBox
    {
        public static PrintSaveDialogBoxResult Show(string message)
        {
            var result = PrintSaveDialogBoxResult.None;

            var dialog = new Window
            {
                Title = "Выберите действие",
                Width = 400,
                Height = 150,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(105, 105, 105)),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stack = new StackPanel();
            dialog.Content = stack;

            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(10),
                Foreground = new SolidColorBrush(Color.FromRgb(255,255,255)),
                FontSize = 14,
                FontFamily = new FontFamily("Roboto"),
            };
            stack.Children.Add(textBlock);

            var buttonPanel = new WrapPanel
            {
                Margin = new Thickness(10)
            };
            stack.Children.Add(buttonPanel);

            var btnPrint = new Button
            {
                Content = "Печатать",
                Margin = new Thickness(5)
            };
            btnPrint.Click += (sender, e) => { result = PrintSaveDialogBoxResult.Print; dialog.Close(); };
            buttonPanel.Children.Add(btnPrint);

            var btnSave = new Button
            {
                Content = "Сохранить",
                Margin = new Thickness(5)
            };
            btnSave.Click += (sender, e) => { result = PrintSaveDialogBoxResult.Save; dialog.Close(); };
            buttonPanel.Children.Add(btnSave);

            var btnCancel = new Button
            {
                Content = "Отменить",
                Margin = new Thickness(5)
            };
            btnCancel.Click += (sender, e) => { result = PrintSaveDialogBoxResult.Cancel; dialog.Close(); };
            buttonPanel.Children.Add(btnCancel);

            dialog.ShowDialog();
            return result;
        }
    }
}