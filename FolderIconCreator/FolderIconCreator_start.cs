///////////////////////////////////////////////////////////////////////////
//DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
//
//Copyright (C) 2004 Sam Hocevar <sam@hocevar.net>
//
//Everyone is permitted to copy and distribute verbatim or modified
//copies of this license document, and changing it is allowed as long
//as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
// 0. You just DO WHAT THE FUCK YOU WANT TO.
///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Form;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;
using PEPlugin.Vmd;
using PEPlugin.Vme;

///ネームスペースの名称は各自でソリューション名に変更してください。
namespace FolderIconCreator
{
    public partial class FolderIconCreator : PEPluginClass
    {
        /// <summary>
        /// PMDEditerを操作するために必要な変数群
        /// </summary>
        //-----------------------------------------------------------ここから-----------------------------------------------------------
        private IPEPluginHost host;

        //private IPEBuilder builder;
        //private IPEShortBuilder bd;
        private IPEConnector connect;

        //private IPEXPmd pex;
        private IPXPmx PMX;

        //private IPEPmd PMD;
        private IPEFormConnector Form;

        //private IPXPmxViewConnector PMXView;
        //private IPEPMDViewConnector PMDView;
        //-----------------------------------------------------------ここまで-----------------------------------------------------------
        // コンストラクタ
        public FolderIconCreator()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "モデルフォルダアイコン作成支援");
        }

        /// <summary>
        /// プラグイン名
        /// </summary>
        public override string Name
        {
            get
            {
                //ここにプラグインの名前を記述してください。
                return "モデルフォルダアイコン作成支援";
            }
        }

        /// <summary>
        /// バージョン情報
        /// </summary>
        public override string Version
        {
            get
            {
                //ここにプラグインのバージョンを記述して下さい。
                return "0.0.1";
            }
        }

        /// <summary>
        /// 説明
        /// </summary>
        public override string Description
        {
            get
            {
                //ここにプラグインの説明を記述して下さい。
                return "モデルが入ったフォルダのアイコン生成を支援します。";
            }
        }

        // エントリポイント　
        public override void Run(IPERunArgs args)
        {
            try
            {
                //PMD/PMXファイルを操作するためにおまじない。
                //不要なものはコメントアウト推奨
                this.host = args.Host;
                //this.builder = this.host.Builder;
                //this.bd = this.host.Builder.SC;
                this.connect = this.host.Connector;
                //this.pex = this.connect.Pmd.GetCurrentStateEx();

                //this.Form = this.connect.Form;
                //this.PMDView = this.connect.View.PMDView;
                //-----------------------------------------------------------ここから-----------------------------------------------------------
                //ここから処理開始
                //-----------------------------------------------------------ここから-----------------------------------------------------------

                this.Execute(args);

                //-----------------------------------------------------------ここまで-----------------------------------------------------------
                //処理ここまで
                //-----------------------------------------------------------ここまで-----------------------------------------------------------
                //必要がある場合はモデル・画面を更新します。
                //this.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// PMXE/PMDEの判別を行います。
        /// </summary>
        /// <param name="version">0:PMXE 1:PMDE</param>
        /// <returns></returns>
        private void CheckVersion(IPERunArgs args, int version = 0)
        {
            switch (version)
            {
                case 0:
                    if (args.Host.Name != "PmxEditor_PluginHost")
                    {
                        throw new System.Exception("PMXE専用です。");
                    }
                    break;

                case 1:
                    if (args.Host.Name != "PMDEditor_PluginHost")
                    {
                        throw new System.Exception("PMDE専用です。");
                    }
                    break;

                default:
                    throw new System.Exception("Versionの判別が不能です。");
            }
        }

        /// <summary>
        /// モデル・画面を更新します。
        /// </summary>
        private void Update()
        {
            //必要な部分のみ実行することを推奨
            this.connect.Pmx.Update(this.PMX);
            //必要な項目のみ更新することを推奨
            this.connect.Form.UpdateList(UpdateObject.All);
            this.connect.View.PMDView.UpdateModel();
            this.connect.View.PMDView.UpdateView();
        }
    }
}