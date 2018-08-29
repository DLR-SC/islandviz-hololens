#if NETFX_CORE
using System;
using System.Threading.Tasks;
using Windows.Storage;
#else
using System.IO;
using System.Text;
#endif

namespace HoloIslandVis.Utility
{
    public class ModelDataReader
    {
        private static ModelDataReader _instance;

        public static ModelDataReader Instance {
            get {
                if (_instance == null)
                    _instance = new ModelDataReader();

                return _instance;
            }

            private set { }
        }

        private ModelDataReader() { }

        public JSONObject Read(string filepath)
        {
#if NETFX_CORE
            string data = ReadAsString(filepath).Result;
#else
            string data = ReadAsString(filepath);
#endif
            return JSONObject.Create(data);
        }

#if NETFX_CORE
        private async Task<string> ReadAsString(string filepath)
        {
            string filedata = "";
            var storageFile = await StorageFile.GetFileFromPathAsync(filepath.Replace("/", "\\"));
            filedata = await FileIO.ReadTextAsync(storageFile);
            return filedata;
        }
#else
        private string ReadAsString(string filepath)
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
