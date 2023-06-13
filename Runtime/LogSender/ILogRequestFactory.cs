using UnityEngine.Networking;

namespace LogSystem
{
    public interface ILogRequestFactory
    {
        UnityWebRequest CreateRequest(byte[] bytes, string fileName);
    }
}