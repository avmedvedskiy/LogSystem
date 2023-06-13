using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LogSystem
{
    public class LogSenderService : ILogSenderService
    {
        private readonly LogService _logService;
        private readonly ILogRequestFactory _requestFactory;
        private bool _inProgress;

        public LogSenderService(LogService logService, ILogRequestFactory requestFactory)
        {
            _logService = logService;
            _requestFactory = requestFactory;
        }

        public async Task SendAsync()
        {
            foreach (var filePath in _logService.GetFiles())
            {
                var fileName = Path.GetFileName(filePath);
                var bytes = await File.ReadAllBytesAsync(filePath);
                if (bytes.Length > 0)
                {
                    using var request = _requestFactory.CreateRequest(bytes, fileName);
                    Debug.Log($"Start send log {fileName} {request.url}");
                    await request.SendWebRequest();
                    Debug.Log($"Send done {fileName} {request.result} {request.error}");
                }
            }
        }
    }
}