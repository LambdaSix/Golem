namespace Golem.Server.Text
{
    public interface IOutputTextWriter
    {
        void Write(string value);
        void Write(object value);
        void Write(string format, params object[] args);

        void WriteLine(string value);
        void WriteLine(object value);
        void WriteLine(string format, params object[] args);
    }
}