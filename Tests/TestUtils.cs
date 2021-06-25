using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using powerplant.Controllers;

namespace powerplant.Tests
{
    public class TestLogger : ILogger<ProductionPlanController>
    {
        public System.IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception exception, System.Func<TState, System.Exception, string> formatter)
        {
            Console.WriteLine($"LOG]{logLevel}\t{exception.Message}");
        }
    }

    public static class TestUtils
    {
        public static string GetTextFromUrl(string url)
        {
            var webRequest = WebRequest.Create(url);

            using var response = webRequest.GetResponse();
            using var content = response.GetResponseStream();
        
            if (content == null) return "";
        
            using var reader = new StreamReader(content);
            var strContent = reader.ReadToEnd();
            return strContent;

        }
    }
}