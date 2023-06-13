using System.Threading.Tasks;

namespace LogSystem
{
    public interface ILogSenderService
    {
        Task SendAsync();
    }
}