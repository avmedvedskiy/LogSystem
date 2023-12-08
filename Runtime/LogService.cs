using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LogSystem
{
    public class LogService : MonoBehaviour
    {
        const string LOG_SETTINGS_FILE_NAME = "LogSettings";
        const string FOLDER_PATH = "{0}/Logs";
        const string PATH_FORMAT = "{0}/Logs/log{1}.txt";
        const string LOG_FORMAT = "{3}\t{0} {1}\n{2}";
        //const string SERVER_URL_FORMAT = "{0}v2/send/log/{1}_{2}";
        const int CURRENT_SESSION_NUMBER = 0;

        private LogSettings LogSettings { get; set; }
        private StreamWriter StreamWriter { get; set; }

        void Awake()
        {
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

        public IEnumerable<string> GetFiles()
        {
            CloseStream();
            string folderPath = string.Format(FOLDER_PATH, Application.persistentDataPath);
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                yield return filePath;
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
			
            StreamWriter?.Flush();
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
            /*
            int unityLogType = (1 << (int)type);
            int aType = (int)LogSettings.supportLogTypes;
            if ((aType & unityLogType) != unityLogType)
                return;
            */

            if (StreamWriter != null)
                StreamWriter.WriteLine(LOG_FORMAT, type, logString, stackTrace, Time.realtimeSinceStartup);
        }
    }
}
