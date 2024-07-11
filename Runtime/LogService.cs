using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LogSystem
{
    public class LogService : MonoBehaviour
    {
        const string SEPARATOR = "\t";
        const string NEW_LINE = "\r\n";
        const string LOG_SETTINGS_FILE_NAME = "LogSettings";
        const string FOLDER_PATH = "{0}/Logs";
        const string PATH_FORMAT = "{0}/Logs/log{1}.txt";
        const int CURRENT_SESSION_NUMBER = 0;

        [Range(1, 5)] [SerializeField] private int _sessionsCount = 3;
        private StringBuilder _builder;
        private StreamWriter StreamWriter { get; set; }

        void Awake()
        {
#if UNITY_WEBGL
            this.enabled = false;
            return;
#endif
            DontDestroyOnLoad(gameObject);

            _builder = new StringBuilder();

            string folderPath = string.Format(FOLDER_PATH, Application.persistentDataPath);
            Directory.CreateDirectory(folderPath);

            for (int i = _sessionsCount - 1; i >= 0; i--)
            {
                string path = string.Format(PATH_FORMAT, Application.persistentDataPath, i);
                if (File.Exists(path))
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
            if (enabled)
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
            StreamWriter =
                new StreamWriter(string.Format(PATH_FORMAT, Application.persistentDataPath, CURRENT_SESSION_NUMBER),
                    true);
        }

        void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            if (StreamWriter == null)
                return;

            _builder
                .Append(Time.realtimeSinceStartup)
                .Append(SEPARATOR)
                .Append(type)
                .Append(logString)
                .Append(NEW_LINE)
                .Append(stackTrace);

            StreamWriter.WriteLine(_builder.ToString());
            _builder.Clear();
        }
    }
}