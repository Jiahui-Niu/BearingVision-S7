using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

using System.IO;

namespace ICPlatformTools
{
    public class HttpUtil
    {
        public string m_url;

        public string Url
        {
            get { return m_url; }
            set
            {
                try
                {
                    m_url = new System.UriBuilder(value).ToString();
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error("Error http url", ex);
                }
            }
        }
        
        public int Timeout { get; set; }

        public HttpUtil()
        {
            this.Timeout = 1000;
        }

        public bool PostJson(string data, out string responseContent)
        {
            responseContent = null;

            try
            {
                var request = WebRequest.Create(this.Url) as HttpWebRequest;
                request.ReadWriteTimeout = this.Timeout;
                request.Timeout = this.Timeout;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = Encoding.UTF8.GetByteCount(data);
                
                System.Net.ServicePointManager.DefaultConnectionLimit = 100;

                var connectTask = request.GetRequestStreamAsync();
                var tasks = new Task[] { Task.Delay(this.Timeout),  connectTask };
                if (Task.WaitAny(tasks) == 0)
                {
                    LogHelper.Log.Error("Http connect timeout.");
                    return false;
                }

                using (var streamWriter = new StreamWriter(connectTask.Result))
                {
                    streamWriter.Write(data);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        LogHelper.Log.ErrorFormat("Http post json error, statue code: {0}", response.StatusCode);
                        return false;
                    }

                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = streamReader.ReadToEnd();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("Http post json error", ex);
                return false;
            }
        }
    }
}
