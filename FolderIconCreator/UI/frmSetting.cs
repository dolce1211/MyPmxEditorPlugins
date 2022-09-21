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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyUtility;

namespace FolderIconCreator.UI
{
    public partial class frmSetting : Form
    {
        private IPERunArgs _args = null;

        //背景
        private Bitmap _backBmp = null;

        //前景
        private Bitmap _foreBmp = null;

        /// <summary>
        /// クロマキー抜き対象の色
        /// </summary>
        private Color _chromakeyColor = Color.White;

        /// <summary>
        /// 設定
        /// </summary>
        private FolderIconCreatorSetting _mysetting = new FolderIconCreatorSetting();

        /// <summary>
        /// pmxファイルが存在しているフォルダ
        /// </summary>
        private string _pmxPath = "";

        public frmSetting(IPERunArgs args)
        {
            InitializeComponent();

            this._args = args;
            this._pmxPath = this._args.Host.Connector.Pmx.CurrentPath;
            if (System.IO.File.Exists(this._pmxPath))
                this.textBox1.Text = System.IO.Path.GetDirectoryName(this._pmxPath);

            //transformViewを出す
            if (!this._args.Host.Connector.View.TransformView.Visible)
            {
                this._args.Host.Connector.View.TransformView.Visible = true;
            }
        }

        private void btnBackground_Click(object sender, EventArgs e)
        {
            var filter = "Image File(*.bmp,*.png)|*.bmp;*.png;|Bitmap(*.bmp)|*.bmp|PNG(*.png)|*.png";
            var ret = "";
            if (sender == this.btnForeground || sender == this.btnBackground)
            {
                ret = TryOpenFile("画像ファイルを選択してください(正方形の画像推奨)", this._mysetting.BackgroundPicturePath, filter);
                if (!System.IO.File.Exists(ret))
                    return;
            }

            var mode = 0;
            if (sender == this.btnForeground || sender == this.btnForeDel)
            {
                mode = 1;
                this.chkDefForeImage.Checked = false;
            }

            this.TrySetImage(mode, ret);
        }

