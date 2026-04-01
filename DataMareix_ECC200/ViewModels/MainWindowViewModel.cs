using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZXing;
using ZXing.Common;

namespace DataMareix_ECC200.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // 2mm × 2mm at 300 DPI ≈ 24 × 24 pixels
    private const int SizePixels = 24;
    private const double PrintDpi = 300.0;

    private static readonly char[] AlphaNumChars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    private readonly Random _random = new();

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private Bitmap? _barcodeImage;

    public MainWindowViewModel()
    {
        GenerateCode();
    }

    [RelayCommand]
    private void GenerateCode()
    {
        var chars = new char[6];
        for (int i = 0; i < 6; i++)
            chars[i] = AlphaNumChars[_random.Next(AlphaNumChars.Length)];
        Code = new string(chars);
        GenerateBarcode();
    }

    private void GenerateBarcode()
    {
        if (string.IsNullOrEmpty(Code))
            return;

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.DATA_MATRIX,
            Options = new EncodingOptions
            {
                Width = SizePixels,
                Height = SizePixels,
                Margin = 0
            }
        };

        var pixelData = writer.Write(Code);

        var bitmap = new WriteableBitmap(
            new PixelSize(pixelData.Width, pixelData.Height),
            new Vector(PrintDpi, PrintDpi),
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);

        using var lockedBitmap = bitmap.Lock();
        Marshal.Copy(pixelData.Pixels, 0, lockedBitmap.Address, pixelData.Pixels.Length);

        BarcodeImage = bitmap;
    }
}
