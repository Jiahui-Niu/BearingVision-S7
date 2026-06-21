using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class HttpClientFactory : IDisposable
    {
        private static HttpClientFactory _instance = new HttpClientFactory();

        private readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>();

        public static HttpClientFactory Instance
        {
            get
            {
                return _instance;
            }
        }

        private HttpClientFactory()
        {

        }

        /// <summary>
        /// 创建HttpClient
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// name is null
        /// </exception>
        /// <param name="name"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public HttpClient CreateClient(string name, TimeSpan timeOut = default(TimeSpan))
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(name.GetType().Name);
            }

            if (timeOut == TimeSpan.Zero)
            {
                timeOut = TimeSpan.FromSeconds(100);
            }

            return _clients.AddOrUpdate(name, (n) => new HttpClient() { Timeout = timeOut }, (u, h) => {

                FieldInfo fieldInfo = h.GetType().GetField("disposed"
                , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var val = (bool)fieldInfo.GetValue(h);
                    if (val)
                    {
                        return new HttpClient() { Timeout = timeOut };
                    }
                }

                return h;
            });
        }

        public void Dispose()
        {
            foreach (var item in _clients)
            {
                if (item.Value != null)
                {
                    item.Value.Dispose();
                }
            }

            _clients.Clear();
        }
    }
}
