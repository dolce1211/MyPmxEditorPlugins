using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Media3D; //"PresentationCore"を参照に追加
using System.Runtime.InteropServices;
namespace MyUtility

{   public static class MathUtil
    {
        public static float lerp(float x, float y, float s)
        {
            return x + s * (y - x);
        }
    }

  
    /// <summary>
    /// テキストボックスのヘルパです。
    /// </summary>
    public static class TextBoxHelper
    {
        /// <summary>
        /// テキストボックスの入力を数値に限定
        /// </summary>
        /// <param name="txtbox"></param>
        /// <returns></returns>
        public static bool LimitInputToNum(this TextBox txtbox, bool allowDecimal=true, bool allowMinus=true)
        {
            if (allowDecimal && allowMinus)
            {
                //正負の小数
                txtbox.KeyPress += new KeyPressEventHandler(KeyPressEventAll);
            }
            else if (allowDecimal && !allowMinus)
            {
                //正の小数
                txtbox.KeyPress += new KeyPressEventHandler(KeyPressEventPlusFloat);
            }
            else if (!allowDecimal && allowMinus)
            {
                //正負の整数
                txtbox.KeyPress += new KeyPressEventHandler(KeyPressEventAllInt);
            }
            else
            {
                //正の整数
                txtbox.KeyPress += new KeyPressEventHandler(KeyPressEventPlusInt);
            }

            return true;
        }
        /// <summary>
        /// 正負の小数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KeyPressEventAll(object sender, KeyPressEventArgs e)
        {
            KeyPressEventInternal(sender, e, true, true);
        }

        /// <summary>
        /// //正負の整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KeyPressEventAllInt(object sender, KeyPressEventArgs e)
        {
            KeyPressEventInternal(sender, e, false, true);
        }

        /// <summary>
        /// 正の整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KeyPressEventPlusInt(object sender, KeyPressEventArgs e)
        {
            KeyPressEventInternal(sender, e, false, false);
        }
        /// <summary>
        /// 正の小数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KeyPressEventPlusFloat(object sender, KeyPressEventArgs e)
        {
            KeyPressEventInternal(sender, e, true, false);
        }

