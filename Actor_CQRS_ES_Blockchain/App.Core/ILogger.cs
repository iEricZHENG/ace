using System;
using System.Threading.Tasks;

namespace App.Core
{
    public interface ILogger
    {
        Task Info(Exception e);
        Task Info<T>(Exception e, T data);
        Task Info(string message, Exception e);
        Task Info<T>(string message, Exception e, T data);
        Task Info(string message);
        Task Info<T>(string message, T data);

        Task Debug(Exception e);
        Task Debug<T>(Exception e, T data);
        Task Debug(string message, Exception e);
        Task Debug<T>(string message, Exception e, T data);
        Task Debug(string message);
        Task Debug<T>(string message, T data);

        Task Warning(Exception e);
        Task Warning<T>(Exception e, T data);
        Task Warning(string message, Exception e);
        Task Warning<T>(string message, Exception e, T data);
        Task Warning(string message);
        Task Warning<T>(string message, T data);

        Task Trace(Exception e);
        Task Trace<T>(Exception e, T data);
        Task Trace(string message, Exception e);
        Task Trace<T>(string message, Exception e, T data);
        Task Trace(string message);
        Task Trace<T>(string message, T data);

        Task Error(Exception e);
        Task Error<T>(Exception e, T data);
        Task Error(string message, Exception e);
        Task Error<T>(string message, Exception e, T data);
        Task Error(string message);
        Task Error<T>(string message, T data);

        Task Fatal(Exception e);
        Task Fatal<T>(Exception e, T data);
        Task Fatal(string message, Exception e);
        Task Fatal<T>(string message, Exception e, T data);
        Task Fatal(string message);
        Task Fatal<T>(string message, T data);
    }
}
