using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static System.Windows.TextDecorations;

namespace ToDoList.Converters
{
    public class CompletedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted && isCompleted)
            {
                return Strikethrough;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 