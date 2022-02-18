using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public static class Constants
    {
        /// <summary>
        /// エンコードID種類
        /// </summary>
        public struct EncodeIdType
        {
            public const string SJIS = "SJIS";
            public const string JIS = "JIS";
            public const string UTF8 = "UTF8";
            public const string UTF8N = "UTF8N";

        }
        /// <summary>
        /// ファイルエンコード種類
        /// </summary>
        public struct FileEncodeType
        {
            public const string SJIS = "Shift-JIS";
            public const string JIS = "JIS";
            public const string UTF8 = "UTF-8";
            public const string UTF8N = "UTF-8N";
        }
    }
}
