using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LogSystem
{
    public interface IScreenshotSenderService
    {
        UniTask SaveAndSendAsync();
    }

    public class ScreenshotSenderService : IScreenshotSenderService
    {
        private readonly ILogRequestFactory _requestFactory;
        private static string Path =>
#if UNITY_EDITOR
            $"{Application.dataPath}/../{FILE_NAME}";
#else
            $"{Application.persistentDataPath}/{FILE_NAME}";
#endif
        private const string FILE_NAME = "SupportScreen.png";

        private bool _inProgress;

        public ScreenshotSenderService(ILogRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
        }

        public async UniTask SaveAndSendAsync()
        {
            if(_inProgress)
                return;
            
            try
            {
                _inProgress = true;
                File.Delete(Path);
                ScreenCapture.CaptureScreenshot(FILE_NAME);
                await WaitFileExists()
                    .Timeout(TimeSpan.FromSeconds(20));
                await SendToServer();
                _inProgress = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async UniTask SendToServer()
        {
            var bytes = await File.ReadAllBytesAsync(Path);
            if (bytes.Length > 0)
            {
                using var request = _requestFactory.CreateRequest(bytes, FILE_NAME);
                Debug.Log($"Start send screen {request.url}");
                try
                {
                    await request.SendWebRequest().ToUniTask();
                    Debug.Log($"Send screen done");
                    File.Delete(Path);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private async UniTask WaitFileExists()
        {
            while (!File.Exists(Path))
                await UniTask.Delay(500, cancellationToken: Application.exitCancellationToken);
        }
    }
}