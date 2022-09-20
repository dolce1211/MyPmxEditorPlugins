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
using SlimDX;
using MyUtility;
using System.Runtime.InteropServices;
using System.Drawing;
namespace MyExtension
{
    public static class MyExtensions
    {

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x000B;

        /// <summary>
        /// コントロールの再描画を停止させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void BeginControlUpdate(this Control control)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// コントロールの再描画を再開させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void EndControlUpdate(this Control control)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            control.Invalidate();
        }

        /// <summary>
        /// 引数のpmxでもって描画を更新します。
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pmx"></param>
        public static void Update(this IPERunArgs args,IPXPmx pmx)
        {
            //必要な部分のみ実行することを推奨
            args.Host.Connector.Pmx.Update(pmx);
            //必要な項目のみ更新することを推奨
            //args.Host.Connector.Form.UpdateList(UpdateObject.All);
            //args.Host.Connector.View.PMDView.UpdateModel();
            args.Host.Connector.View.PmxView.UpdateModel();
            args.Host.Connector.View.PmxView.UpdateView();
        }

        /// <summary>
        /// ファイルをインポートします。
        /// </summary>
        /// <param name="args"></param>
        /// <param name="path"></param>
        /// <param name="retpmx">インポートを反映させたpmxです。</param>
        /// <returns></returns>
        public static bool ImportPMX(this IPERunArgs args,  string path, ref IPXPmx retpmx)
        {
    
            IPXPmx pmx = args.Host.Connector.Pmx.GetCurrentState();
  
            if (!System.IO.File.Exists(path)) { return false; }

            if (!args.Host.Connector.Form.AppendPMDFile(path))
            {
                MessageBox.Show("ファイルの追加インポートに失敗しました", "エラー");
                return false;
            }

            //インポート済みのやつを反映させる
            retpmx = args.Host.Connector.Pmx.GetCurrentState();
            return true;
        }

        public static bool SetEnabled(this Control container, bool enabled, bool includeMyself)
        {
            List<Control> children = new List<Control>(); 
            container.CollectControl(includeMyself,ref children);
            foreach (Control  ctrl in children)
            {
                ctrl.Enabled = enabled;
            }
            return true;
        }

        public static  int CollectControl(this Control container, bool  includeMyself , ref List <Control> retCtrls)
        {
            if(retCtrls == null){ retCtrls = new List<Control>(); }
            if (includeMyself) { retCtrls.Add(container); }
       
            foreach (Control ctr in container.Controls)
            {
           
                if (ctr.HasChildren)
                {
                    List<Control> children = new List<Control>();
                    ctr.CollectControl(false,ref children );
                    foreach (Control  child in children )
                    {
                        retCtrls.Add(child);
                    }
                }
                else
                {
                    retCtrls.Add(ctr);
                }
            }

            return retCtrls.Count ;
        }


