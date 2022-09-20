using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderIconCreator
{
    public class FolderIconCreatorSetting
    {
        /// <summary>
        /// 既定の前画像を使うならtrue
        /// </summary>
        public bool DefaultForegroundPicture { get; set; } = true;

        /// <summary>
        /// 前画像ファイルのパス
        /// </summary>
        public string ForegroundPicturePath { get; set; }

        /// <summary>
        /// 背景画像ファイルのパス
        /// </summary>
        public string BackgroundPicturePath { get; set; }
    }
}