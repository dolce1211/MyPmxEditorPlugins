using FolderIconCreator.UI;
using PEPlugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderIconCreator
{
    public partial class FolderIconCreator
    {
        private frmSetting _frm = null;

        /// <summary>
        /// メイン処理です。
        /// </summary>
        private void Execute(IPERunArgs args)
        {
            try
            {
                this.IsGetClientImageAvailable(args);
            }
            catch (System.MissingMethodException)
            {
                //TransformView.GetClientImageが使えるのは0254g以降から。
                //0254gでPEPlugin.dllのバージョンが更新されていなかったためdllのファイルバージョンでは判定できず力技で判定。
                MessageBox.Show(@"pmxEditorのバージョンが足りません。
pmxEditor ver.0.2.5.4g以上を使ってください。");
                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // 起動時
            if (args.IsBootup)
            {
                // フォームの初期化
                _frm = new frmSetting(args);
            }
            else
            {
                if (_frm != null)
                {
                    // フォームの表示状態の変更
                    _frm.Dispose();
                }

                _frm = null;
                _frm = new frmSetting(args);
            }
            _frm.Show();
        }

        private void IsGetClientImageAvailable(IPERunArgs args)
        {
            args.Host.Connector.View.TransformView.GetClientImage();
        }
    }
}