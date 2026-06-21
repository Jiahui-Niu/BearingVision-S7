using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    /// <summary>
    /// 仿Javascript Fetch
    /// </summary>
    public static class Fetch
    {
        public static int timeOut { get; set; }

        static Fetch ()
	    {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
	    }

        /// <summary>
        /// 异步请求
        /// </summary>
        /// <param name="url">地址 (http:// | https://)</param>
        /// <param name="method">GET | POST</param>
        /// <param name="header">header 头</param>
        /// <param name="body">json需要自行序列化</param>
        /// <param name="user_Agent">User-Agent</param>
        /// <param name="timeOut">请求超时</param>
        /// <returns>获得一个AwaitableResponse task</returns>
        public static AwaitableResponse Request(string url
            , string method
            , Dictionary<string, object> header = null
            , object body = null
            , string user_Agent = null
            , int timeOut = 2000)
        {
            var task = Task.Factory.StartNew<FetchedResponse>(() =>
            {
                try
                {
                    var request = WebRequest.Create(url) as HttpWebRequest;
                    request.ReadWriteTimeout = timeOut;
                    request.Timeout = timeOut;
                    request.Method = method.ToUpper();
                    request.UserAgent = user_Agent != null ? user_Agent : request.UserAgent;
                    
                    if (header != null)
                    {
                        foreach (var i in header)
                        {
                            SetHeaderValue(request.Headers, i.Key, i.Value.ToString());
                        }
                    }

                    if (body != null)
                    {
                        var connectTask = request.GetRequestStreamAsync();
                        if (!connectTask.Wait(timeOut))
                        {
                            LogHelper.Log.Error("Http connect timeout.");
                            return new FetchedResponse();
                        }

                        var bytes = Encoding.UTF8.GetBytes(body.ToString());
                        using (var stream = connectTask.Result)
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }

                    var response = request.GetResponse() as HttpWebResponse;
                    return new FetchedResponse(response);
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error("Http post json error", ex);
                    return new FetchedResponse();
                }
            });

            AwaitableResponse res = new AwaitableResponse(task);
            return res;
        }

        // 设置请求头
        private static void SetHeaderValue(WebHeaderCollection headerCollection, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collecion = property.GetValue(headerCollection, null) as System.Collections.Specialized.NameValueCollection;
                collecion[name] = value;
            }
        }

        // 测试例子
        private static async Task Test()
        {
            await Fetch.Request(
                url: "http://baidu.com",
                method: "POST",
                body: JsonHelper.SerializeObject(new TestCls() { TestA = "123" }),
                header: new Dictionary<string, object>
                {
                    {"Content-Type", "application/json"}
                },
                user_Agent: "Windows",
                timeOut: 1000
            ).Then(s => 
			{
                if (s.StatusCode == HttpStatusCode.OK)
                {
                    //var ret = s.Text();   直接获取返回值string, Json 和 Text只能调用其中之一, 不然会导致异常
                    var ret = s.Json();
                    LogHelper.Log.DebugFormat("response text: {0}", ret.ToString());
                    return ret;
                }
                else
                {
                    return null;
                }
            }).Then(s =>
                s.Entity<TestCls>()
            ).DoneAsync();
        }

        // 测试例子2
        private static void Test2()
        {
            Fetch.Request(
                url: "http://baidu.com",
                method: "POST",
                body: JsonHelper.SerializeObject(new TestCls() { TestA = "123" }),
                header: new Dictionary<string, object>
                {
                    {"Content-Type", "application/json"}
                },
                user_Agent: "Windows",
                timeOut: 1000
            ).Then(s =>
            {
                if (s.StatusCode == HttpStatusCode.OK)
                {
                    //var ret = s.Text();   直接获取返回值string, Json 和 Text只能调用其中之一, 不然会导致异常
                    var ret = s.Json();
                    LogHelper.Log.DebugFormat("response text: {0}", ret.ToString());
                    return ret;
                }
                else
                {
                    return null;
                }
            }).Then(s =>
                s.Entity<TestCls>()
            ).Done();
        }

        private class TestCls
        {
            public string TestA { get; set; }
        }
    }

    public class AwaitableResponse : AwaitableResult<FetchedResponse>
    {
        public AwaitableResponse(Task<FetchedResponse> _task)
            : base(_task)
        {

        }
    }

    /// <summary>
    /// 包装Task 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AwaitableResult<T>
    {
        public Task<T> task { get; set; }

        public AwaitableResult(Task<T> _task)
        {
            task = _task;
        }

        /// <summary>
        /// 下一步, 异步, 装饰
        /// </summary>
        /// <typeparam name="outEntity"></typeparam>
        /// <param name="predict"></param>
        /// <returns></returns>
        public AwaitableResult<outEntity> Then<outEntity>(Func<T, outEntity> predict)
        {
            var task2 = Task.Factory.StartNew<outEntity>(() =>
            {
                task.Wait();
                if (task.Result != null)
                {
                    return predict(task.Result);
                }
                else
                {
                    LogHelper.Log.WarnFormat("[Then] The result of the task is null, skipped predicting.");
                    return default(outEntity); 
                }
            });
            return new AwaitableResult<outEntity>(task2);
        }

        /// <summary>
        /// 异步时调用获得结果
        /// </summary>
        /// <typeparam name="outEntity"></typeparam>
        /// <param name="predict"></param>
        /// <returns></returns>
        public async Task<outEntity> DoneAsync<outEntity>(Func<T, outEntity> predict)
        {
            var result = await task;
            if (result != null)
            {
                return predict(result);
            }
            else
            {
                LogHelper.Log.WarnFormat("[DoneAsync] The result of the task is null, skipped predicting.");
                return default(outEntity);
            }
        }

        /// <summary>
        /// 异步时调用获得结果
        /// </summary>
        /// <returns></returns>
        public async Task<T> DoneAsync()
        {
            return await task;
        }

        /// <summary>
        /// 非异步时调用获得结果
        /// </summary>
        /// <typeparam name="outEntity"></typeparam>
        /// <param name="predict"></param>
        /// <returns></returns>
        public outEntity Done<outEntity>(Func<T, outEntity> predict)
        {
            task.Wait();
            if (task.Result != null)
            {
                return predict(task.Result);
            }
            else
            {
                LogHelper.Log.WarnFormat("[Done] The result of the task is null, skipped predicting.");
                return default(outEntity);
            }
        }

        /// <summary>
        /// 非异步时调用获得结果
        /// </summary>
        /// <returns></returns>
        public T Done()
        {
            task.Wait();
            return task.Result;
        }
    }

    /// <summary>
    /// 封装的HttpWebResponse
    /// </summary>
    public class FetchedResponse
    {
        public HttpWebResponse Response { get; private set; }

        public HttpStatusCode StatusCode
        {
            get 
            {
                return Response != null ? Response.StatusCode : HttpStatusCode.NotFound;
            }
        }

        public FetchedResponse(HttpWebResponse response = null)
        {
            Response = response;
        }

        /// <summary>
        /// 获取FetchedJson 
        /// </summary>
        /// <returns></returns>
        public FetchedJson Json()
        {
            return new FetchedJson(GetResult());
        }

        /// <summary>
        /// 直接获取文本
        /// </summary>
        /// <returns></returns>
        public string Text()
        {
            return GetResult();
        }

        private string GetResult()
        {
            if (Response == null)
            {
                return null;
            }
            else
            {
                try
                {
                    var stream = Response.GetResponseStream();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        return sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// 封装的Json字符串
    /// </summary>
    public class FetchedJson
    {
        private string Json { get; set; }

        public FetchedJson(string json = null)
        {
            Json = json;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Entity<T>()
        {
            T t = default(T);
            if (Json != null)
            {
                try
                {
                    t = JsonHelper.DeSerialize<T>(Json);
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
            }
            return t;
        }

        public override string ToString()
        {
            return Json;
        }
    }
}
