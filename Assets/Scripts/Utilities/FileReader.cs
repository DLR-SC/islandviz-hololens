using HoloIslandVis.Core;

#if NETFX_CORE
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
#else
using System.IO;
using System.Text;
#endif

namespace HoloIslandVis.Utilities
{
    public class FileReader : Singleton<FileReader>
    {
        public static string Read(string filepath)
        {
            return ReadAsString(filepath);
        }

#if NETFX_CORE
        private static string ReadAsString(string filepath)
        {
            string filedata = "";
            filedata = File.ReadAllText(filepath.Replace("/", "\\"));
            return filedata;
        }
#else
        private static string ReadAsString(string filepath)
        {
            string filedata = "";

            using (FileStream fileStream = File.Open(filepath, FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                filedata = Encoding.ASCII.GetString(buffer);
                fileStream.Close();
            }

            return filedata;
        }
#endif
    }
}