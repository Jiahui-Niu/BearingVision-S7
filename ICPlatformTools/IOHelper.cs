using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public static class IOHelper
    {
        /*
         * 获取父目录所有文件, 忽略异常
         * */
        public static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption, System.Threading.CancellationToken token)
        {
            var foundFiles = Enumerable.Empty<string>();

            if (token.IsCancellationRequested)
            {
                return foundFiles;
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foreach (string dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryFiles(dir, patternMatch, searchOption, token)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException)
                {
                    LogHelper.Log.Error("[FtpUploader] 文件访问失败");
                }
                catch (PathTooLongException)
                {
                    LogHelper.Log.Error("[FtpUploader] 文件名过长");
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
            }

            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)); // Add files from the current directory
            }
            catch (UnauthorizedAccessException)
            {
                LogHelper.Log.Error("[FtpUploader] 文件访问失败2");
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }

            return foundFiles;
        }

        public static List<FileInfo> GetDirectoryFilesInfo(DirectoryInfo rootPath, string patternMatch, SearchOption searchOption, System.Threading.CancellationToken token, ref List<FileInfo> foundFiles)
        {
            if (token.IsCancellationRequested)
            {
                return foundFiles;
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    var subDirs = rootPath.EnumerateDirectories().OrderBy(s => s.CreationTime);
                    foreach (var dir in subDirs)
                    {
                        GetDirectoryFilesInfo(dir, patternMatch, searchOption, token, ref foundFiles);
                    }
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException)
                {
                    LogHelper.Log.Error("[IOHelper] 文件访问失败");
                }
                catch (PathTooLongException)
                {
                    LogHelper.Log.Error("[IOHelper] 文件名过长");
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
            }

            try
            {
                foundFiles.AddRange(rootPath.EnumerateFiles(patternMatch).OrderBy(s => s.CreationTime));
            }
            catch (UnauthorizedAccessException)
            {
                LogHelper.Log.Error("[IOHelper] 文件访问失败2");
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }

            return foundFiles;
        }

        public static IEnumerable<FileInfo> GetDirectoryFilesInfo(DirectoryInfo rootPath, string patternMatch, SearchOption searchOption, System.Threading.CancellationToken token)
        {
            var foundFiles = Enumerable.Empty<FileInfo>();

            if (token.IsCancellationRequested)
            {
                return foundFiles;
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    var subDirs = rootPath.EnumerateDirectories().OrderBy(s => s.CreationTime);
                    foreach (var dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryFilesInfo(dir, patternMatch, searchOption, token)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException)
                {
                    LogHelper.Log.Error("[IOHelper] 文件访问失败");
                }
                catch (PathTooLongException)
                {
                    LogHelper.Log.Error("[IOHelper] 文件名过长");
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
            }

            try
            {
                foundFiles = foundFiles.Concat(rootPath.EnumerateFiles(patternMatch).OrderBy(s => s.CreationTime));
            }
            catch (UnauthorizedAccessException)
            {
                LogHelper.Log.Error("[IOHelper] 文件访问失败2");
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }

            return foundFiles;
        }

        public static bool IsSSD(this DriveInfo drive)
        {
            SafeFileHandle fileHandle = NativeMethods.CreateFileW("\\\\.\\" + drive.Name[0].ToString() + ":", 0, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, NativeMethods.OPEN_EXISTING, NativeMethods.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

            uint IOCTL_STORAGE_QUERY_PROPERTY = NativeMethods.CTL_CODE(
                    NativeMethods.IOCTL_STORAGE_BASE, 0x500,
                    NativeMethods.METHOD_BUFFERED, NativeMethods.FILE_ANY_ACCESS); // From winioctl.h

            NativeMethods.STORAGE_PROPERTY_QUERY propertyQuery = new NativeMethods.STORAGE_PROPERTY_QUERY();
            propertyQuery.PropertyId = (uint)NativeMethods.STORAGE_PROPERTY_ID.StorageDeviceTrimProperty;
            propertyQuery.QueryType = NativeMethods.PropertyStandardQuery;

            NativeMethods.DEVICE_TRIM_DESCRIPTOR trimDescriptor = new NativeMethods.DEVICE_TRIM_DESCRIPTOR();

            uint returnedSize;

            bool ret = NativeMethods.DeviceIoControl(fileHandle, IOCTL_STORAGE_QUERY_PROPERTY, ref propertyQuery, (uint)Marshal.SizeOf(propertyQuery), ref trimDescriptor, (uint)Marshal.SizeOf(trimDescriptor), out returnedSize, IntPtr.Zero);

            fileHandle.Close();

            return trimDescriptor.TrimEnabled;
        }

        public static System.IO.DriveInfo GetDrive(string path)
        {
            try
            {
                var root = System.IO.Path.GetPathRoot(path);
                var drive = System.IO.DriveInfo.GetDrives().Where(s => root.Contains(s.Name)).FirstOrDefault();
                return drive; 
            }
            catch { }
            return null;
        }
    }
}
