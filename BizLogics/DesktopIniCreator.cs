using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FolderIconCreator.BizLogics
{
    /// <summary>
    /// Desktop.iniを生成するためのクラスです。
    /// </summary>
    public class DesktopIniCreator
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        private const UInt32 _FCS_READ = 0x00000001;
        private const UInt32 _FCS_FORCEWRITE = 0x00000002;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize;
            public UInt32 dwMask;
            public IntPtr pvid;
            public string pszWebViewTemplate;
            public UInt32 cchWebViewTemplate;
            public string pszWebViewTemplateVersion;
            public string pszInfoTip;
            public UInt32 cchInfoTip;
            public IntPtr pclsid;
            public UInt32 dwFlags;
            public string pszIconFile;
            public UInt32 cchIconFile;
            public int iIconIndex;
            public string pszLogo;
            public UInt32 cchLogo;
        }

        private enum FOLDERCUSTOMSETTINGSMASK : UInt32

        {
            FCSM_VIEWID = 0x0001,

            FCSM_WEBVIEWTEMPLATE = 0x0002,

            FCSM_INFOTIP = 0x0004,

            FCSM_CLSID = 0x0008,

            FCSM_ICONFILE = 0x0010,

            FCSM_LOGO = 0x0020,

            FCSM_FLAGS = 0x0040,
        }

        /// <summary>
        /// 指定されたフォルダにdesktop.iniを生成します。
        /// </summary>
        /// <param name="folderpath"></param>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public static bool Create(string folderpath, string iconName)
        {
            var filepath = System.IO.Path.Combine(folderpath, "desktop.ini");
            var filename = System.IO.Path.GetFileName(filepath);

            //if (System.IO.File.Exists(filepath))
            //{
            //    try
            //    {
            //        //desktop.iniを削除
            //        System.IO.FileAttributes attrww = System.IO.File.GetAttributes(filepath);
            //        attrww &= ~System.IO.FileAttributes.System;
            //        attrww &= ~System.IO.FileAttributes.ReadOnly;
            //        attrww &= ~System.IO.FileAttributes.Hidden;
            //        System.IO.File.SetAttributes(filepath, attrww);
            //        System.IO.File.Delete(filepath);
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            //    }
            //}

            //desktop.iniを生成
            LPSHFOLDERCUSTOMSETTINGS fcs = new LPSHFOLDERCUSTOMSETTINGS();

            fcs.dwMask = (UInt32)FOLDERCUSTOMSETTINGSMASK.FCSM_ICONFILE;
            if (!string.IsNullOrWhiteSpace(iconName))
            {
                fcs.pszIconFile = iconName;
                fcs.iIconIndex = 0;
            }
            else
            {
                fcs.pszIconFile = null;
                fcs.cchIconFile = 0;
                fcs.iIconIndex = 0;
            }

            UInt32 fw = _FCS_READ | _FCS_FORCEWRITE;

            if (System.IO.File.Exists(filepath))
                fw = _FCS_FORCEWRITE;

            string pszPath = folderpath;
            UInt32 HRESULT = SHGetSetFolderCustomSettings(ref fcs, pszPath, fw);
            return (HRESULT == 0);
        }
    }
}