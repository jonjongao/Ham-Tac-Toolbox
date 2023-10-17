using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace HamTac
{
    public class GameSaveModel
    {
        static string folderName;
        static string extensionName;
        const string defaultFolderName = "Saves";
        const string defaultExtensionName = "sav";

        public static void Initialize()
        {
            Initialize(defaultFolderName, defaultExtensionName);
        }
        public static void Initialize(string folderName, string extensionName)
        {
            GameSaveModel.folderName = folderName;
            GameSaveModel.extensionName = extensionName;
            ES3.CacheFile();
        }

        public static bool Save<T1, T2>(string fileName, T1 header, T2 profile)
            where T1 : ISaveFileHeader
            where T2 : IProfileWrapper
        {
            try
            {
                var path = FormFilePath(fileName);
                var meta = new Meta<T1>(fileName, path, header);
                var bytes = profile.SerializeThenWrapToBytes();
                Save(fileName, meta, bytes);
                return true;
            }
            catch (System.Exception error)
            {
                Debug.LogError(error);
            }
            return false;
        }

        public static void Save(string fileName, object meta, object bytes)
        {
            var path = FormFilePath(fileName);
            ES3.Save("meta", meta, path);
            ES3.Save("save", bytes, path);
        }

        public static bool Load<T>(ISaveFileMeta meta, out T result)
        {
            return Load<T>(meta.fileName, out result);
        }
        public static bool Load<T>(string fileName, out T result)
        {
            try
            {
                var path = FormFilePath(fileName);
                var bytes = ES3.Load<byte[]>("save", path);
                result = ES3.Deserialize<T>(bytes);
                return true;
            }
            catch (System.Exception error)
            {
                Debug.LogError(error);
            }
            result = default(T);
            return false;
        }

        public static bool Delete(string fileNameInclExtension)
        {
            var path = $"{folderName}/{fileNameInclExtension}";
            if (FileExist(fileNameInclExtension) == false)
            {
                Debug.LogError($"Cant find file:{fileNameInclExtension}");
                return true;
            }
            try
            {
                ES3.DeleteFile(path);
                return true;
            }
            catch (System.Exception error)
            {
                Debug.LogError(error);
            }
            return false;
        }

        public static bool FileExist(string fileNameInclExtension)
        {
            var path = $"{folderName}/{fileNameInclExtension}";
            return ES3.FileExists(path);
        }

        /// <summary>
        /// 將ES3.GetFile的名稱帶入，包含副檔名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Meta<T> LoadMeta<T>(string fileNameInclExtension)
        {
            try
            {
                return ES3.Load<Meta<T>>("meta", $"Saves/{fileNameInclExtension}");
            }
            catch (System.Exception error)
            {
                Debug.LogWarning(error);
            }
            return null;
        }

        public static bool TryLoadMeta<T>(string fileNameInclExtension, out Meta<T> result)
        {
            try
            {
                var meta = ES3.Load<Meta<T>>("meta", $"{folderName}/{fileNameInclExtension}");
                result = meta;
                return true;
            }
            catch (System.Exception error)
            {
                Debug.LogWarning(error);
            }
            result = null;
            return false;
        }

        public static async Task<Meta<T>[]> GetMetaInFixedSizeAsync<T>(int length, bool includeAutoSave, int autoSaveLength,
            Func<string, Task<bool>> onCorruptSaveFile,
            Func<T, bool> onAutoSaveMeta = null)
        {
            if(ES3.DirectoryExists($"Saves")==false)
            {
                System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Saves");
            }
            var fileNames = ES3.GetFiles($"Saves/");
            var arr = fileNames.Select(x => LoadMeta<T>(x)).Where(x => x != null && x.IsValid()).
                                ToDictionary(x => x.fileName, x => x);

            var expect = new string[0];
            if (includeAutoSave)
            {
                //todo讀檔模式: 11格
                expect = GenerateFileNames(0, autoSaveLength, "autosave_").Concat(GenerateFileNames(0, length)).ToArray();
            }
            else
            {
                //todo存檔模式: 10格
                expect = GenerateFileNames(0, length);
                //autoSaveLength = 0;
            }
            var meta = new Meta<T>[expect.Length];
            for (int i = 0; i < expect.Length; i++)
            {
                var name = expect[i];
                //var match = arr.FirstOrDefault(x => x.fileName.Equals(name));
                if (arr.ContainsKey(name))
                {
                    if (includeAutoSave && i < autoSaveLength)
                    {
                        if(onAutoSaveMeta != null)
                            onAutoSaveMeta?.Invoke(arr[name].header);
                    }
                    meta[i] = arr[name];
                    //Debug.Log($"Displaying save file:{name}");
                }
                else
                {
                    if (FileExist($"{name}.{extensionName}"))
                    {
                        await onCorruptSaveFile(name);
                        Delete($"{name}.{extensionName}");
                    }
                    meta[i] = null;
                }
            }
            return meta;
        }

        static string FormFilePath(string fileName)
        {
            //Save/00.save
            return $"{folderName}/{fileName}.{extensionName}";
        }

        static string[] GenerateFileNames(int startNumber, int endNumber, string prepend = "", string append = "")
        {
            return Enumerable.Range(startNumber, endNumber - startNumber)
                             .Select(i => prepend + i.ToString("00") + append)
                             .ToArray();
        }

        public class Meta<T> : ISaveFileMeta
        {
            public string version;
            /// <summary>
            /// 排除副檔名的檔案名稱
            /// </summary>
            public string fileName;
            public string filePath;
            public long unixTimestamp;
            public T header;
            string ISaveFileMeta.fileName => fileName;
            string ISaveFileMeta.filePath => filePath;

            public static string GetVersion() { return "2.40"; }

            public static bool SaveVersionIfDifferent(Meta<T> value)
            {
                return value.version.Equals(Meta<T>.GetVersion()) == false;
            }

            public bool IsValid() { return string.IsNullOrEmpty(fileName) == false; }
            public Meta() { }

            public Meta(string fileName, string filePath, T header)
            {
                this.version = GetVersion();
                DateTime currentTime = DateTime.UtcNow;
                this.unixTimestamp = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                this.filePath = filePath;
                this.fileName = fileName;
                this.header = header;
            }
        }
    }

   

    


 


  
}