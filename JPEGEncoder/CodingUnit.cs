public struct Matrix
{
    public Matrix(double[,] init)
    {
        Data = init;
    }

    public Matrix(int h, int w)
    {
        Data = new double[h, w];
    }

    public double[,] Data { get; private set; }
    public int Height => Data.GetLength(0);
    public int Width => Data.GetLength(1);
    public double Max{
        get{
            double ans = double.MinValue;
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    ans = Math.Max(ans, Data[h,w]);
                }
            }
            return ans;
        }
    }

    public double Min{
        get{
            double ans = double.MaxValue;
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    ans = Math.Min(ans, Data[h,w]);
                }
            }
            return ans;
        }
    }

    public byte[,] GetBytesForImg(){
        byte[,] ans = new byte[Height, Width];
        var max = Max;
        var min = Min;
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                var d = (Data[h,w]+min) * 255 / (max - min);
                ans[h,w] = d<0?(byte)0:
                d>255?(byte)255:
                (byte)d;
            }
        }
        return ans;
    }

    public Matrix Abs(){
        double[,] ans = new double[Height,Width];
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                ans[h,w] = Math.Abs(Data[h,w]);
            }
        }
        return new Matrix(ans);
    }

    // 2次元インデクサーの定義
    // this[int rowIndex, int columnIndex] という形式でアクセス可能にする
    public double this[int rowIndex, int columnIndex]
    {
        get
        {
            // インデックスが範囲内かチェック
            ValidateIndex(rowIndex, columnIndex);
            // 内部配列から値を取得して返す
            return Data[rowIndex, columnIndex];
        }
        set
        {
            // インデックスが範囲内かチェック
            ValidateIndex(rowIndex, columnIndex);
            // 内部配列に値を設定する
            Data[rowIndex, columnIndex] = value;
        }
    }

    private bool ValidateIndex(int h, int w)
    {
        return 0 <= h && h < Height && 0 <= w && w < Width;
    }

    public Matrix GetTranspose()
    {
        var data = new double[Width, Height];
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                data[w, h] = Data[h, w];
            }
        }
        return new Matrix(data);
    }

    public byte[,] GetBytes(){
        var ans = new byte[Height,Width];
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                ans[h,w] = (
                    Data[h,w] < 0 ? (byte)0:
                    Data[h,w]>255 ? (byte)255:
                    (byte)Data[h,w]
                );
            }
        }
        return ans;
    }

    public Matrix Round()
    {
        var d = new double[Height, Width];
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                d[h, w] = Math.Round(Data[h, w]);
            }
        }
        return new Matrix(d);
    }

    public double[] ZigZagRead()
    {
        var ans = new List<double>();
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var k = i - j;
                if (k < 0 || 8 <= k) continue;

                var h = j;
                var w = k;

                if (i % 2 == 0)
                {
                    (h, w) = (w, h);
                }

                ans.Add(Data[h,w]);
            }
        }
        return ans.ToArray();
    }

    public override string ToString()
    {
        var ans = "";
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                ans += Data[h, w].ToString("0.00") + " ";
            }
            ans += "\n";
        }

        return ans;
    }


    public static Matrix operator +(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width)
        {
            double[,] d = new double[a.Height, a.Width];

            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    d[h, w] = a[h, w] + b[h, w];
                }
            }
            return new Matrix(d);
        }
        else
        {
            throw new InvalidOperationException("サイズの異なる行列同士の演算はできません");
        }
    }

    public static Matrix operator -(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width)
        {
            double[,] d = new double[a.Height, a.Width];

            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    d[h, w] = a[h, w] - b[h, w];
                }
            }
            return new Matrix(d);
        }
        else
        {
            throw new InvalidOperationException("サイズの異なる行列同士の演算はできません");
        }
    }

    public static Matrix operator /(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width)
        {
            double[,] d = new double[a.Height, a.Width];

            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    d[h, w] = a[h, w] / b[h, w];
                }
            }
            return new Matrix(d);
        }
        else
        {
            throw new InvalidOperationException("サイズの異なる行列同士の演算はできません");
        }
    }

    public static Matrix MultiplyElements(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width)
        {
            double[,] d = new double[a.Height, a.Width];

            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    d[h, w] = a[h, w] * b[h, w];
                }
            }
            return new Matrix(d);
        }
        else
        {
            throw new InvalidOperationException("サイズの異なる行列同士の演算はできません");
        }
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width && a.Height == a.Width)
        {
            double[,] d = new double[a.Height, a.Width];

            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    double temp = 0;
                    for (int k = 0; k < a.Height; k++)
                    {
                        temp += a[h, k] * b[k, w];
                    }
                    d[h, w] = temp;
                }
            }
            return new Matrix(d);
        }
        else
        {
            throw new InvalidOperationException("サイズの異なる行列同士の演算はできません");
        }
    }

    public static Matrix operator +(Matrix a, double b)
    {
        double[,] d = new double[a.Height, a.Width];

        for (int h = 0; h < a.Height; h++)
        {
            for (int w = 0; w < a.Width; w++)
            {
                d[h, w] = a[h, w] + b;
            }
        }
        return new Matrix(d);

    }

    public static Matrix operator -(Matrix a, double b)
    {
        double[,] d = new double[a.Height, a.Width];

        for (int h = 0; h < a.Height; h++)
        {
            for (int w = 0; w < a.Width; w++)
            {
                d[h, w] = a[h, w] - b;
            }
        }
        return new Matrix(d);

    }

    public static Matrix operator *(Matrix a, double b)
    {
        double[,] d = new double[a.Height, a.Width];

        for (int h = 0; h < a.Height; h++)
        {
            for (int w = 0; w < a.Width; w++)
            {
                d[h, w] = a[h, w] * b;
            }
        }
        return new Matrix(d);

    }


    public static Matrix operator /(Matrix a, double b)
    {
        double[,] d = new double[a.Height, a.Width];

        for (int h = 0; h < a.Height; h++)
        {
            for (int w = 0; w < a.Width; w++)
            {
                d[h, w] = a[h, w] / b;
            }
        }
        return new Matrix(d);

    }

    public static bool operator ==(Matrix a, Matrix b)
    {


        if (a.Height == b.Height && a.Width == b.Width)
        {
            var ans = true;
            for (int h = 0; h < a.Height; h++)
            {
                for (int w = 0; w < a.Width; w++)
                {
                    if (a[h, w] != b[h, w])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator !=(Matrix a, Matrix b)
    {
        return !(a == b);
    }


}