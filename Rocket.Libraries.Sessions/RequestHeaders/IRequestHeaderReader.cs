namespace Rocket.Libraries.Sessions.RequestHeaders
{
    public interface IRequestHeaderReader
    {
        string Read(string key);

        string ReadOrDefault(string key);
    }
}