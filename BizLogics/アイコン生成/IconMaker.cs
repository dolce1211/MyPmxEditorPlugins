using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using ColorManager;

//アイコン生成周りのロジックは「わわててweb」様より拝借
//https://wawatete.com/archives/116

namespace IconMaker
{
    /// <summary>
    /// BITMAPFILEHEADER構造体(未使用)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct T_BITMAPFILEHEADER
    {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    /// <summary>
    /// BMPINFOHEADER構造体(未使用)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct T_BMPINFOHEADER
    {
        public uint biSize;
        public uint biWidth;
        public uint biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCopmression;
        public uint biSizeImage;
        public uint biXPixPerMeter;
        public uint biYPixPerMeter;
        public uint biClrUsed;
        public uint biCirImportant;
    }

    /// <summary>
    /// ICONDIR構造体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct T_ICONDIR
    {
        public ushort idReserved;
        public ushort idType;
        public ushort idCount;
    }

    /// <summary>
    /// ICONDIRENTRY構造体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct T_ICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public uint dwImageOffset;
    }

    // アイコン画像形式
    public enum EPictureFormat
    {
        BMP = 0,
        PNG
    }

    /// <summary>
    /// 色数の列挙体
    /// </summary>
    public enum EColorDepth
    {
        CD_1BIT = 1,
        CD_4BIT = 4,
        CD_8BIT = 8,
        CD_16BIT = 16,
        CD_24BIT = 24,
        CD_32BIT = 32,
    }

    /// <summary>
    /// アイコン変換情報クラス
    /// </summary>
    public class IconConvertInfo
    {
        /// <summary>
        ///  アイコン画像形式
        /// </summary>
        protected EPictureFormat pictureFormat;

        /// <summary>
        /// 幅
        /// </summary>
        protected ushort width;

        /// <summary>
        /// 高さ
        /// </summary>
        protected ushort height;

        /// <summary>
        /// アイコン色数
        /// </summary>
        protected EColorDepth colorDepth;

        /// <summary>
        /// 補間モード
        /// </summary>
        protected InterpolationMode interpolation;

        /// <summary>
        /// 誤差拡散
        /// </summary>
        protected bool errorDiffusion;

        /// <summary>
        /// 透過しきい値
        /// </summary>
        protected byte transparentThreshold;

        /// <summary>
        /// アイコン画像形式
        /// </summary>
        public EPictureFormat PictureFormat
        {
            get { return this.pictureFormat; }
            set { this.pictureFormat = value; }
        }

        /// <summary>
        /// 幅
        /// </summary>
        public ushort Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public ushort Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        /// <summary>
        /// アイコン色数
        /// </summary>
        public EColorDepth ColorDepth
        {
            get { return this.colorDepth; }
            set { this.colorDepth = value; }
        }

        /// <summary>
        /// 補間モード
        /// </summary>
        public InterpolationMode Interpolation
        {
            get { return this.interpolation; }
            set { this.interpolation = value; }
        }

        /// <summary>
        /// 誤差拡散
        /// </summary>
        public bool ErrorDiffusion
        {
            get { return this.errorDiffusion; }
            set { this.errorDiffusion = value; }
        }

