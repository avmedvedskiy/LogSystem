using System;
using UnityEngine;

namespace LogSystem
{

    // Summary:
    //     ///
    //     The type of the log message in Debug.logger.Log or delegate registered with Application.RegisterLogCallback.
    //     ///
    [Flags]
    public enum LogType
    {
        // Summary:
        //     ///
        //     LogType used for Errors.
        //     ///
        Error = 0 << 1,
        //
        // Summary:
        //     ///
        //     LogType used for Asserts. (These could also indicate an error inside Unity itself.)
        //     ///
        Assert = 1 << 1,
        //
        // Summary:
        //     ///
        //     LogType used for Warnings.
        //     ///
        Warning = 2 << 1,
        //
        // Summary:
        //     ///
        //     LogType used for regular log messages.
        //     ///
        Log = 3 << 1,
        //
        // Summary:
        //     ///
        //     LogType used for Exceptions.
        //     ///
        Exception = 4 << 1,

    }
    [CreateAssetMenu(menuName = "Scriptable Objects/LogSystem/LogSettings", order = -1000)]
    public class LogSettings : ScriptableObject
    {
        /// <summary>
        /// Записывать ли логи в файл
        /// </summary>
        public bool enable = true;

        /// <summary>
        /// Количество сессий которые нужно сохранить
        /// </summary>
        ///
        [Range(1,5)]
        public int sessionsCount = 5;
        
        public LogType supportLogTypes;

    }
}
