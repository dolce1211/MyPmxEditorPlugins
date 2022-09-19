using FolderIconCreator.BizLogics;
using IconMaker;
using PEPlugin;
using PEPlugin.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderIconCreator.UI
{
    public partial class frmSetting : Form
    {
        private IPERunArgs _args = null;

        /// <summary>
        /// pmxファイルが存在しているフォルダ
        /// </summary>
        private string _pmxPath = "";

        public frmSetting(IPERunArgs args)
        {
            InitializeComponent();

            this._args = args;
            this._pmxPath = this._args.Host.Connector.Pmx.CurrentPath;
            this.textBox1.Text = System.IO.Path.GetDirectoryName(this._pmxPath);

            //transformViewを出す
            if (!this._args.Host.Connector.View.TransformView.Visible)
            {
                this._args.Host.Connector.View.TransformView.Visible = true;
            }
        }

        private void frmSetting_Shown(object sender, EventArgs e)
        {
            //アイコンを常時更新するためのタイマー
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(new System.Threading.TimerCallback((Action<object>)(x =>
            {
                try
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        var pmxpath = this._args.Host.Connector.Pmx.CurrentPath;
                        if (pmxpath != this._pmxPath)
                        {
                            //モデルが切り替わったぽい
                            this._pmxPath = pmxpath;
                            this.textBox1.Text = System.IO.Path.GetDirectoryName(this._pmxPath);
                        }

                        Bitmap bmp = null;
                        var tv = _args.Host.Connector.View.TransformView;
                        if (tv.Visible)
                        {
                            bmp = tv.GetClientImage();
                        }
                        if (bmp == null)
                        {
                            this.pictureBox1.Image = null;
                            return;
                        }
                        var resizedImage = ResizeImage(bmp, 256);
                        this.pictureBox1.Image = resizedImage;
                        this.pictureBox1.Refresh();
                        if (this.IsDisposed)
                        {
                            //timer.Change(int.MaxValue, int.MaxValue);
                            timer.Dispose();
                            return;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                }
            }
            )), null, 500, 50);

            this.FormClosed += (ss, ee) =>
            {
                timer.Change(int.MaxValue, int.MaxValue);
                timer.Dispose();
            };
        }

        private void btnDir_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            try
            {
                // ダイアログのタイトル
                openFileDialog.Title = "フォルダを選択してください。";
                // デフォルトのフォルダ
                if (System.IO.Directory.Exists(this.textBox1.Text))
                    openFileDialog.InitialDirectory = this.textBox1.Text;
                else
                    openFileDialog.InitialDirectory = Environment.CurrentDirectory;

                // ダイアログボックスに表示する文字列
                openFileDialog.FileName = "選択してください";

                // フォルダのみを表示
                openFileDialog.Filter = "Folder|.";

                // 存在しないファイル指定時の警告
                openFileDialog.CheckFileExists = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.textBox1.Text = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                }
                // オブジェクトを破棄する
            }
            finally
            {
                openFileDialog.Dispose();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_args.Host.Connector.View.TransformView.Visible)
            {
                MessageBox.Show("TransformViewを表示させてください");
                return;
            }
            if (!Directory.Exists(this.textBox1.Text))
            {
                MessageBox.Show("有効なフォルダを指定してください");
                this.textBox1.Focus();
                return;
            }
            var msg = $"{this.textBox1.Text} \r\n 以上のフォルダの";
            if (sender == this.button1)
            {
                //確定
                var iconName = this.TrySaveIcon();
                if (!string.IsNullOrEmpty(iconName))
                {
                    //アイコン保存成功したらDesktop.iniを書き換える
                    var ret = DesktopIniCreator.Create(this.textBox1.Text, iconName);
                    if (ret)
                        msg += "フォルダアイコンを設定しました";
                    else
                        msg += "フォルダアイコンの設定に失敗しました";
                }
            }
            else
            {
                //戻す
                DesktopIniCreator.Create(this.textBox1.Text, String.Empty);
                msg += "フォルダアイコンを既定値に戻しました";
            }
            MessageBox.Show(msg);
        }

        /// <summary>
        /// アイコン保存
        /// </summary>
        /// <returns></returns>
        private string TrySaveIcon()
        {
            //既存のicoファイルを削除する
            foreach (var file in new System.IO.DirectoryInfo(this.textBox1.Text).GetFiles("*.ico"))
            {
                if (Regex.IsMatch(file.Name.ToLower(), @"foldericon\d{14}"))
                    System.IO.File.Delete(file.FullName);
            }

            var ms = new MemoryStream();
            this.pictureBox1.Image.Save(ms, ImageFormat.Png);
            var i2ic = new I2IConverter();
            if (!i2ic.LoadImage(ms))
            {
                MessageBox.Show("画像の読み込みに失敗しました");
                return "";
            }
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.PNG, 256, 256, EColorDepth.CD_32BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 128, 128, EColorDepth.CD_32BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 48, 48, EColorDepth.CD_32BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 32, 32, EColorDepth.CD_32BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 16, 16, EColorDepth.CD_32BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 48, 48, EColorDepth.CD_8BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 32, 32, EColorDepth.CD_8BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 16, 16, EColorDepth.CD_8BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 32, 32, EColorDepth.CD_4BIT));
            i2ic.ConvertInfoList.Add(new IconConvertInfo(EPictureFormat.BMP, 16, 16, EColorDepth.CD_4BIT));

            //同名のアイコンで上書き保存するとなんかフォルダ側でキャッシュが残ってるかなにかしてうまく更新されないため、
            //アイコン名には日時情報を含めて保存する
            var iconName = $"foldericon{DateTime.Now.ToString("yyyyMMddHHmmss")}.ico";
            var outputFileName = System.IO.Path.Combine(this.textBox1.Text, iconName);
            if (!i2ic.SaveIcon(outputFileName))
            {
                MessageBox.Show("アイコンの保存に失敗しました");
                return "";
            }
            return iconName;
        }

        private Image ResizeImage(Bitmap image, int length)
        {
            //画像を正方形に整形する
            Rectangle rect = new Rectangle(20, 90, 450, 100);
            if (image.Width > image.Height)
            {
                //横長
                var x = (image.Width - image.Height) / 2;
                rect = new Rectangle(x, 0, image.Height, image.Height);
            }
            else
            {
                //縦長
                var y = (image.Height - image.Width) / 2;
                rect = new Rectangle(0, y, image.Width, image.Width);
            }
            Bitmap clipedImage = image.Clone(rect, image.PixelFormat);

            //一辺256pxにリサイズする
            var destinationRect = new Rectangle(0, 0, length, length);
            var destinationImage = new Bitmap(length, length);
            destinationImage.SetResolution(clipedImage.HorizontalResolution, clipedImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destinationImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(clipedImage, destinationRect, 0, 0, clipedImage.Width, clipedImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            image.Dispose();
            clipedImage.Dispose();
            return (Image)destinationImage;
        }
    }
}