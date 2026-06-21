using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public enum ErrorCode
    {
        None = 0,
        Unknown = 0x8000,
        OpenDatabaseError,
        CreateDatabaseError,
        CreateThreadError,
        OpenTcpServerError,
        OpenTcpClientError,
        OpenSerialPortError,
        InitHttpClientError,
        InitSFTPClientError,
        OpenPackageModuleError,
        OpenRollingBeltError,
        ImageSaveRootNotFound,
        ImageSavePathEqualToRoot,
        SocketBindPortError,
        InvalidIPAddress,
        OpenAppCfgError,
    }

    /// <summary>
    /// 错误状态
    /// </summary>
    [Flags]
    public enum ErrorStatus
    {
        None = 0,                       // 无错误, 正常
        NoCode = 1 << 0,                // 无条码     
        NoWeight = 1 << 1,              // 无重量
        NoVolume = 1 << 2,              // 无体积     
        WeightTooHeavy = 1 << 3,        // 重量超范围     
        LengthTooLong = 1 << 4,         // 长度超范围
        Noread = 1 << 5,                // noread
        MultiCodes = 1 << 6,            // 多条码
        WeightTooThin = 1 << 7,         // 超轻
        LengthTooShort = 1 << 8,        // 超短 
    }

    public class ErrorHelper
    {
        private static readonly Dictionary<ErrorCode, string> s_dictAppError = new Dictionary<ErrorCode, string>()
            {
                { ErrorCode.Unknown, "MsgUnknownError" }, 
                { ErrorCode.OpenDatabaseError, "MsgOpenDatabaseError" }, 
                { ErrorCode.CreateDatabaseError, "MsgCreateDatabaseError" }, 
                { ErrorCode.CreateThreadError, "MsgCreateThreadError" }, 
                { ErrorCode.OpenTcpServerError, "MsgOpenTcpServerError" }, 
                { ErrorCode.OpenTcpClientError, "MsgOpenTcpClientError" }, 
                { ErrorCode.OpenSerialPortError, "MsgOpenSerialPortError" }, 
                { ErrorCode.InitHttpClientError, "MsgInitHttpClientError" }, 
                { ErrorCode.InitSFTPClientError, "MsgInitSFTPClientError" },
                { ErrorCode.OpenPackageModuleError, "MsgOpenPackageModuleError" },
                { ErrorCode.OpenRollingBeltError, "MsgOpenRollingBeltError" },
                { ErrorCode.ImageSaveRootNotFound, "MsgImageSaveRootNotFound" },
                { ErrorCode.ImageSavePathEqualToRoot, "MsgImageSavePathEqualToRoot" },
                { ErrorCode.SocketBindPortError, "MsgSocketBindPortError" },
                { ErrorCode.InvalidIPAddress, "MsgInvalidIPAddress" }
            };

        private static readonly Dictionary<ErrorStatus, string> s_dictError = new Dictionary<ErrorStatus, string>()
        {
            { ErrorStatus.NoCode, "MsgNoCode" },
            { ErrorStatus.Noread, "MsgNoCode" },
            { ErrorStatus.MultiCodes, "MsgMultiCode" }, 
            { ErrorStatus.NoWeight, "MsgNoWeight" }, 
            { ErrorStatus.NoVolume, "MsgNoVolume" }, 
            { ErrorStatus.WeightTooHeavy, "MsgWeightTooHeavy" }, 
            { ErrorStatus.WeightTooThin, "MsgWeightTooThin" }, 
            { ErrorStatus.LengthTooLong, "MsgLengthTooLong" }, 
            { ErrorStatus.LengthTooShort, "MsgLengthTooShort" }, 
        };
        
        /// <summary>
        /// 获取APP错误翻译
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetAppErrorMessage(int code)
        {
            string msg;
            if (s_dictAppError.TryGetValue((ErrorCode)code, out msg))
            {
                return LanguageManager.Translate(msg);
            }
            return msg;
        }

        /// <summary>
        /// 获取APP错误翻译
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetErrorMessage(int code)
        {
            return GetAppErrorMessage(code);
        }

        /// <summary>
        /// 获取条码错误翻译
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetCodeErrorString(int code)
        {
            StringBuilder sb = new StringBuilder();

            var error = (ErrorStatus)code;
            foreach (var i in s_dictError)
            {
                if (error.HasFlag(i.Key))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(LanguageManager.Translate(i.Value));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 注册 errorCode, errorCode 需要大于0x10000, 避免冲突
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="translateKey"></param>
        /// <returns></returns>
        public static bool RegisterErrorCode(int errorCode, string translateKey)
        {
            if (errorCode < 0x10000 || s_dictAppError.ContainsKey((ErrorCode)errorCode))
            {
                return false;
            }

            s_dictAppError.Add((ErrorCode)errorCode, translateKey);
            return true;
        }

        /// <summary>
        /// 注册条码错误信息, 需满足位域 (大于 1 << (错误字典个数 + 1)) (最大 1 << 30)
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="translateKey"></param>
        /// <returns></returns>
        public static bool RegisterErrorStatus(int errorCode, string translateKey)
        {
            if (errorCode < (1 << (s_dictError.Count + 1)) || s_dictError.ContainsKey((ErrorStatus)errorCode))
            {
                return false;
            }

            s_dictError.Add((ErrorStatus)errorCode, translateKey);
            return true;
        }
    }
}
