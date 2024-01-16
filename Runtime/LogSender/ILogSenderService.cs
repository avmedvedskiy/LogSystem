using System.Threading.Tasks;

namespace LogSystem
{
    public interface ILogSenderService
    {
        bool InProgress { get;}
        Task SendAsync();
    }
}