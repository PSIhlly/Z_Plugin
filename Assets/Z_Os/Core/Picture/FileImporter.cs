
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Z_Os.DllImporter;
using System.Runtime.InteropServices;
using System;

namespace Z_Os.File
{
    public class FileImporter
    {
        public static void ImportImageBytes(Action<byte[]> onComplete)
        {
#if UNITY_STANDALONE_WIN

            FileOpenDialog dialog = new FileOpenDialog();

            dialog.structSize = Marshal.SizeOf(dialog);

            dialog.filter = "PNG files\0*.png\0JPG Files\0*.jpg\0GIF Files\0*.gif\0\0";

            dialog.file = new string(new char[256]);

            dialog.maxFile = dialog.file.Length;

            dialog.fileTitle = new string(new char[64]);

            dialog.maxFileTitle = dialog.fileTitle.Length;

            dialog.initialDir = UnityEngine.Application.dataPath;  //默认路径

            dialog.title = "Import image file";

            dialog.defExt = "image";
                                  //注意一下项目不一定要全选 但是0x00000008项不要缺少
            dialog.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;  //OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR


            if (DialogShow.GetOpenFileName(dialog))
            {
                onComplete?.Invoke(System.IO.File.ReadAllBytes(dialog.file));
            }
            else onComplete?.Invoke(null);
#endif
#if UNITY_ANDROID
            FileOpenDialog dialog = new FileOpenDialog();
            dialog.type = "image";
            DialogShow.GetOpenFileName(dialog, onComplete);
#endif


        }


    }
}
