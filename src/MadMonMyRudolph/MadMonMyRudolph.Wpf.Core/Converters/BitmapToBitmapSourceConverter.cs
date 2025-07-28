namespace MadMonMyRudolph.Wpf.Core.Converters;

/// <summary>
/// System.Drawing.Bitmap을 WPF BitmapSource로 변환하는 컨버터
/// </summary>
[ValueConversion(typeof(Bitmap), typeof(BitmapSource))]
public sealed class BitmapToBitmapSourceConverter : ConverterMarkupExtension<BitmapToBitmapSourceConverter>
{
    public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Bitmap bitmap)
            return BitmapToBitmapSource(bitmap);

        return null;
    }

    public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }
}