using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LogSystem
{
    public class LogManager : MonoBehaviour
    {
        public enum RequestType
        {
            Post,Put
        }

        const string LOG_SETTINGS_FILE_NAME = "LogSettings";
        const string FOLDER_PATH = "{0}/Logs";
        const string PATH_FORMAT = "{0}/Logs/log{1}.txt";
        const string LOG_FORMAT = "{3}\t{0} {1}\n{2}";
        //const string SERVER_URL_FORMAT = "{0}v2/send/log/{1}_{2}";
        const int CURRENT_SESSION_NUMBER = 0;

        public static LogManager Instance { get; private set; }

        protected LogSettings LogSettings { get; set; }
        protected StreamWriter StreamWriter { get; set; }

        void Awake()
        {
            Instance = this;
#if UNITY_WEBGL
            this.enabled = false;
            return;
#endif

            LogSettings = Resources.Load(LOG_SETTINGS_FILE_NAME) as LogSettings;
            if (LogSettings == null)
                LogSettings = ScriptableObject.CreateInstance<LogSettings>();

            enabled = LogSettings.enable;
            int sessionsCount = LogSettings.sessionsCount - 1;

            string folderPath = string.Format(FOLDER_PATH, Application.persistentDataPath);
            Directory.CreateDirectory(folderPath);

            for (int i = sessionsCount; i >= 0; i--)
            {
                string path = string.Format(PATH_FORMAT, Application.persistentDataPath, i);
                if(File.Exists(path))
                {
                    string newPath = string.Format(PATH_FORMAT, Application.persistentDataPath, i + 1);
                    File.Delete(newPath);
                    File.Move(path, newPath);
                }
            }

            OpenStream();
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
		}

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            CloseStream();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!this.enabled)
                return;
			
            StreamWriter.Flush();
        }

        void CloseStream()
        {
            if (StreamWriter != null)
            {
                StreamWriter.Close();
                StreamWriter.Dispose();
                StreamWriter = null;
            }
        }

        void OpenStream()
        {
            StreamWriter = new StreamWriter(string.Format(PATH_FORMAT, Application.persistentDataPath, CURRENT_SESSION_NUMBER), true);
        }

        void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            int unityLogType = (1 << (int)type);
            int aType = (int)LogSettings.supportLogTypes;
            if ((aType & unityLogType) != unityLogType)
                return;

            if (StreamWriter != null)
                StreamWriter.WriteLine(LOG_FORMAT, type, logString, stackTrace, Time.realtimeSinceStartup);
        }

        //TODO Move to IUpload class
        public void UploadToServer(string serverURL, string serverFormat, string userid, RequestType type, Action < List<string>> callback = null)
        {
            if (!enabled)
                return;

            CloseStream();
            StartCoroutine(Upload(serverURL, serverFormat, userid, type,callback));
        }

        private IEnumerator Upload(string serverURL,string serverFormat, string userid, RequestType type, Action<List<string>> callback)
        {
            List<string> logUrls = new List<string>();
            string folderPath = string.Format(FOLDER_PATH, Application.persistentDataPath);

            foreach (string file in Directory.GetFiles(folderPath))
            {
                var fileName = Path.GetFileName(file);
                var url = string.Format(serverFormat, serverURL, userid, fileName);
                var bytes = File.ReadAllBytes(file);
                if (bytes.Length > 0)
                {
                    UnityWebRequest upload = null;
                    switch(type)
                    {
                        case RequestType.Put:
                            upload = UnityWebRequest.Put(url, bytes);
                            break;
                        case RequestType.Post:
                            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                            formData.Add(new MultipartFormFileSection("uploadedFile", bytes, fileName, ""));
                            upload = UnityWebRequest.Post(url, formData);
                            break;
                    }

                    Debug.LogFormat("Start loading logs on server {0}", fileName);
                    yield return upload.SendWebRequest();
                    if (upload.result != UnityWebRequest.Result.Success)
                    {
                        logUrls.Add(upload.downloadHandler.text);
                        Debug.LogFormat("End loading logs on server \n {0}", upload.downloadHandler.text);
                    }
                }
            }

            callback?.Invoke(logUrls);
            Debug.LogFormat("All logs loaded on server");
            OpenStream();
        }

    }
}