        private static void KeyPressEventInternal(object sender, KeyPressEventArgs e, bool allowDecimal, bool allowMinus)
        {

            TextBox txtbox = (TextBox)sender;

            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                if (e.KeyChar == '.')
                {
                    if (!allowDecimal) { e.Handled = true; }
                    if (txtbox.Text.IndexOf(".") > 0)
                    {
                        e.Handled = true;
                    }
                }
                else if (e.KeyChar == '-')
                {
                    if (!allowMinus) { e.Handled = true; }
                    if (txtbox.SelectionStart != 0 || txtbox.Text.IndexOf("-") > 0)
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    //押されたキーが 0～9でない場合は、イベントをキャンセルする
                    e.Handled = true;


                }
            }
        }
    }

    /// <summary>
    /// ファイルマネージャです。
    /// </summary>
    public class MyFileManager
    {
        /// <summary>
        /// ファイルオープン
        /// </summary>
        /// <returns></returns>
        public static string ShowOpenFileDialog( string argFilter= "",string argTitle = "", string argInitialDirectory = "")
        {
            //MessageBox.Show(argInitialDirectory);
            string initialDirectory = Environment.CurrentDirectory;
            //MessageBox.Show(initialDirectory);
            if (! string.IsNullOrEmpty (argInitialDirectory) && (System.IO.Directory.Exists(argInitialDirectory)|| System.IO.File.Exists(argInitialDirectory)))
                initialDirectory = System.IO.Path.GetDirectoryName(argInitialDirectory).Trim();

            
            string filter = "pmd/pmx Files(*.pmx, *.pmd) | *.pmx; *.pmd";
            if (argFilter != string.Empty) { filter = argFilter; }

            string title = "ファイル指定";
            if (argTitle != string.Empty) { title = argTitle; }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.Filter = filter;// "pmx files (*.pmx)|*.pmx"; 
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = title;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }
            return openFileDialog.FileName;
        }

        /// <summary>
        /// ファイルオープン
        /// </summary>
        /// <returns></returns>
        public static string ShowSaveFileDialog(string argFileName = "", string argFilter = "", string argTitle = "")
        {
            //SaveFileDialogクラスのインスタンスを作成
            SaveFileDialog sfd = new SaveFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            sfd.FileName = "text.txt";
            if (! string.IsNullOrEmpty ( argFileName)) { sfd.FileName  = argFileName; }

            //はじめに表示されるフォルダを指定する
            sfd.InitialDirectory = Environment.CurrentDirectory;
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            sfd.Filter = "txtファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
            if(!string.IsNullOrEmpty(argFilter)) { sfd.Filter = argFilter; }
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            sfd.FilterIndex = 1;
            //タイトルを設定する
            sfd.Title = "ファイル保存";
            if (!string.IsNullOrEmpty(argTitle)) { sfd.Title  = argTitle; }
            
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            //既に存在するファイル名を指定したとき警告する
            //デフォルトでTrueなので指定する必要はない
            sfd.OverwritePrompt = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            sfd.CheckPathExists = true;

            //ダイアログを表示する
            try
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                    return  sfd.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return string.Empty ;
        }



        /// <summary>
        /// このアセンブリの現在のパスを返します。
        /// </summary>
        /// <returns></returns>
        public static string  GetCurrentAssemblyPath()
        {
            string result = string.Empty;
            result = System.Reflection.Assembly.GetExecutingAssembly().Location;
            result = System.IO.Path.GetDirectoryName(result);
            return result;
        }
    }




    /// <summary>
    /// XMLシリアライザです。
    /// </summary>
    public class Serializer
    {
        public static bool Serialize<T>(T entity, string fileName)
        {
 
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
            
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(T));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, entity);
            //ファイルを閉じる
            sw.Close();

            return true;
        }

        public static T Deserialize<T>(string fileName)
        {

            T result = default(T);

            if (!System.IO.File.Exists(fileName))
            {
                return result;
            }

            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(T));

            try
            {
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(
                    fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                result = (T)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }





    /// <summary>
    /// 変換用ヘルパです。
    /// </summary>
    public static class ParseHelper
    {
        
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern Int32 SendMessage(IntPtr hWnd,  int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x000B;

        /// <summary>
        /// コントロールの再描画を停止させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void BeginUpdate(this Control control)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// コントロールの再描画を再開させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void EndUpdate(this Control control, bool noRefresh = false)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            //control.Invalidate();
            if (!noRefresh)
                control.Refresh();
        }

        /// <summary>
        /// コントロールの再描画を停止させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void BeginUpdate(this IntPtr hWnd)
        {
            SendMessage(hWnd,
                WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// コントロールの再描画を再開させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void EndUpdate(this IntPtr hWnd)
        {
            SendMessage(hWnd,
                WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
        }



    }
    /// <summary>
    /// コントロール用ヘルパです。
    /// </summary>
    public static class ControlHelper
    { 
        /// <summary>
        /// byte配列を文字列に変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static string ToStringFromByte(this byte[] bs)
        {
            string result =  System.Text.Encoding.GetEncoding("Shift_JIS").GetString(bs).Replace("\0", "");
            //謎の文字が挟まる事があるので補正 2019/11/23;
            char c = (char)63729;
            string rep = c.ToString () ;
            if (result.IndexOf(rep) >= 0)
                result = result.Replace(rep, "");

            return result;
        }

        /// <summary>
        /// byte配列をintに変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static int ToIntFromByte(this byte[] bs)
        {
            return BitConverter.ToInt32(bs, 0); ;
        }

        /// <summary>
        /// byte配列をintに変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static float ToFloatFromByte(this byte[] bs)
        {
            return BitConverter.ToSingle(bs, 0); ;
        }


        /// <summary>
        /// intをbyte配列に変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static byte[] ToByte(this int val, int length)
        {
            byte[] data = BitConverter.GetBytes(val);
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i < data.Length)
                {
                    result[i] = data[i];
                }
                else
                {
                    result[i] = 0;
                }

            }

            return result;
        }

        /// <summary>
        /// floatをbyte配列に変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static byte[] ToByte(this float val, int length)
        {
            byte[] data = BitConverter.GetBytes(val);
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i < data.Length)
                {
                    result[i] = data[i];
                }
                else
                {
                    result[i] = 0;
                }
            }
            return result;
        }


        /// <summary>
        /// 文字列をbyte配列に変換して返します。
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static byte[] ToByte(this string str, int length)
        {
            byte[] data = Encoding.GetEncoding("shift_jis").GetBytes(str);

            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i < data.Length)
                {
                    result[i] = data[i];
                }
                else
                {
                    result[i] = 0;
                }
            }
            return result;
        }

        public static float ToFloat(this string value)
        {
            float result = 0f;
            try
            {
                result = float.Parse(value);
            }
            catch (Exception)
            {
                result = 0f;
            }
            return result;
        }
        public static int ToInt(this string value)
        {
            int result = 0;
            try
            {
                result = int.Parse(value);
            }
            catch (Exception)
            {
                result = 0;
            }
            return result;
        }


        /// <summary>
        /// オイラーからクォータニオンに変換します。
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Quaternion ToQuatanion(this Vector3D v)
        {
            Double Rcosx = Math.Cos(v.X / 180.0 * Math.PI / 2.0);
            Double Rcosy = Math.Cos(v.Y / 180.0 * Math.PI / 2.0);
            Double Rcosz = Math.Cos(v.Z / 180.0 * Math.PI / 2.0);

            Double Rsinx = Math.Sin(v.X / 180.0 * Math.PI / 2.0);
            Double Rsiny = Math.Sin(v.Y / 180.0 * Math.PI / 2.0);
            Double Rsinz = Math.Sin(v.Z / 180.0 * Math.PI / 2.0);

            Double w = (Rcosy * Rcosx * Rcosz + Rsiny * Rsinx * Rsinz);
            Double y = (Rsiny * Rcosx * Rcosz - Rcosy * Rsinx * Rsinz);
            Double x = (Rcosy * Rsinx * Rcosz + Rsiny * Rcosx * Rsinz);
            Double z = (Rcosy * Rcosx * Rsinz - Rsiny * Rsinx * Rcosz);

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// クォータニオンからオイラーに変換します。
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3D ToEular(this Quaternion q)
        {
            Double ry = (Math.Atan2((2.0 * (q.W * q.Y + q.X * q.Z)), (1.0 - 2.0 * (q.X * q.X + q.Y * q.Y))) / Math.PI * 180.0);
            Double rx = (Math.Asin((2.0 * (q.W * q.X - q.Z * q.Y))) / Math.PI * 180.0);
            Double rz = (Math.Atan2((2.0 * (q.W * q.Z + q.X * q.Y)), (1.0 - 2.0 * (q.X * q.X + q.Z * q.Z))) / Math.PI * 180.0);
            return new Vector3D(rx, ry, rz);
        }




        ///// <summary>
        ///// オイラーからクォータニオンに変換します。
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns></returns>
        //public static DxMath.Quaternion ToQuatanion(this DxMath.Vector3  v)
        //{
        //    Double Rcosx = Math.Cos(v.X / 180.0 * Math.PI / 2.0);
        //    Double Rcosy = Math.Cos(v.Y / 180.0 * Math.PI / 2.0);
        //    Double Rcosz = Math.Cos(v.Z / 180.0 * Math.PI / 2.0);

        //    Double Rsinx = Math.Sin(v.X / 180.0 * Math.PI / 2.0);
        //    Double Rsiny = Math.Sin(v.Y / 180.0 * Math.PI / 2.0);
        //    Double Rsinz = Math.Sin(v.Z / 180.0 * Math.PI / 2.0);

        //    Double w = (Rcosy * Rcosx * Rcosz + Rsiny * Rsinx * Rsinz);
        //    Double y = (Rsiny * Rcosx * Rcosz - Rcosy * Rsinx * Rsinz);
        //    Double x = (Rcosy * Rsinx * Rcosz + Rsiny * Rcosx * Rsinz);
        //    Double z = (Rcosy * Rcosx * Rsinz - Rsiny * Rsinx * Rcosz);

        //    return new DxMath.Quaternion((float)x, (float)y, (float)z, (float)w);
        //}

        ///// <summary>
        ///// クォータニオンからオイラーに変換します。
        ///// </summary>
        ///// <param name="q"></param>
        ///// <returns></returns>
        //public static DxMath.Vector3 ToEular(this DxMath.Quaternion q)
        //{
        //    Double ry = (Math.Atan2((2.0 * (q.W * q.Y + q.X * q.Z)), (1.0 - 2.0 * (q.X * q.X + q.Y * q.Y))) / Math.PI * 180.0);
        //    Double rx = (Math.Asin((2.0 * (q.W * q.X - q.Z * q.Y))) / Math.PI * 180.0);
        //    Double rz = (Math.Atan2((2.0 * (q.W * q.Z + q.X * q.Y)), (1.0 - 2.0 * (q.X * q.X + q.Z * q.Z))) / Math.PI * 180.0);
        //    return new DxMath.Vector3 ((float)rx, (float)ry, (float)rz);
        //}

    }





    

}
