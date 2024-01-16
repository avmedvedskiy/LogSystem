using Cysharp.Threading.Tasks;

namespace LogSystem
{
    public interface ILogSenderService
    {
        UniTask SendAsync();
    }
}