        private void btnDir_Click(object sender, EventArgs e)
        {
            var initialDirectory = this.textBox1.Text;
            if (!System.IO.Directory.Exists(this.textBox1.Text))
                initialDirectory = Environment.CurrentDirectory;
            var ret = TryOpenFile("フォルダを選択してください。", initialDirectory, "Folder|.", "選択してください");
            if (!string.IsNullOrEmpty(ret))
                this.textBox1.Text = ret;
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

        private void chkTransparent_CheckedChanged(object sender, EventArgs e)
        {
            this.TrySetImage(1, this.lblForePath.Text);
        }

        private void frmSetting_FormClosed(object sender, FormClosedEventArgs e)
        {
            var dllpath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var mysettingpath = System.IO.Path.Combine(dllpath, "FolderIconCreatorSetting.xml");

            var setting = new FolderIconCreatorSetting()
            {
                DefaultForegroundPicture = this.chkDefForeImage.Checked,
                BackgroundPicturePath = this.lblBackPath.Text,
                ForegroundPicturePath = this.lblForePath.Text
            };
            if (setting.DefaultForegroundPicture)
                setting.ForegroundPicturePath = "";
            Serializer.Serialize(setting, mysettingpath);
        }

        private void frmSetting_Shown(object sender, EventArgs e)
        {
            this.Initialize();

            //アイコンを常時更新するためのタイマーを動かす
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(new System.Threading.TimerCallback((Action<object>)(x =>
            {
                try
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        //アイコンプレビューを更新する
                        var pmxpath = this._args.Host.Connector.Pmx.CurrentPath;
                        if (pmxpath != this._pmxPath)
                        {
                            //モデルが切り替わったぽい
                            this._pmxPath = pmxpath;

                            this.textBox1.Text = "";
                            if (System.IO.File.Exists(this._pmxPath))
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
                            //TransformViewから画像が取れなかった？
                            this.pictureBox1.Image = null;
                            return;
                        }

                        using (Bitmap resizedImage = ResizeImage(bmp, 256, (this._backBmp != null)))
                        {
                            var resultbmp = resizedImage;
                            if (_backBmp != null)
                            {
                                resultbmp = (Bitmap)_backBmp.Clone();
                                using (Graphics g = Graphics.FromImage(resultbmp))
                                {
                                    g.DrawImage(resizedImage, 0, 0, resizedImage.Width, resizedImage.Height);
                                }
                            }
                            else
                            {
                                resultbmp = (Bitmap)resultbmp.Clone();
                            }
                            if (_foreBmp != null)
                            {
                                using (Graphics g = Graphics.FromImage(resultbmp))
                                {
                                    g.DrawImage(_foreBmp, 0, 0, _foreBmp.Width, _foreBmp.Height);
                                }
                            }
                            this.pictureBox1.Image = resultbmp;
                            this.pictureBox1.Refresh();
                        }

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

        /// <summary>
        /// 初期化を行います。
        /// </summary>
        private void Initialize()
        {
            //自分の設定を読み込む
            var dllpath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var mysettingpath = System.IO.Path.Combine(dllpath, "FolderIconCreatorSetting.xml");
            if (System.IO.File.Exists(mysettingpath))
            {
                try
                {
                    this._mysetting = Serializer.Deserialize<FolderIconCreatorSetting>(mysettingpath);
                }
                catch (Exception)
                {
                }
            }
            if (this._mysetting == null)
                this._mysetting = new FolderIconCreatorSetting();

            this.chkDefForeImage.Checked = this._mysetting.DefaultForegroundPicture;
            this.TrySetImage(0, this._mysetting.BackgroundPicturePath);
            this.TrySetImage(1, this._mysetting.ForegroundPicturePath);

            //TransformViewの設定ファイルから透過対象の色を取得する
            var mypath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var transviewpath = System.IO.Path.Combine(mypath, @"_data\表示設定\_TransView.vdt");
            if (System.IO.File.Exists(transviewpath))
            {
                try
                {
                    var viewsetting = Serializer.Deserialize<ViewSettingData>(transviewpath);
                    if (viewsetting != null)
                        this._chromakeyColor = Color.FromArgb(viewsetting.BackColor);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 画像のりサイズを行います。
        /// </summary>
        /// <param name="image">画像</param>
        /// <param name="length">一辺の長さ(px)</param>
        /// <param name="chromakey">クロマキー抜きするならtrue</param>
        /// <returns></returns>
        private Bitmap ResizeImage(Bitmap image, int length, bool chromakey)
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

            if (chromakey)
                //背景色を透過する
                destinationImage.MakeTransparent(this._chromakeyColor);

            image.Dispose();
            clipedImage.Dispose();
            return destinationImage;
        }

        private string TryOpenFile(string title, string initialDirectory, string filter, string filename = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                // ダイアログのタイトル
                openFileDialog.Title = title;

                openFileDialog.InitialDirectory = initialDirectory;

                // ダイアログボックスに表示する文字列
                openFileDialog.FileName = filename;

                // フォルダのみを表示
                openFileDialog.Filter = filter;

                // 存在しないファイル指定時の警告
                openFileDialog.CheckFileExists = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (filter.ToLower().IndexOf("folder") >= 0)
                        return System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                    else
                        return openFileDialog.FileName;
                }

                return string.Empty;
            }
            finally
            {
                openFileDialog.Dispose();
            }
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

        /// <summary>
        /// 背景・前景画像を設定します。
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool TrySetImage(int mode, string path)
        {
            try
            {
                Label lbl = null;
                bool chromakey = false;
                if (mode == 0)
                {
                    //背景
                    lbl = this.lblBackPath;
                    this._backBmp = null;
                }
                else
                {
                    //前景
                    lbl = this.lblForePath;
                    this._foreBmp = null;
                    chromakey = true;
                }
                if (!System.IO.File.Exists(path))
                {
                    lbl.Text = "なし";
                    return false;
                }

                var bmp = new Bitmap(path);
                var resizedbmp = this.ResizeImage(bmp, 256, chromakey);
                if (mode == 0)
                    //背景
                    this._backBmp = resizedbmp;
                else
                {
                    //前景
                    this._foreBmp = resizedbmp;
                }
                lbl.Text = path;
                return true;
            }
            finally
            {
                if (this.chkDefForeImage.Checked)
                    this._foreBmp = Properties.Resources.defaultFore;
            }
        }
    }
}