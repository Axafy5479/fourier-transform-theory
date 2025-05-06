
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class Encoder
{
    /// <summary>
    /// 離散コサイン変換行列
    /// </summary>
    private static Matrix C = new Matrix(8, 8);
    private static Matrix Q_map = new Matrix(new double[8, 8]
        {
            { 16,  11,  10,  16,  24,  40,  51,  61 },
            { 12,  12,  14,  19,  26,  58,  60,  55 },
            { 14,  13,  16,  24,  40,  57,  69,  56 },
            { 14,  17,  22,  29,  51,  87,  80,  62 },
            { 18,  22,  37,  56,  68, 109, 103,  77 },
            { 24,  35,  55,  64,  81, 104, 113,  92 },
            { 49,  64,  78,  87, 103, 121, 120, 101 },
            { 72,  92,  95,  98, 112, 100, 103,  99 }
        });

    // 二次元配列 q2 を指定された行列で初期化
    private static Matrix Q_map2 = new Matrix(new double[8, 8]
        {
            { 128,   88,   80,  128,  192,  320,  408,  488 },
            {  96,   96,  112,  152,  208,  464,  480,  440 },
            { 112,  104,  128,  192,  320,  456,  552,  448 },
            { 112,  136,  176,  232,  408,  696,  640,  496 },
            { 144,  176,  296,  448,  544,  872,  824,  616 },
            { 192,  280,  440,  512,  648,  832,  904,  736 },
            { 392,  512,  624,  696,  824,  968,  960,  808 },
            { 576,  736,  760,  784,  896,  800,  824,  792 }
        });

    /// <summary>
    /// 離散コサイン行列の計算式
    /// </summary>
    /// <param name="k"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private static double C_func(int k, int n)
    {
        if (k == 0)
        {
            return 1 / Math.Sqrt(8);
        }

        return 1 / 2.0 * Math.Cos(Math.PI * (2 * n + 1) * k / 16.0);
    }
    public static void Main()
    {
        // 用いる量子化テーブル (圧縮率が決まる)
        var Q = Q_map;

        // 離散コサイン変換行列の作成
        // 各々の要素を計算
        for (int k = 0; k < 8; k++)
        {
            for (int n = 0; n < 8; n++)
            {
                C[n, k] = C_func(n, k);
            }
        }

        // 画像の読み込み
        var img = (Matrix)LoadImageAsGrayscaleWithImageSharp("img.png");
        SaveGrayscaleArrayAsPngWithImageSharp(img.GetBytes(),"original.png");

        Matrix img_DCT = new Matrix(img.Height, img.Width);
        Matrix img_reconstructed = new Matrix(img.Height, img.Width);

        List<string> transformed_arrays = new List<string>();

        for (int group_h = 0; group_h < img.Height / 8; group_h++)
        {
            for (int group_w = 0; group_w < img.Width / 8; group_w++)
            {
                // 8x8 の画像を抜き出す
                var img_8x8 = new Matrix(8, 8);
                for (int h = 0; h < 8; h++)
                {
                    for (int w = 0; w < 8; w++)
                    {
                        img_8x8[h, w] = img[group_h * 8 + h, group_w * 8 + w];
                    }
                }


                // -------------変換本体--------------
                img_8x8 -= 128;
                var img_transformed = C * img_8x8 * C.GetTranspose(); // 離散コサイン変換 ( C*P*Ct )
                img_transformed /= Q; // 量子化テーブルで割る

                // 情報を落とす (四捨五入)
                img_transformed = img_transformed.Round();

                var img_array = img_transformed.ZigZagRead();
                transformed_arrays.Add(string.Join(" ",Array.ConvertAll(img_array,d=>(int)d)));

                img_transformed = Matrix.MultiplyElements(img_transformed, Q); // 量子化テーブルを掛ける
                var reconstructed = C.GetTranspose() * img_transformed * C; // 離散コサイン逆変換 ( Ct*P*C )
                reconstructed += 128;


                // 変換後の8x8画像を
                for (int h = 0; h < 8; h++)
                {
                    for (int w = 0; w < 8; w++)
                    {
                        img_DCT[group_h * 8 + h, group_w * 8 + w] = img_transformed[h, w];
                        img_reconstructed[group_h * 8 + h, group_w * 8 + w] = reconstructed[h, w];
                    }
                }
            }
            Console.WriteLine(group_h + "/" + img.Height/8);
        }

        using StreamWriter sw = new("./arrays.txt", false);
        sw.Write(string.Join("\n", transformed_arrays));
        sw.Close();

        SaveGrayscaleArrayAsPngWithImageSharp(img_DCT.Abs().GetBytes(), "temp.png");
        SaveGrayscaleArrayAsPngWithImageSharp(img_reconstructed.GetBytes(), "reconstructed.png");
    }

    /// <summary>
    /// ImageSharpを使用して、指定された画像ファイルを読み込み、グレースケール値の2次元配列を返します。
    /// </summary>
    /// <param name="filePath">画像ファイルのパス</param>
    /// <returns>グレースケール値 (0-255) の2次元配列 (byte[高さ, 幅])。失敗した場合はnull。</returns>
    public static Matrix? LoadImageAsGrayscaleWithImageSharp(string filePath)
    {
        try
        {
            // ImageSharpで画像を読み込む (一般的なRGBA32形式として読み込む例)
            // 必要に応じて <TPixel> を変更可能 (例: Rgb24)
            using (Image<Rgba32> image = Image.Load<Rgba32>(filePath))
            {
                int width = image.Width;
                int height = image.Height;
                byte[,] grayscaleArray = new byte[height, width]; // [行(高さ), 列(幅)]

                // ImageSharpでは image[x, y] でピクセルに直接アクセスできる (比較的低速な場合あり)
                // または、より高速な ProcessPixelRows を使うことも可能
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // ピクセルの色を取得 (Rgba32形式)
                        Rgba32 pixelColor = image[x, y];

                        // グレースケール値を計算 (輝度 Luminance: BT.601 標準)
                        // Gray = 0.299 * R + 0.587 * G + 0.114 * B
                        // ImageSharpの R, G, B は byte (0-255)
                        int grayValue = (int)(pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114);

                        // 値を 0-255 の範囲にクリッピング
                        grayValue = Math.Max(0, Math.Min(255, grayValue));

                        grayscaleArray[y, x] = (byte)grayValue;
                    }
                }

                var bytes = new double[height, width];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        bytes[i,j] = grayscaleArray[i,j];
                    }
                }

                return new Matrix(bytes);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"エラー: 画像ファイルが見つかりません '{filePath}'");
            return null;
        }
        catch (UnknownImageFormatException) // ImageSharp固有の例外
        {
            Console.WriteLine($"エラー: '{filePath}' はサポートされていない、または無効な画像形式です。");
            return null;
        }
        catch (Exception ex) // その他の予期せぬエラー
        {
            Console.WriteLine($"予期せぬエラーが発生しました: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// ImageSharpを使用して、グレースケール値の2次元配列をPNG画像として保存します。
    /// </summary>
    /// <param name="grayscaleArray">グレースケール値 (0-255) の2次元配列 (byte[高さ, 幅])。</param>
    /// <param name="filePath">保存するPNGファイルのパス。</param>
    /// <returns>成功した場合は true、失敗した場合は false。</returns>
    public static bool SaveGrayscaleArrayAsPngWithImageSharp(byte[,] grayscaleArray, string filePath)
    {
        if (grayscaleArray == null || grayscaleArray.Length == 0)
        {
            Console.WriteLine("エラー: 入力配列が null または空です。");
            return false;
        }

        int height = grayscaleArray.GetLength(0); // 配列の0番目の次元 = 高さ
        int width = grayscaleArray.GetLength(1);  // 配列の1番目の次元 = 幅

        if (width <= 0 || height <= 0)
        {
            Console.WriteLine("エラー: 配列の次元が無効です。");
            return false;
        }

        try
        {
            // 8ビットグレースケール形式 (L8) で新しい画像を作成
            using (Image<L8> image = new Image<L8>(width, height))
            {
                // ImageSharp のピクセルアクセスは image[x, y] の順序
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // byte 配列から値を取得し、L8 ピクセルとして設定
                        byte grayValue = grayscaleArray[y, x];
                        image[x, y] = new L8(grayValue);
                    }
                }

                /* --- 参考: ProcessPixelRows を使った高速な方法 ---
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Span<L8> pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < width; x++)
                        {
                            pixelRow[x] = new L8(grayscaleArray[y, x]);
                        }
                    }
                });
                */

                // 画像をPNGファイルとして保存
                image.SaveAsPng(filePath);
                Console.WriteLine($"画像を '{filePath}' に保存しました (ImageSharp)。");
                return true;
            }
        }
        catch (Exception ex) // ファイルアクセス権限エラーなども含む
        {
            Console.WriteLine($"画像の保存中にエラーが発生しました: {ex.Message}");
            return false;
        }
    }

}
