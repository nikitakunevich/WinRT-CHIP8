using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CHIP8_VM.Common
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Visibility))
            {
                throw new NotSupportedException(string.Format("Type {0} is not supported", targetType));
            }
            bool visible = (bool)value;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(bool))
            {
                throw new NotSupportedException(string.Format("Type {0} is not supported", targetType));
            }
            Visibility visible = (Visibility)value;
            return visible == Visibility.Visible;

        }
        #endregion
    }
}