        /// <summary>
        /// 透過しきい値
        /// </summary>
        public byte TransparentThreshold
        {
            get { return this.transparentThreshold; }
            set { this.transparentThreshold = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="aPictureFormat">アイコン画像形式</param>
        /// <param name="aWidth">幅</param>
        /// <param name="aHeight">高さ</param>
        /// <param name="aColorDepth">アイコン色数</param>
        public IconConvertInfo(EPictureFormat aPictureFormat, ushort aWidth, ushort aHeight, EColorDepth aColorDepth)
        {
            this.pictureFormat = aPictureFormat;
            this.width = aWidth;
            this.height = aHeight;
            this.colorDepth = aColorDepth;
            this.interpolation = InterpolationMode.Default;
            this.errorDiffusion = true;
            this.transparentThreshold = 128;
        }
    }

    // アイコン変換クラス
    internal class I2IConverter
    {
        /// <summary>
        /// BITMAPFILEHEADERのサイズ
        /// </summary>
        private const int BITMAPFILEHEADER_SIZE = 14;

        /// <summary>
        /// BMPINFOHEADERのメンバ「biHeight」までのオフセット
        /// </summary>
        private const int BITMAP_HEIGHT_OFFSET = 8;

        /// <summary>
        /// ICONDIRのサイズ
        /// </summary>
        private const int ICONDIR_SIZE = 6;

        /// <summary>
        /// ICONDIRENTRYのサイズ
        /// </summary>
        private const int ICONDIRENTRY_SIZE = 16;

        /// <summary>
        /// アイコン変換情報リスト
        /// </summary>
        protected List<IconConvertInfo> convertInfoList;

        /// <summary>
        /// ベース画像
        /// </summary>
        protected Bitmap baseBitmap;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public I2IConverter()
        {
            this.convertInfoList = new List<IconConvertInfo>();
            this.baseBitmap = null;
        }

        /// <summary>
        /// アイコン変換情報リスト
        /// </summary>
        public List<IconConvertInfo> ConvertInfoList
        {
            get { return this.convertInfoList; }
            set { this.convertInfoList = value; }
        }

        /// <summary>
        /// ベース画像
        /// </summary>
        public Bitmap BaseBitmap
        {
            get { return this.baseBitmap; }
        }

        /// <summary>
        /// ベース画像読み込み(ファイル)
        /// </summary>
        /// <param name="aFileName">入力ファイル名</param>
        /// <returns>正常に入力出来た場合、trueを返す</returns>
        public bool LoadImage(string aFileName)
        {
            FileStream ifs = new FileStream(aFileName, FileMode.Open, FileAccess.Read);
            bool ret = this.LoadImage(ifs);
            ifs.Close();

            return ret;
        }

        /// <summary>
        /// ベース画像読み込み(ストリーム)
        /// </summary>
        /// <param name="aImageStream">入力ストリーム</param>
        /// <returns>正常に入力出来た場合、trueを返す</returns>
        public bool LoadImage(Stream aImageStream)
        {
            // 対応画像形式かどうかチェック
            if (!this.IsImageFormat(aImageStream))
            {
                return false;
            }

            // ストリームからビットマップオブジェクトを作成
            this.baseBitmap = new Bitmap(aImageStream);

            // 32bitビットマップ形式(アルファチャンネルなし)の場合
            if (this.baseBitmap.PixelFormat == PixelFormat.Format32bppRgb)
            {
                // ビットマップデータを読み込み
                Rectangle bitmapRectangle = new Rectangle(0, 0, this.baseBitmap.Width, this.baseBitmap.Height);
                BitmapData bitmapData = this.baseBitmap.LockBits(bitmapRectangle, ImageLockMode.ReadOnly, this.baseBitmap.PixelFormat);
                byte[] rgbValues = new byte[bitmapData.Height * bitmapData.Stride];
                Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bitmapData.Height * bitmapData.Stride);
                this.baseBitmap.UnlockBits(bitmapData);

                // アルファチャンネルありの32bitビットマップ形式に変換
                this.baseBitmap = new Bitmap(bitmapData.Width, bitmapData.Height, PixelFormat.Format32bppArgb);
                bitmapData = this.baseBitmap.LockBits(bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(rgbValues, 0, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride);
                this.baseBitmap.UnlockBits(bitmapData);
            }
            // その他の場合
            else
            {
                // アルファチャンネルありの32bitビットマップとして読み込み
                Bitmap tmpBitmap = (Bitmap)this.baseBitmap.Clone();
                this.baseBitmap = new Bitmap(this.baseBitmap.Width, this.baseBitmap.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(this.baseBitmap);
                g.DrawImage(tmpBitmap, 0, 0, this.baseBitmap.Width, this.baseBitmap.Height);
                tmpBitmap.Dispose();
            }

            return true;
        }

        /// <summary>
        /// アイコン作成(ファイル)
        /// </summary>
        /// <param name="aFileName">出力ファイル名</param>
        /// <returns>正常に出力できた場合、trueを返す</returns>
        public bool SaveIcon(string aFileName)
        {
            FileStream ofs = new FileStream(aFileName, FileMode.Create, FileAccess.Write);
            bool ret = this.SaveIcon(ofs);
            ofs.Close();

            return ret;
        }

        /// <summary>
        /// アイコン作成(ストリーム)
        /// </summary>
        /// <param name="aOutputStream">出力ストリーム</param>
        /// <returns>正常に出力できた場合、trueを返す</returns>
        public bool SaveIcon(Stream aOutputStream)
        {
            T_ICONDIR iconDir;
            T_ICONDIRENTRY iconDirEntry;
            List<T_ICONDIRENTRY> iconDirEntries = new List<T_ICONDIRENTRY>();

            MemoryStream iconImage = null;
            List<MemoryStream> iconImageList = new List<MemoryStream>();

            long writeSize = 0;
            long currentImageOffset = 0x00000000;

            // アイコン変換情報が1つもない場合
            if (this.convertInfoList.Count == 0)
            {
                return false;
            }

            // ベース画像が「null」の場合
            if (this.baseBitmap == null)
            {
                return false;
            }

            // アイコン変換情報数分ループ
            for (int i = 0; i < this.convertInfoList.Count; i++)
            {
                // アイコンイメージ作成
                iconImage = new MemoryStream();

                // 「アイコン画像形式」チェック
                switch (this.convertInfoList[i].PictureFormat)
                {
                    // 「BMP」の場合
                    case EPictureFormat.BMP:
                        // BMPアイコンイメージ作成
                        writeSize = this.CreateBmpIconImage(this.baseBitmap,
                            this.convertInfoList[i], iconImage);
                        break;

                    // 「PNG」の場合
                    case EPictureFormat.PNG:
                        // PNGアイコンイメージ作成
                        writeSize = this.CreatePngIconImage(this.baseBitmap,
                            this.convertInfoList[i], iconImage);
                        break;

                    default:
                        continue;
                }

                // 書き込み失敗時、次のループへ
                if (writeSize == 0)
                {
                    continue;
                }

                iconImageList.Add(iconImage);
            }

            // T_ICONDIR 書き出し
            iconDir = new T_ICONDIR();
            iconDir.idReserved = 0x0000;
            iconDir.idType = 0x0001;
            iconDir.idCount = (ushort)iconImageList.Count;
            writeSize = this.WriteStructData(iconDir, ref aOutputStream);

            // T_ICONDIRENTRY 書き出し
            currentImageOffset = ICONDIR_SIZE + (ICONDIRENTRY_SIZE * iconDir.idCount);
            for (int i = 0; i < iconDir.idCount; i++)
            {
                iconDirEntry = new T_ICONDIRENTRY();
                iconDirEntry.bWidth = (0 == this.convertInfoList[i].Width || 256 < this.convertInfoList[i].Width) ? (byte)0 : (byte)this.convertInfoList[i].Width;
                iconDirEntry.bHeight = (0 == this.convertInfoList[i].Height || 256 < this.convertInfoList[i].Height) ? (byte)0 : (byte)this.convertInfoList[i].Height;
                iconDirEntry.bColorCount = this.GetColorCount(this.convertInfoList[i].ColorDepth);
                iconDirEntry.bReserved = 0x00;
                iconDirEntry.wPlanes = 0x0001;
                iconDirEntry.wBitCount = (ushort)this.convertInfoList[i].ColorDepth;
                iconDirEntry.dwBytesInRes = (uint)iconImageList[i].Length;
                iconDirEntry.dwImageOffset = (uint)currentImageOffset;
                writeSize = this.WriteStructData(iconDirEntry, ref aOutputStream);

                // 次に書き込む「ICONDIRENTRY」の「dwImageOffset」を設定
                currentImageOffset += iconDirEntry.dwBytesInRes;
            }

            // 全てのアイコンイメージ書き出し
            for (int i = 0; i < iconDir.idCount; i++)
            {
                iconImageList[i].WriteTo(aOutputStream);
            }

            return true;
        }

        /// <summary>
        /// アイコンヘッダ記載色数取得
        /// </summary>
        /// <returns></returns>
        protected byte GetColorCount(EColorDepth colorDepth)
        {
            switch (colorDepth)
            {
                case EColorDepth.CD_32BIT:
                case EColorDepth.CD_24BIT:
                case EColorDepth.CD_16BIT:
                case EColorDepth.CD_8BIT:
                    return 0;

                case EColorDepth.CD_4BIT:
                    return 16;

                case EColorDepth.CD_1BIT:
                    return 2;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 変換後のアイコンプレビュー取得
        /// </summary>
        /// <param name="aConvertInfo">アイコン変換情報</param>
        /// <returns>プレビュー画像のBitmapを返す</returns>
        public Bitmap GetIconPreview(IconConvertInfo aConvertInfo)
        {
            Bitmap iconBitmap;

            // ベース画像が「null」の場合
            if (this.baseBitmap == null)
            {
                return null;
            }

            // アイコンイメージ作成共通処理
            if (!this.CreateIconImageCommon(this.baseBitmap, aConvertInfo, out iconBitmap))
            {
                return null;
            }

            return iconBitmap;
        }

        /// <summary>
        /// BMPアイコンイメージ作成
        /// </summary>
        /// <param name="aSrcBitmap">入力画像Bitmap</param>
        /// <param name="aConvertInfo">アイコン変換情報</param>
        /// <param name="aIconStream">出力アイコンStream</param>
        /// <returns></returns>
        protected long CreateBmpIconImage(Bitmap aSrcBitmap, IconConvertInfo aConvertInfo, Stream aIconStream)
        {
            // アイコン画像用Bitmap
            Bitmap iconBitmap;
            // アルファマスク用Bitmap
            Bitmap maskBitmap;
            // 一時ストリーム
            MemoryStream tmpStream;

            // アイコンイメージ作成共通処理
            if (!this.CreateIconImageCommon(aSrcBitmap, aConvertInfo, out iconBitmap, out maskBitmap))
            {
                return 0;
            }

            // 作成したアイコンイメージデータを一時ストリームへ書き込み
            tmpStream = new MemoryStream();
            iconBitmap.Save(tmpStream, ImageFormat.Bmp);

            // 画像書き出し(デバッグ用)
            //string fileName;
            //fileName = string.Format("image_{0}_{1}_{2}.bmp", aConvertInfo.Width, aConvertInfo.Height, aConvertInfo.ColorDepth );
            //iconBitmap.Save(fileName, ImageFormat.Bmp);

            //fileName = string.Format("mask_{0}_{1}_{2}.bmp", aConvertInfo.Width, aConvertInfo.Height, aConvertInfo.ColorDepth);
            //maskBitmap.Save(fileName, ImageFormat.Bmp);

            // マスクビットマップデータを読み込み
            Rectangle bitmapRectangle = new Rectangle(0, 0, maskBitmap.Width, maskBitmap.Height);
            BitmapData bitmapData = maskBitmap.LockBits(bitmapRectangle, ImageLockMode.ReadOnly, maskBitmap.PixelFormat);
            byte[] maskBuf = new byte[bitmapData.Height * bitmapData.Stride];
            Marshal.Copy(bitmapData.Scan0, maskBuf, 0, bitmapData.Height * bitmapData.Stride);
            maskBitmap.UnlockBits(bitmapData);

            // マスクデータの書き込み
            tmpStream.Write(maskBuf, 0, maskBuf.Length);

            // Bitmap破棄
            iconBitmap.Dispose();
            maskBitmap.Dispose();

            // 「BMPINFOHEADER」の「biHeight」箇所へ移動
            tmpStream.Seek(BITMAPFILEHEADER_SIZE + BITMAP_HEIGHT_OFFSET, SeekOrigin.Begin);
            // 「biHeight」を元の2倍の値に変更
            tmpStream.Write(BitConverter.GetBytes(aConvertInfo.Height * 2), 0, sizeof(int));

            // 作成したアイコンイメージデータをストリームへ書き込み
            aIconStream.Write(tmpStream.ToArray(), BITMAPFILEHEADER_SIZE,
                (int)(tmpStream.Length - BITMAPFILEHEADER_SIZE));

            // 書き込んだストリーム長を返す
            return aIconStream.Length;
        }

        /// <summary>
        /// PNGアイコンイメージ作成
        /// </summary>
        /// <param name="aSrcBitmap">入力画像Bitmap</param>
        /// <param name="aConvertInfo">アイコン変換情報</param>
        /// <param name="aIconStream">出力アイコンStream</param>
        /// <returns></returns>
        protected long CreatePngIconImage(Bitmap aSrcBitmap, IconConvertInfo aConvertInfo, Stream aIconStream)
        {
            Bitmap iconBitmap;

            // アイコンイメージ作成共通処理
            if (!this.CreateIconImageCommon(aSrcBitmap, aConvertInfo, out iconBitmap))
            {
                return 0;
            }

            // 作成したアイコンイメージデータをストリームへ書き込み
            iconBitmap.Save(aIconStream, ImageFormat.Png);
            // Bitmap破棄
            iconBitmap.Dispose();

            // 書き込んだストリーム長を返す
            return aIconStream.Length;
        }

        /// <summary>
        /// アイコンイメージ作成共通処理
        /// </summary>
        /// <param name="aSrcBitmap">入力画像Bitmap</param>
        /// <param name="aConvertInfo">アイコン変換情報</param>
        /// <param name="aIconBitmap">出力アイコンBitmap</param>
        /// <returns></returns>
        protected bool CreateIconImageCommon(Bitmap aSrcBitmap, IconConvertInfo aConvertInfo, out Bitmap aIconBitmap)
        {
            bool result = false;
            Bitmap maskBitmap;
            result = CreateIconImageCommon(aSrcBitmap, aConvertInfo, out aIconBitmap, out maskBitmap);

            return result;
        }

        /// <summary>
        /// アイコンイメージ作成共通処理
        /// </summary>
        /// <param name="aSrcBitmap">入力画像Bitmap</param>
        /// <param name="aConvertInfo">アイコン変換情報</param>
        /// <param name="aIconBitmap">出力アイコンBitmap</param>
        /// <param name="aMaskBitmap">出力マスクBitmap</param>
        /// <returns></returns>
        protected bool CreateIconImageCommon(Bitmap aSrcBitmap, IconConvertInfo aConvertInfo, out Bitmap aIconBitmap, out Bitmap aMaskBitmap)
        {
            bool result = false;
            aIconBitmap = null;
            aMaskBitmap = null;
            int width = aConvertInfo.Width;
            int height = aConvertInfo.Height;

            // 幅が「0」指定又は「256」より大きい場合、「256」に設定
            if (0 == aConvertInfo.Width || 256 < aConvertInfo.Width)
            {
                width = 256;
            }
            // 高さが「0」指定又は「256」より大きい場合、「256」に設定
            if (0 == aConvertInfo.Height || 256 < aConvertInfo.Height)
            {
                height = 256;
            }

            // Bitmapオブジェクト作成(変換後のサイズで作成)
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            try
            {
                // アイコンイメージデータ作成
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.InterpolationMode = aConvertInfo.Interpolation;
                graphics.DrawImage(aSrcBitmap, 0, 0, width, height);
            }
            catch
            {
                return false;
            }

            // 減色
            ColorQuantizer colorQuantizer = new ColorQuantizer();
            colorQuantizer.Depth = (EDepth)aConvertInfo.ColorDepth;
            colorQuantizer.PalleteType = EPalleteType.SYS_WIN;
            colorQuantizer.ErrorDiffusion = aConvertInfo.ErrorDiffusion;
            colorQuantizer.TransparentThreshold = aConvertInfo.TransparentThreshold;
            result = colorQuantizer.ReduceColor(bitmap, out aIconBitmap);
            if (!result)
            {
                return false;
            }

            result = colorQuantizer.GetAlphaMask(bitmap, out aMaskBitmap);
            if (!result)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 対応画像形式かどうかチェック
        /// </summary>
        /// <param name="aImageStream">対象の画像ストリーム</param>
        /// <returns>対応画像の場合、trueを返す</returns>
        protected bool IsImageFormat(Stream aImageStream)
        {
            // 対応画像のリスト
            ImageFormat[] imageFormats = new ImageFormat[]
            {
                ImageFormat.Bmp,
                ImageFormat.Gif,
                ImageFormat.Jpeg,
                ImageFormat.Png,
                ImageFormat.Tiff
            };

            try
            {
                // ファイル形式取得
                Image pictureImage = Image.FromStream(aImageStream);
                ImageFormat pictureImageFormat = pictureImage.RawFormat;
                pictureImage.Dispose();

                // ファイル形式のGUIDが一致するかチェック
                for (int i = 0; i < imageFormats.Length; i++)
                {
                    if (imageFormats[i].Guid == pictureImageFormat.Guid)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// 構造体データ書き込み
        /// </summary>
        /// <param name="aStructData">構造体データ</param>
        /// <param name="aDstStream">出力先ストリーム</param>
        /// <returns>書き込みサイズを返す</returns>
        private int WriteStructData(object aStructData, ref Stream aDstStream)
        {
            int size = Marshal.SizeOf(aStructData);
            IntPtr buffer = Marshal.AllocCoTaskMem(size);
            Marshal.StructureToPtr(aStructData, buffer, true);
            byte[] buf = new byte[size];
            Marshal.Copy(buffer, buf, 0, size);
            aDstStream.Write(buf, 0, size);
            return size;
        }
    }
}