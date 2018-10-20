using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartHouse
{
    public class SmartHomeSettings
    {
        public static async void Save(Byte[] data)
        {
            string filename = "settings.txt";

#if !WINDOWS_UWP
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, filename);
            System.IO.File.WriteAllBytes(filePath, data);
#else
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await localFolder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteBytesAsync(sampleFile, data);
#endif
        }
        public static Byte[] Load()
        {


            string filename = "settings.txt";
#if !WINDOWS_UWP
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, filename);
            return System.IO.File.ReadAllBytes(filePath);
#else
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = storageFolder.GetFileAsync(filename).GetAwaiter<Windows.Storage.StorageFile>().GetResult();
            byte[] fileBytes = null;
            using (Windows.Storage.Streams.IRandomAccessStreamWithContentType stream = sampleFile.OpenReadAsync().GetAwaiter<Windows.Storage.Streams.IRandomAccessStreamWithContentType>().GetResult())
            {
                fileBytes = new byte[stream.Size];
                using (Windows.Storage.Streams.DataReader reader = new Windows.Storage.Streams.DataReader(stream))
                {
                    reader.LoadAsync((uint)stream.Size).GetAwaiter().GetResult();
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
#endif
        }

        public static bool Exist()
        {


            string filename = "settings.txt";
#if !WINDOWS_UWP
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, filename);
            return System.IO.File.Exists(filePath);
#else
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = storageFolder.GetFileAsync(filename).GetAwaiter<Windows.Storage.StorageFile>().GetResult();

            var files = storageFolder.GetFilesAsync().GetAwaiter<IReadOnlyList<Windows.Storage.StorageFile>>().GetResult();
            foreach (Windows.Storage.StorageFile file in files)
            {
                if (file.Name == filename) return true;
            }
            return false;
#endif
        }
    }
}
