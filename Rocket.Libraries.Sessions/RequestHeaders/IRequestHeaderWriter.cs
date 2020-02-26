namespace Rocket.Libraries.Sessions.RequestHeaders
{
    public interface IRequestHeaderWriter
    {
        void Write(string key, object value);
    }
}