        /// <summary>
        /// 文字列の末尾から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string Right(this string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(str.Length - len, len);
        }

        #region ("Bone")

        /// <summary>
        /// ユーザが操作できるボーンならTrueを返します。
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        public static bool IsVisibleAndControllable(this IPXBone bone)
        {
            if (bone == null) { return false; }
            if(bone.AppendParent != null && (bone.IsAppendRotation || bone.IsAppendTranslation ))
            {
                //付与が入ってたらTrue扱いにする　
                //Todo:楽器プラグイン特別、あとで変えるかも）
                return true;
            }
            else
            {
                if (!bone.Visible) { return false; }
                if (!bone.Controllable) { return false; }
            }
            
            return true;
        }

        /// <summary>
        /// Local軸の設定を行う。
        /// </summary>
        /// <param name="Bone">ボーン</param>
        public static void SetLocalAxisTo(this IPXBone Bone,IPXBone target )
        {
            V3 X, Y, Z;

            if (! target.GetLocalAxisLocal (out X ,out Y, out Z)){
                MessageBox.Show(target.Name + "に向きがありません。" + Environment.NewLine + "Local軸の設定が出来ません。");
                return;
            }
           
            //MessageBox.Show(X.X.ToString() + "," + X.Y.ToString() + "," + X.Z.ToString() + "," +
            //               Y.X.ToString() + "," + Y.Y.ToString() + "," + Y.Z.ToString() + "," +
            //               Z.X.ToString() + "," + Z.Y.ToString() + "," + Z.Z.ToString() + ",");
            //Local軸を設定する。
            Bone.SetLocalAxis(X, Z);
            //カレントボーンの軸ON
            Bone.IsLocalFrame = true;
         
        }



        /// <summary>
        /// ボーンのToBone/ToOffsetの値からLocal軸の値を取得する。
        /// 要SlimDX
        /// </summary>
        /// <param name="Bone">ボーン</param>
        /// <param name="x">Local軸（X）</param>
        /// <param name="y">Local軸（Y）</param>
        /// <param name="z">Local軸（Z）</param>
        /// 
        private static bool GetLocalAxisLocal(this IPXBone Bone, out V3 x, out V3 y, out V3 z)
        {
            V3 V = Vector3.Zero;
            //ボーンの向きの取得
            if (Bone.IsToBone()  )
            {
                V = Bone.ToBone.Position - Bone.Position;
            }
            else
            {
                V = Bone.ToOffset;
            }
            //向きがある場合のみ処理。
            if (V != Vector3.Zero)
            {
                V3 X = new V3(1.0f, 0.0f, 0.0f);
                V3 Y = new V3(0.0f, 1.0f, 0.0f);
                V3 Z = new V3(0.0f, 0.0f, 1.0f);
                V.Normalize();
                //向きがZ方向の場合
                if (V == Vector3.UnitZ)
                {
                    X = new V3(0.0f, 0.0f, 1.0f);
                    Z = new V3(-1.0f, 0.0f, 0.0f);

                }
                //向きが-Z方向の場合
                else if (V == -Vector3.UnitZ)
                {
                    X = new V3(0.0f, 0.0f, -1.0f);
                    Z = new V3(1.0f, 0.0f, 0.0f);
                }
                //それ以外の場合
                else
                {
                    //向きが単位ベクトルXになるクォータニオンを取得
                    Q Qu = Q.Dir(V, new V3(0.0f, 0.0f, 1.0f), new V3(1.0f, 0.0f, 0.0f), new V3(0.0f, 0.0f, 1.0f));
                    //クォータニオンから回転行列を取得
                    Matrix Ma = Qu.ToMatrix();

                    //回転行列の要素から各軸の向きを取得（PMXEの結果と微妙にずれるときがある。）
                    X = new V3(Ma.M11, Ma.M12, Ma.M13);
                    Y = new V3(Ma.M21, Ma.M22, Ma.M23);
                    Z = new V3(Ma.M31, Ma.M32, Ma.M33);
                }
                //取得した向きをノーマライズ
                X.Normalize();
                Y.Normalize();
                Z.Normalize();
                x = X;
                y = Y;
                z = Z;
            }
            else
            {
                //throw new System.Exception(Bone.Name + "に向きがありません。");
                x = new V3(1.0f, 0.0f, 0.0f);
                y = new V3(0.0f, 1.0f, 0.0f);
                z = new V3(0.0f, 0.0f, 1.0f);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ボーンのToBoneが有効かどうか
        /// </summary>
        /// <param name="Bone">ボーン</param>
        /// <returns></returns>
        public static  bool IsToBone(this IPXBone Bone)
        {
            if (Bone.ToBone == null)
            {
                return false;
            }
            else
            {
                if (Bone.ToOffset.X == 0 && Bone.ToOffset.Y == 0 && Bone.ToOffset.Z == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IKLinkの追加
        /// IK角度制限をしない場合はこちら
        /// </summary>
        /// <param name="Bone">IKボーン</param>
        /// <param name="LinkBone">IKLinkボーン</param>
        public static void AddIKLink(this IPXBone Bone, IPXBone LinkBone)
        {
            IPXIKLink IKLink = (IPXIKLink)PEStaticBuilder.Pmx.IKLink();
            IKLink.Bone = LinkBone;
            Bone.IK.Links.Add(IKLink);
        }
        /// <summary>
        /// IKLinkLimitの追加2
        /// IKLinkのHigh/Lowに入れる値はラジアンでPMDE上の表記は度数なので気をつけること。
        /// </summary>
        /// <param name="Bone">IKボーン</param>
        /// <param name="LinkBone">IKLinkボーン</param>
        /// <param name="LimitHigh">制限最大値（入力値はラジアン）</param>
        /// <param name="LimitLow">制限最小値（入力値はラジアン）</param>
        public static void AddIKLinkLimit(this IPXBone Bone, IPXBone LinkBone, V3 LimitHigh, V3 LimitLow)
        {
            IPXIKLink IKLink = (IPXIKLink)PEStaticBuilder.Pmx.IKLink();
            IKLink.Bone = LinkBone;
            IKLink.IsLimit = true;
            IKLink.High = LimitHigh;
            IKLink.Low = LimitLow;
            Bone.IK.Links.Add(IKLink);
        }
        /// <summary>
        /// IKLinkLimitの追加2
        /// IKLinkのHigh/Lowに入れる値は度数です。
        /// </summary>
        /// <param name="Bone">IKボーン</param>
        /// <param name="LinkBone">IKLinkボーン</param>
        /// <param name="LimitHigh">制限最大値（入力値は度）</param>
        /// <param name="LimitLow">制限最小値（入力値は度）</param>
        public static void AddIKLinkLimitDegree(this IPXBone Bone, IPXBone LinkBone, V3 LimitHighd, V3 LimitLowd, bool isChecked= true )
        {
            IPXIKLink IKLink = (IPXIKLink)PEStaticBuilder.Pmx.IKLink();
            IKLink.Bone = LinkBone;
            IKLink.IsLimit = isChecked;
            IKLink.High = new V3((float)(LimitHighd.X / 180 * Math.PI)
                                , (float)(LimitHighd.Y / 180 * Math.PI)
                                , (float)(LimitHighd.Z / 180 * Math.PI)
                                );
            IKLink.Low = new V3((float)(LimitLowd.X / 180 * Math.PI)
                               , (float)(LimitLowd.Y / 180 * Math.PI)
                               , (float)(LimitLowd.Z / 180 * Math.PI)
                               );
            Bone.IK.Links.Add(IKLink);
        }

        /// <summary>
        /// <para>回転連動ボーンの連動値を設定する。</para>
        /// <para>bone:対象ボーン名</para>
        /// <para>appendbone:影響ボーン名</para>
        /// <para>ratio:影響比率(float) -n ～ n</para>
        /// </summary>
        public static void SetAppendRotation(this IPXBone Bone, IPXBone AppendBone, float Ratio)
        {
            Bone.AppendParent = AppendBone;
            Bone.AppendRatio = Ratio;
            Bone.IsAppendRotation = true;
            Bone.IsAppendTranslation = false;
        }
        /// <summary>
        /// <para>移動連動ボーンの連動値を設定する。</para>
        /// <para>bone:対象ボーン</para>
        /// <para>appendbone:影響ボーン</para>
        /// <para>ratio:影響比率(float) -n ～ n</para>
        /// </summary>
        public static void SetAppendTranslation(this IPXBone Bone, IPXBone AppendBone, float Ratio)
        {
            Bone.AppendParent = AppendBone;
            Bone.AppendRatio = Ratio;
            Bone.IsAppendRotation = false;
            Bone.IsAppendTranslation = true;
        }



        /// <summary>
        /// IKの追加
        /// </summary>
        /// <param name="Bone">IKボーン</param>
        /// <param name="TargetBone">IKターゲットボーン</param>
        /// <param name="Angle">単位角（ラジアン）</param>
        /// <param name="LoopCount">ループ数</param>
        public static void AddIK(this IPXBone Bone, IPXBone TargetBone, float Angle = 1.0f, int LoopCount = 20)
        {
            Bone.IsIK = true;
            //コード内はラジアンでPMDE上の表記は度数なので気をつけること。

            Bone.IK.Angle = Angle; //Angle;
            Bone.IK.LoopCount = LoopCount;
            Bone.IK.Target = TargetBone;
        }

        /// <summary>
        /// <para>回転・移動連動ボーンの連動値を設定する。</para>
        /// <para>bone:対象ボーン名</para>
        /// <para>appendbone:影響ボーン名</para>
        /// <para>ratio:影響比率(float) -n ～ n</para>
        /// </summary>
        public static void SetAppend(this IPXBone Bone, IPXBone AppendBone, float Ratio)
        {
            Bone.AppendParent = AppendBone;
            Bone.AppendRatio = Ratio;
            Bone.IsAppendRotation = true;
            Bone.IsAppendTranslation = true;
        }
        /// <summary>
        /// ボーンのToOffsetを設定する。
        /// </summary>
        /// <param name="Bone">ボーン</param>
        /// <param name="Offset">オフセット値</param>
        public static void SetToOffset(this IPXBone Bone, V3 Offset)
        {
            Bone.ToBone = null;
            Bone.ToOffset = Offset;
        }

        /// <summary>
        /// ボーンのToBoneを設定する。
        /// </summary>
        /// <param name="Bone">ボーン</param>
        /// <param name="ToBone">先ボーン</param>
        public static void SetToBone(this IPXBone Bone, IPXBone ToBone)
        {
            Bone.ToBone = ToBone;
            Bone.ToOffset = new V3(0.0f, 0.0f, 0.0f);
        }

        #endregion

        
        #region ("PMX")

        /// <summary>
        /// ★指定したボーンを付与親に持つボーンを取得します。
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static List<IPXBone> TryGetBoneListWithAppendParnet(this IPXPmx PMX, IPXBone argBone)
        {
            List<IPXBone> result = new List<IPXBone>();

            foreach (IPXBone bone in PMX.Bone)
            {
                if (bone.IsAppendRotation || bone.IsAppendTranslation)
                {
                    if (bone.AppendParent == argBone)
                    {
                        result.Add(bone);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// このボーンを付与親に持つボーンが存在すればTrueを返します。
        /// </summary>
        /// <param name="PMX"></param>
        /// <param name="argBone"></param>
        /// <returns></returns>
        public static bool  AppendBoneExists(this IPXPmx PMX, IPXBone argBone)
        {
            if(PMX.TryGetBoneListWithAppendParnet (argBone).Count > 0)
            {
                return true;
            }
             return false ;
        }

        /// <summary>
        /// 指定されたボーンを表示枠から除去します。
        /// </summary>
        /// <param name="delBoneList"></param>
        /// <returns></returns>
        public static bool DeleteBoneFromFrame(this IPXPmx pmx, IPXBone[] delBoneList)
        {
            bool result = false;
            foreach (IPXNode node in pmx.Node)
            {
                foreach (IPXBone bone in delBoneList)
                {
                    IPXNodeItem delNodeItem = null;
                    foreach (IPXNodeItem item in node.Items)
                    {
                        if (item.BoneItem != null)
                        {
                            if (item.BoneItem.Bone == bone)
                            {
                                //MessageBox.Show(item.BoneItem.Bone.Name);
                                delNodeItem = item;
                                break;
                            }
                        }
                    }
                    if (delNodeItem != null)
                    {
                        node.Items.Remove(delNodeItem);
                        result = true;
                    }
                }
            }
            return result;
        }

 

        /// <summary>
        /// ボーンIndexを返す関数です。
        /// </summary>
        /// <param name="BoneName">ボーン名</param>
        /// <returns></returns>
        public static int GetBoneIndex(this IPXPmx pmx, string BoneName, int e = 0)
        {
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if (pmx.Bone[i].Name == BoneName) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(BoneName + "ボーンが存在しません。");
            }
        }



        /// <summary>
        /// ボーンIndexを返す関数です。
        /// </summary>
        /// <param name="Bone">ボーン</param>
        /// <returns></returns>
        public static int GetBoneIndex(this IPXPmx pmx, IPXBone Bone, int e = 0)
        {
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if (pmx.Bone[i] == Bone) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(Bone.Name + "ボーンが存在しません。");
            }
        }

        /// <summary>
        /// ボーンの追加
        /// ボーンの上側に追加
        /// </summary>
        /// <param name="Bone">追加したいボーン</param>
        /// <param name="TargetBone">追加位置ボーン</param>
        public static void UpperInsertBone(this IPXPmx pmx, IPXBone Bone, IPXBone TargetBone)
        {
            pmx.Bone.Insert(pmx.Bone.IndexOf(TargetBone), Bone);
        }
        /// <summary>
        /// ボーンの追加
        /// ボーンの下側に追加
        /// </summary>
        /// <param name="Bone">追加したいボーン</param>
        /// <param name="TargetBone">追加位置ボーン</param>
        public static void LowerInsertBone(this IPXPmx pmx, IPXBone Bone, IPXBone TargetBone)
        {
            pmx.Bone.Insert(pmx.Bone.IndexOf(TargetBone) + 1, Bone);
        }

        
        /// <summary>
        /// 引数の名前を含むボーンをすべて拾って返します。
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static List<IPXBone> TryFindBones(this IPXPmx pmx, string  srcBoneName)
        {
            List<IPXBone> result = new List<IPXBone>();
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if (pmx.Bone[i].Name.ToLower().IndexOf ( srcBoneName.ToLower())>=0  )
                {
                    result.Add (pmx.Bone [i]);
                }
            }
            return result ;
        }
        public static IPXBone TryFindBoneByName(this IPXPmx pmx, string srcBoneName)
        {
            List<IPXBone> result = new List<IPXBone>();
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if (pmx.Bone[i].Name.ToLower() == srcBoneName.ToLower())
                {
                    return pmx.Bone[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 引数の名前を含む材質をすべて拾って返します。
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static List<IPXMaterial> TryFindMaterials(this IPXPmx pmx, string srcMatName)
        {
            List<IPXMaterial> result = new List<IPXMaterial>();
            for (int i = 0; i < pmx.Material.Count; i++)
            {
                if (pmx.Material[i].Name.ToLower().IndexOf(srcMatName.ToLower()) >= 0)
                {
                    
                    result.Add(pmx.Material[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// ★指定したモーフを取得
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXMorph TryGetMorph(this IPXPmx pmx, string srcMorphName)
        {
            for (int i = 0; i < pmx.Morph.Count; i++)
            {
                if (pmx.Morph[i].Name.ToLower() == srcMorphName.ToLower()) { return pmx.Morph[i]; }
            }
            return null;
        }


        /// <summary>
        /// グループモーフの追加
        /// </summary>
        /// <param name="GroupMorphName">モーフ名</param>
        /// <param name="GroupMorphNameE">モーフ名英</param>
        /// <param name="panel">表示種別</param>
        /// <param name="position">挿入位置</param>
        public static void AddGroupMorph(this IPXPmx pmx, string GroupMorphName, string GroupMorphNameE, int panel, int position)
        {
            pmx.AddMorph(GroupMorphName, GroupMorphNameE, MorphKind.Group, panel, position);
        }

        /// <summary>
        /// モーフの追加
        /// </summary>
        /// <param name="MorphName">モーフ名</param>
        /// <param name="MorphNameE">モーフ名英</param>
        /// <param name="kind">モーフ種別</param>
        /// <param name="panel">表示種別</param>
        /// <param name="position">挿入位置</param>
        public static void AddMorph(this IPXPmx pmx, string MorphName, string MorphNameE, MorphKind kind, int panel, int position)
        {
            int i = pmx.GetMorphIndex(MorphName);
            if (i == -1)
            {
                IPXMorph item = PEStaticBuilder.Pmx.Morph();
                item.Name = MorphName;
                item.NameE = MorphNameE;
                item.Kind = kind;
                item.Panel = panel;
                pmx.Morph.Insert(position, item);
            }
            else
            {
                throw new System.Exception(MorphName + "が既に存在します。");
            }
        }

        /// <summary>
        /// パネル番号をパネル名へ
        /// </summary>
        /// <param name="morph"></param>
        /// <returns></returns>
        public static string Panel2PanelNum(this IPXMorph morph)
        {
            switch (morph.Panel)
            {
                case 1:return "まゆ";
                case 2: return "目";
                case 3: return "ﾘｯﾌﾟ";
                case 4: return "その他";
                default:
                    break;
            }
            return string.Empty;
        }
        /// <summary>
        /// テクスチャをいじっている材質モーフならＴｒｕｅを返します。使いにくいのですぐわかるように
        /// </summary>
        /// <param name="morph"></param>
        /// <returns></returns>
        public static bool IsUnApplyableMorph(this IPXMorph morph)
        {
            if (morph.IsBone || morph.IsFlip || morph.IsImpulse)
                return true; //こいつらは対象外
            else if (morph.IsMaterial)
            {
                foreach (IPXMaterialMorphOffset offset in morph.Offsets)
                {
                    if (offset.Op == 0)
                    {

                        if (!(offset.Tex.A == 1 && offset.Tex.R == 1 && offset.Tex.G == 1 && offset.Tex.B == 1))
                            return true;
                        else if (!(offset.Sphere.A == 1 && offset.Sphere.R == 1 && offset.Sphere.G == 1 && offset.Sphere.B == 1))
                            return true;
                        else if (!(offset.Toon.A == 1 && offset.Toon.R == 1 && offset.Toon.G == 1 && offset.Toon.B == 1))
                            return true;
                    }
                    else
                    {
                        //加算
                        if (!(offset.Tex.A == 0 && offset.Tex.R == 0 && offset.Tex.G == 0 && offset.Tex.B == 0))
                            return true;
                        else if (!(offset.Sphere.A == 0 && offset.Sphere.R == 0 && offset.Sphere.G == 0 && offset.Sphere.B == 0))
                            return true;
                        else if (!(offset.Toon.A == 0 && offset.Toon.R == 0 && offset.Toon.G == 0 && offset.Toon.B == 0))
                            return true;
                    }

                }
            }
            else if (morph.IsGroup)
            {
                //グループモーフなら、内訳にあかんモーフがふくまれてればTreu
                foreach (var item in morph.Offsets)
                {
                    IPXGroupMorphOffset grpoffset = (IPXGroupMorphOffset)item;

                    if (grpoffset.Morph.IsUnApplyableMorph())
                    {
                        return true;
                    }
                }
            }

            return false;
            
        }

        /// <summary>
        /// グループモーフの要素追加
        /// </summary>
        /// <param name="GroupMorph">グループモーフ</param>
        /// <param name="Morph">モーフ</param>
        /// <param name="ratio">影響度</param>
        public  static void AddGroupMorphItem(this IPXPmx pmx, IPXMorph GroupMorph, IPXMorph Morph, float ratio = 1.0f)
        {
            int i = GetMorphIndex(pmx,GroupMorph);
            if (i >= 0)
            {
                int k = GetMorphIndex(pmx,Morph);
                if (k >= 0)
                {
                    if (GroupMorph.Kind == MorphKind.Group)
                    {
                        IPXGroupMorphOffset item = PEStaticBuilder.Pmx.GroupMorphOffset();
                        item.Morph = Morph;
                        item.Ratio = ratio;
                        GroupMorph.Offsets.Add(item);
                    }
                    else
                    {
                        throw new System.Exception("モーフの種類が間違っています。");
                    }
                }
                else
                {
                    throw new System.Exception(Morph.Name + "が存在しません。");
                }
            }
            else
            {
                throw new System.Exception(GroupMorph.Name + "が存在しません。");
            }
        }
        /// <summary>
        /// モーフIndexを取得する。
        /// </summary>
        /// <param name="Morph">モーフ</param>
        /// <param name="e">エラー種別</param>
        /// <returns>モーフIndex</returns>
        private static int GetMorphIndex(this IPXPmx pmx, IPXMorph Morph, int e = 0)
        {
            for (int i = 0; i < pmx.Morph.Count; i++)
            {
                if (pmx.Morph[i] == Morph) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(Morph.Name + " 表情が存在しません。");
            }
        }
        /// <summary>
        /// モーフIndexを取得する。
        /// </summary>
        /// <param name="MorphName">モーフ名</param>
        /// <param name="e">エラー種別</param>
        /// <returns>モーフIndex</returns>
        private static int GetMorphIndex(this IPXPmx pmx, string MorphName, int e = 0)
        {
            for (int i = 0; i < pmx.Morph.Count; i++)
            {
                if (pmx.Morph[i].Name == MorphName) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(MorphName + " 表情が存在しません。");
            }
        }

        /// <summary>
        /// 表示枠の追加
        /// 表示枠の上側に追加
        /// </summary>
        /// <param name="node">表示枠</param>
        /// <param name="TargetNode">追加したい位置にある表示枠</param>
        public static void UpperInsertFrame(this IPXPmx pmx, IPXNode node, IPXNode TargetNode)
        {
            pmx.Node.Insert(pmx.Node.IndexOf(TargetNode), node);
        }


        /// <summary>
        /// ★指定した表示枠を取得し、無ければ足して返す
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static  IPXNode TryGetAndAddNode(this IPXPmx pmx, string srcNodeName, int index)
        {
            for (int i = 0; i < pmx.Node.Count; i++)
            {
                if (pmx.Node[i].Name == srcNodeName) { return pmx.Node[i]; }
            }
            //無ければ足す
            IPXNode node = PEStaticBuilder.Pmx.Node();
            node.Name = srcNodeName;
            if (index >= 0 && index < pmx.Node.Count)
            {
                pmx.Node.Insert(index, node);
            }
            else
            {
                pmx.Node.Add(node);
            }

            return node;
        }
        /// <summary>
        /// ★指定した表示枠を取得します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static bool  TryGetNode(this IPXPmx pmx, string srcNodeName)
        {
            for (int i = 0; i < pmx.Node.Count; i++)
            {
                if (pmx.Node[i].Name.IndexOf(srcNodeName) >= 0) { return true; }
            }
            return false;
        }

        /// <summary>
        /// ★指定した骨名の骨が属する表示枠を返します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXNode TryFindNodeByBone(this IPXPmx pmx, IPXBone bone)
        {
            int retIndex = -1;
            return TryFindNodeByBone(pmx, bone, ref retIndex);
        }
        /// <summary>
        /// ★指定した骨名の骨が属する表示枠を返します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXNode TryFindNodeByBone(this IPXPmx pmx, string boneName)
        {
            int retIndex = -1;
            return TryFindNodeByBone(pmx, boneName, ref retIndex);
        }
        /// <summary>
        /// ★指定した骨名の骨が属する表示枠を返します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXNode TryFindNodeByBone(this IPXPmx pmx, string boneName, ref int retIndex)
        {
            IPXBone bone = pmx.TryGetBone(boneName);
            if( bone!= null){
                return TryFindNodeByBone(pmx, bone, ref retIndex);
            }
            return null;
        }

        /// <summary>
        /// ★指定した骨が属する表示枠を返します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXNode TryFindNodeByBone(this IPXPmx pmx, IPXBone bone, ref int retIndex)
        {
            IPXNodeItem dummy = null;
            return pmx.TryFindNodeByBone(bone, ref retIndex, ref dummy); 
        }


        /// <summary>
        /// ★指定した骨が属する表示枠を返します
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXNode TryFindNodeByBone(this IPXPmx pmx, IPXBone bone, ref int retIndex, ref IPXNodeItem retNodeItem  )
        {
            foreach (IPXNode tmpnode in pmx.Node)
            {
                int index = 0;
                foreach (IPXNodeItem nodeItem in tmpnode.Items)
                {

                    if (nodeItem.IsBone)
                    {
                        if (nodeItem.BoneItem.Bone == bone)
                        {

                            retIndex = index;
                            retNodeItem = nodeItem; 
                            return tmpnode;
                        }
                    }
                    index += 1;
                }
            }
            return null;
        }



        /// <summary>
        /// ★指定したボーンを取得
        /// </summary>
        /// <param name="srcBoneName"></param>
        /// <returns></returns>
        public static IPXBone TryGetBone(this IPXPmx pmx, string srcBoneName)
        {
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if (pmx.Bone[i].Name.ToLower () == srcBoneName.ToLower() ) { return pmx.Bone[i]; }
            }
            return null;
        }


        /// <summary>
        /// 指定したボーンの先祖に持つボーンをすべて取得します。
        /// </summary>
        /// <param name="pmx"></param>
        /// <param name="bone"></param>
        /// <returns></returns>
        public static List<IPXBone> CollectChildren(this IPXPmx pmx,IPXBone bone )
        {
            List<IPXBone> result = new List<IPXBone>();

            CollectChildrenInternal(pmx, bone,ref result);

            return result;
        }
        private static void CollectChildrenInternal(IPXPmx pmx, IPXBone parent , ref List <IPXBone > result)
        {
            if (parent == null) { return; }

            //この階層で得た結果
            List<IPXBone> currentResult = new List<IPXBone>();
            foreach (IPXBone bone in pmx.Bone)
            {
                if (!result.Contains(bone)) { 
                    if (bone == parent || bone.Parent == parent)
                    {
                        currentResult.Add(bone);
                    }
                }
            }
            if (currentResult.Count > 0)
            {
                result.AddRange(currentResult);
                foreach (var bone in currentResult)
                {
                    //boneの子を再帰で撮りに行く
                    CollectChildrenInternal(pmx, bone,ref result);
                }
            }

        }



        /// <summary>
        /// ★ボーンの親を作成する
        /// </summary>
        /// <param name="basebone"></param>
        /// <returns></returns>
        public static IPXBone CreateParentBone(this IPXPmx pmx, IPXBone basebone, string addName)
        {
            if (basebone == null) return null;
            IPXBone clone = (IPXBone)basebone.Clone();
            clone.Name += addName;
            clone.NameE += addName;

            int boneIndex = -1;
            for (int i = 0; i < pmx.Bone.Count; i++)
            {
                if(pmx.Bone[i]== basebone)
                {
                    boneIndex = i;
                    break;
                }
            }
            if (boneIndex < 0) return null;

            pmx.Bone.Insert(boneIndex, clone);
            basebone.Parent = clone;

            return clone;
        }

        #endregion

        /// <summary>
        /// このノードにfromBoneが存在すれば、toBoneに置き換えます。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pmx"></param>
        /// <param name="fromBone"></param>
        /// <param name="toBone"></param>
        public static bool  SwapBoneFrameItem(this IPXNode node, IPXPmx pmx, IPXBone fromBone, IPXBone toBone)
        {
            bool result = false;
            int origIndex = -1;
            IPXNodeItem retNodeItem = null;
            IPXNode foundNode = pmx.TryFindNodeByBone (fromBone,ref origIndex,ref retNodeItem);
            if(foundNode != null)
            {
                
                foundNode.Items.Remove(retNodeItem);
                foundNode.AddBoneFrameItem(pmx, toBone, origIndex);

            }
            return result;
        }



        public static void AddBoneFrameItem(this IPXNode node,IPXPmx pmx, IPXBone Bone, int index)
        {
            pmx.AddBoneFrameItem(node,Bone, index);
        }



        /// <summary>
        /// ボーン表示枠の要素追加
        /// </summary>
        /// <param name="Frame">表示枠名</param>
        /// <param name="BoneName">ボーン名</param>
        public static void AddBoneFrameItem(this IPXPmx pmx, string Frame, string BoneName)
        {
            IPXNode node = pmx.Node[pmx.GetFrameIndex(Frame, 1)];
            IPXBone Bone = pmx.Bone[pmx.GetBoneIndex(BoneName, 1)];
            AddBoneFrameItem(pmx,node, Bone);
        }
        /// <summary>
        /// ボーン表示枠の要素追加
        /// </summary>
        /// <param name="node">表示枠</param>
        /// <param name="Bone">ボーン</param>
        public static void AddBoneFrameItem(this IPXPmx pmx, IPXNode node, IPXBone Bone)
        {
            AddBoneFrameItem(pmx,node, Bone, -1);
        }
        public static void AddBoneFrameItem(this IPXPmx pmx, IPXNode node, IPXBone Bone, int index)
        {
            int i = pmx.GetFrameIndex(node);
            if (i >= 0)
            {
                int j = pmx.GetBoneIndex(Bone);
                if (j >= 0)
                {
                    IPXBoneNodeItem NodeItem = PEStaticBuilder.Pmx.BoneNodeItem();
                    NodeItem.Bone = pmx.Bone[j];

                    if (index < 0 | index >= node.Items.Count())
                    {
                        node.Items.Add(NodeItem);
                    }
                    else
                    {
                        node.Items.Insert(index, NodeItem);
                    }

                }
                else
                {
                    throw new System.Exception(Bone.Name + "が存在しません。");
                }
            }
            else
            {
                throw new System.Exception(node.Name + "が存在しません。");
            }
        }
        /// <summary>
        /// 表示枠Indexを取得する。
        /// </summary>
        /// <param name="Frame">表示枠名</param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int GetFrameIndex(this IPXPmx pmx, string Frame, int e = 0)
        {
            for (int i = 0; i < pmx.Node.Count; i++)
            {
                if (pmx.Node[i].Name == Frame) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(Frame + "表示枠が存在しません。");
            }
        }
        /// <summary>
        /// 表示枠Indexを取得する。
        /// </summary>
        /// <param name="Frame">表示枠</param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int GetFrameIndex(this IPXPmx pmx, IPXNode Frame, int e = 0)
        {
            for (int i = 0; i < pmx.Node.Count; i++)
            {
                if (pmx.Node[i] == Frame) { return i; }
            }
            if (e == 0)
            {
                return -1;
            }
            else
            {
                throw new System.Exception(Frame.Name + "表示枠が存在しません。");
            }
        }


        /// <summary>
        /// ボーン表示枠の追加
        /// </summary>
        /// <param name="Frame">表示枠名</param>
        public static void AddBoneFrame(this IPXPmx pmx, string Frame)
        {
            int i = pmx.GetFrameIndex(Frame);
            if (i == -1)
            {
                IPXNode node = PEStaticBuilder.Pmx.Node();
                node.Name = Frame;
                pmx.Node.Add(node);
            }
            else
            {
                throw new System.Exception(Frame + "は既に存在します。");
            }
        }
        /// <summary>
        /// ボーン表示枠の削除
        /// </summary>
        /// <param name="Frame">表示枠名</param>
        public static void DelBoneFrame(this IPXPmx pmx, string Frame)
        {
            int i = pmx.GetFrameIndex(Frame, 1);
            IPXNode node = (IPXNode)pmx.Node[i];
            pmx.DelBoneFrame(node);
        }
        /// <summary>
        /// ボーン表示枠の削除
        /// </summary>
        /// <param name="node">表示枠</param>
        public static void DelBoneFrame(this IPXPmx pmx, IPXNode node)
        {
            pmx.Node.Remove(node);
        }
        /// <summary>
        /// ボーン表示枠の内容全削除
        /// </summary>
        /// <param name="Frame">表示枠名</param>
        /// <returns></returns>
        public static void ClearBoneFrame(this IPXPmx pmx, string Frame)
        {
            IPXNode node = (IPXNode)pmx.Node[pmx.GetFrameIndex(Frame, 1)];
            pmx.ClearBoneFrame(node);
        }
        public static void ClearBoneFrame(this IPXPmx pmx, IPXNode node)
        {
            node.Items.Clear();
            pmx.Node.Remove(node);
        }

        public static int GetMorphNum(this IPXMorph m, IPXPmx pmx )
        {
            Dictionary<IPXMorph, int> morphNumHash = pmx.GetMorphNumHash();
            if (morphNumHash.ContainsKey(m))
                return morphNumHash[m];
            return -1;
        }
        public static Dictionary<IPXMorph, int> GetMorphNumHash(this IPXPmx pmx)
        {
            Dictionary<IPXMorph, int> result = new Dictionary<IPXMorph, int>();
            int c = 0;
            foreach (var item in pmx.Morph)
            {
                result.Add(item, c);
                c += 1;
            }
            return result;
        }

        /// <summary>
        /// 現在のモーフ選択状態を文字列にして返します。
        /// </summary>
        /// <param name="tv"></param>
        /// <param name="pmx"></param>
        /// <returns></returns>
        public static string GetCurrentMorphStateString(this IPETransformViewConnector tv, IPXPmx pmx,bool addNum =false)
        {
            Dictionary<IPXMorph, float> hash = tv.GetCurrentMorphState(pmx);
            if (hash == null || hash.Count == 0) return string.Empty;
            _addNumForMorphNameAndRatioFunc = addNum;
            _pmxForMorphNameAndRatioFunc = pmx;
            var str = string.Join
                                ("+"
                                    , hash.Select(MorphNameAndRatioFunc)
                                );
            _addNumForMorphNameAndRatioFunc = false;
            _pmxForMorphNameAndRatioFunc = null;
            return str;
        }
        private static IPXPmx _pmxForMorphNameAndRatioFunc = null;
        private static bool _addNumForMorphNameAndRatioFunc = false;
        private static string MorphNameAndRatioFunc(KeyValuePair<IPXMorph, float> kvp)
        {
            string result = string.Empty;
            if (_addNumForMorphNameAndRatioFunc)
                result = kvp.Key.GetMorphNum(_pmxForMorphNameAndRatioFunc).ToString()+" : " ;

            if (kvp.Value == 1)
            {
                result += kvp.Key.Name;
            }
            else
            {
                result += string.Format("{0}({1})", kvp.Key.Name, kvp.Value.ToString());
            }
            return result;
        }

        /// <summary>
        /// 現在のTransformViewのモーフ適用状態を取得して返します。
        /// </summary>
        /// <param name="tv"></param>
        /// <param name="pmx"></param>
        /// <returns></returns>
        public static Dictionary<IPXMorph, float> GetCurrentMorphState(this IPETransformViewConnector tv,IPXPmx pmx)
        {
            Dictionary<IPXMorph, float> result = new Dictionary<IPXMorph, float>();
            if (tv == null) return result;
            //モーフ番号を取得するためのハッシュを作る
            Dictionary<IPXMorph, int> morphNumHash = pmx.GetMorphNumHash();

            int prevIndex = tv.SelectedMorphIndex;
            
            Control ctrl = (Control )tv;
            if (ctrl.IsDisposed) return null;
            ctrl.BeginControlUpdate();
            try
            {
                foreach (var m in pmx.Morph)
                {
                    tv.SelectedMorphIndex = morphNumHash[m];
                    if (tv.MorphValue > 0)
                        result.Add(m, tv.MorphValue);
                }
            }
            finally
            {
                tv.SelectedMorphIndex = prevIndex;
                ctrl.EndControlUpdate();
                ctrl.Refresh();
                
            }
            return result;
        }

        /// <summary>
        /// 画像に文字を書き込みます。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="name"></param>
        /// <param name="font"></param>
        /// <param name="letterColor"></param>
        /// <param name="rimColor"></param>
        /// <param name="rimWidth"></param>
        public static void AddTextToPicture(this Graphics g, Rectangle rect 
                                            ,int pos ,string letters, Font font,Color letterColor, Color rimColor,float rimWidth)
        {
            Size nopadSize = TextRenderer.MeasureText(g, letters, font
                            , new Size(1000, 1000), TextFormatFlags.NoPadding);
            

            int top = 5;
            if (pos == 1)
            {
                //下
                top = rect.Height - nopadSize.Height - 5;
            }

            System.Drawing.Drawing2D.SmoothingMode prevsm = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //GraphicsPathオブジェクトの作成
            System.Drawing.Drawing2D.GraphicsPath gp =
                new System.Drawing.Drawing2D.GraphicsPath();
            //GraphicsPathに文字列を追加する
            FontFamily ff = new FontFamily(font.Name);
            gp.AddString(letters, ff, (int)font.Style, font.Size,
                new Point(10, top), StringFormat.GenericDefault);

            Brush lb = new SolidBrush(letterColor);
            Pen  rp = new Pen(rimColor,rimWidth);

            //文字列の中を塗りつぶす
            g.FillPath(lb, gp);
            //文字列の縁を描画する
            g.DrawPath(rp, gp);

            //リソースを解放する
            ff.Dispose();

            g.SmoothingMode = prevsm;
            //g.Dispose();

        }
        ///// <summary>
        ///// ボーン表示枠内の要素Indexを取得する。
        ///// </summary>
        ///// <param name="Frame">表示枠名</param>
        ///// <param name="BoneName">ボーン名</param>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //private int GetBoneFrameItemIndex(string Frame, string BoneName, int e = 0)
        //{
        //    IPXNode node = pmx.Node[this.GetFrameIndex(Frame, 1)];
        //    IPXBone Bone = pmx.Bone[this.GetBoneIndex(BoneName, 1)];
        //    return this.GetBoneFrameItemIndex(node, Bone);
        //}
        ///// <summary>
        ///// ボーン表示枠内の要素Indexを取得する。
        ///// </summary>
        ///// <param name="Frame">表示枠</param>
        ///// <param name="BoneName">ボーン名</param>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //private int GetBoneFrameItemIndex(IPXNode Frame, string BoneName, int e = 0)
        //{
        //    IPXBone Bone = pmx.Bone[this.GetBoneIndex(BoneName, 1)];
        //    return this.GetBoneFrameItemIndex(Frame, Bone);
        //}
        ///// <summary>
        ///// ボーン表示枠内の要素Indexを取得する。
        ///// </summary>
        ///// <param name="Frame">表示枠名</param>
        ///// <param name="Bone">ボーン</param>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //private int GetBoneFrameItemIndex(string Frame, IPXBone Bone, int e = 0)
        //{
        //    IPXNode node = pmx.Node[this.GetFrameIndex(Frame, 1)];
        //    return this.GetBoneFrameItemIndex(node, Bone);
        //}
        ///// <summary>
        ///// ボーン表示枠内の要素Indexを取得する。
        ///// </summary>
        ///// <param name="Frame">表示枠</param>
        ///// <param name="Bone">ボーン</param>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //private int GetBoneFrameItemIndex(IPXNode node, IPXBone Bone, int e = 0)
        //{
        //    int i = this.GetFrameIndex(node);
        //    if (i >= 0)
        //    {
        //        int j = this.GetBoneIndex(Bone);
        //        if (j >= 0)
        //        {
        //            for (int k = 0; k < node.Items.Count; k++)
        //            {
        //                if (node.Items[k].IsBone)
        //                {
        //                    if (node.Items[k].BoneItem.Bone == Bone) { return k; }
        //                }
        //            }
        //        }
        //        if (e == 0)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            throw new System.Exception(Bone.Name + "が存在しません。");
        //        }

        //    }
        //    if (e == 0)
        //    {
        //        return -1;
        //    }
        //    else
        //    {
        //        throw new System.Exception(node.Name + "が存在しません。");
        //    }
        //}

    }
}