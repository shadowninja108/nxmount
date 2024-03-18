using Avalonia.Data.Converters;
using Avalonia.Media;

namespace nxmount.Frontend.Util
{
    public static class ErrorBorderConverter
    {
        public static FuncValueConverter<bool, IBrush> Converter { get; } = new(Convert);


        public static IBrush Convert(bool error)
        {
            if (error)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Transparent;
            }
        }
    }
}
