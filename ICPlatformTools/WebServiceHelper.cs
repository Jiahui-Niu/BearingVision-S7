using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace ICPlatformTools
{
    public class WebServiceHelper
    {
        /// <summary>
        /// 生成dll文件保存到本地
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="className">类名</param>
        /// <param name="filePath">保存dll文件的路径</param>
        public static bool CreateWebServiceDLL(string url, string className, string filePath = "./")
        {
            try
            {
                if (filePath == "./")
                {
                    filePath = System.Environment.CurrentDirectory;
                }
                if (filePath.Substring(filePath.Length - 1, 1) != @"\")
                {
                    filePath += @"\";
                }

                // 1. 使用 WebClient 下载 WSDL 信息。
                WebClient web = new WebClient();
                Stream stream = web.OpenRead(url);
                // 2. 创建和格式化 WSDL 文档。
                ServiceDescription description = ServiceDescription.Read(stream);
                //如果不存在就创建file文件夹
                if (Directory.Exists(filePath) == false)
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(filePath + className + ".dll"))
                {
                    //判断缓存是否过期
                    var cachevalue = HttpRuntime.Cache.Get(className + "Cate");
                    if (cachevalue == null)
                    {
                        //缓存过期删除dll
                        File.Delete(filePath + className + ".dll");
                    }
                    else
                    {
                        // 如果缓存没有过期直接返回
                        return true;
                    }
                }

                // 3. 创建客户端代理代理类。
                ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
                // 指定访问协议。
                importer.ProtocolName = "Soap";
                // 生成客户端代理。
                importer.Style = ServiceDescriptionImportStyle.Client;
                importer.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync;
                // 添加 WSDL 文档。
                importer.AddServiceDescription(description, null, null);
                // 4. 使用 CodeDom 编译客户端代理类。
                // 为代理类添加命名空间，缺省为全局空间。
                CodeNamespace nmspace = new CodeNamespace();
                CodeCompileUnit unit = new CodeCompileUnit();
                unit.Namespaces.Add(nmspace);
                ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit);
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CompilerParameters parameter = new CompilerParameters();
                parameter.GenerateExecutable = false;
                // 可以指定你所需的任何文件名。
                parameter.OutputAssembly = filePath + className + ".dll";
                parameter.ReferencedAssemblies.Add("System.dll");
                parameter.ReferencedAssemblies.Add("System.XML.dll");
                parameter.ReferencedAssemblies.Add("System.Web.Services.dll");
                parameter.ReferencedAssemblies.Add("System.Data.dll");
                // 生成dll文件，并会把WebService信息写入到dll里面
                CompilerResults result = provider.CompileAssemblyFromDom(parameter, unit);
                if (result.Errors.HasErrors)
                {
                    // 显示编译错误信息
                    System.Text.StringBuilder sb = new StringBuilder();
                    foreach (CompilerError ce in result.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                //记录缓存
                var objCache = HttpRuntime.Cache;
                // 缓存信息写入dll文件
                objCache.Insert(className, "1", null, DateTime.Now.AddMinutes(5), TimeSpan.Zero, CacheItemPriority.High, null);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.ErrorFormat("[ICPlatformTools][WebServiceHelper][CreateWebServiceDLL] Error:{0}", ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// 生成dll文件保存到本地
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法名</param>
        /// <param name="filePath">保存dll文件的路径</param>
        public static void CreateWebServiceDLL(string url, string className, string methodName, string filePath)
        {
            // 1. 使用 WebClient 下载 WSDL 信息。
            WebClient web = new WebClient();
            Stream stream = web.OpenRead(url);
            // 2. 创建和格式化 WSDL 文档。
            ServiceDescription description = ServiceDescription.Read(stream);
            //如果不存在就创建file文件夹
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            if (File.Exists(filePath + className + "_" + methodName + ".dll"))
            {
                //判断缓存是否过期
                var cachevalue = HttpRuntime.Cache.Get(className + "_" + methodName);
                if (cachevalue == null)
                {
                    //缓存过期删除dll
                    File.Delete(filePath + className + "_" + methodName + ".dll");
                }
                else
                {
                    // 如果缓存没有过期直接返回
                    return;
                }
            }

            // 3. 创建客户端代理代理类。
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            // 指定访问协议。
            importer.ProtocolName = "Soap";
            // 生成客户端代理。
            importer.Style = ServiceDescriptionImportStyle.Client;
            importer.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync;
            // 添加 WSDL 文档。
            importer.AddServiceDescription(description, null, null);
            // 4. 使用 CodeDom 编译客户端代理类。
            // 为代理类添加命名空间，缺省为全局空间。
            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit = new CodeCompileUnit();
            unit.Namespaces.Add(nmspace);
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit);
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameter = new CompilerParameters();
            parameter.GenerateExecutable = false;
            // 可以指定你所需的任何文件名。
            parameter.OutputAssembly = filePath + className + "_" + methodName + ".dll";
            parameter.ReferencedAssemblies.Add("System.dll");
            parameter.ReferencedAssemblies.Add("System.XML.dll");
            parameter.ReferencedAssemblies.Add("System.Web.Services.dll");
            parameter.ReferencedAssemblies.Add("System.Data.dll");
            // 生成dll文件，并会把WebService信息写入到dll里面
            CompilerResults result = provider.CompileAssemblyFromDom(parameter, unit);
            if (result.Errors.HasErrors)
            {
                // 显示编译错误信息
                System.Text.StringBuilder sb = new StringBuilder();
                foreach (CompilerError ce in result.Errors)
                {
                    sb.Append(ce.ToString());
                    sb.Append(System.Environment.NewLine);
                }
                throw new Exception(sb.ToString());
            }
            //记录缓存
            var objCache = HttpRuntime.Cache;
            // 缓存信息写入dll文件
            objCache.Insert(className + "_" + methodName, "1", null, DateTime.Now.AddMinutes(5), TimeSpan.Zero, CacheItemPriority.High, null);
        }
    }
}