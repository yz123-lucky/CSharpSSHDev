using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SSHTestCode
{
    /// <summary>
    /// SSH工具助手类
    /// 基于.NET Framework 4.0
    /// 使用Renci.SshNet库
    /// 支持多线程调用
    /// </summary>
    public class SshHelper : IDisposable
    {
        #region 私有字段

        private SshClient _sshClient;
        private ShellStream _shellStream;
        private string _host;
        private int _port;
        private string _username;
        private string _password;

        // 监听相关
        private Task _monitorTask;
        private CancellationTokenSource _monitorCts;
        private StringBuilder _monitorData;
        private readonly object _monitorDataLock = new object();
        private volatile bool _isMonitorPaused;

        // 后台发送相关
        private Task _backenWriteTask;
        private CancellationTokenSource _backenWriteCts;
        private volatile bool _isBackenWritePaused;

        // SSH操作锁
        private readonly object _sshLock = new object();

        private bool _disposed = false;

        #endregion

        #region 构造函数

        public SshHelper()
        {
            _monitorData = new StringBuilder();
            _isMonitorPaused = false;
            _isBackenWritePaused = false;
        }

        #endregion

        #region 1. Init方法

        /// <summary>
        /// 初始化SSH连接参数
        /// </summary>
        public void Init(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;

            _sshClient = new SshClient(host, port, username, password);
            _sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(30);
        }

        public void Init(string host, string username, string password)
        {
            Init(host, 22, username, password);
        }

        #endregion

        #region 2. OpenSerialPort方法

        /// <summary>
        /// 打开SSH连接
        /// </summary>
        public bool OpenSerialPort()
        {
            try
            {
                lock (_sshLock)
                {
                    if (_sshClient != null && !_sshClient.IsConnected)
                    {
                        _sshClient.Connect();
                        if (_sshClient.IsConnected)
                        {
                            _shellStream = _sshClient.CreateShellStream("terminal", 80, 24, 800, 600, 1024);
                            Thread.Sleep(500);
                            _shellStream.Read();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        #endregion

        #region 3. SetMsg方法

        /// <summary>
        /// 发送指令
        /// </summary>
        public void SetMsg(string msg)
        {
            try
            {
                lock (_sshLock)
                {
                    if (_shellStream != null && _shellStream.CanWrite)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(msg);
                        _shellStream.Write(data, 0, data.Length);
                        _shellStream.Flush();
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region 4. SendAndReadMsg方法

        /// <summary>
        /// 发送指令并读取返回值
        /// </summary>
        public string SendAndReadMsg(string msg, string rightMsg, int emptyTimeout, int readTimeout)
        {
            try
            {
                SetMsg(msg);
                return ReadRightMsg(rightMsg, emptyTimeout, readTimeout);
            }
            catch
            {
                return "设置失败：发送或读取异常";
            }
        }

        #endregion

        #region 5. SendAndReadMultiMsg方法

        /// <summary>
        /// 发送指令并读取返回值（支持多个期望值）
        /// </summary>
        public string SendAndReadMultiMsg(string msg, List<object> rightMsgList, int emptyTimeout, int readTimeout)
        {
            try
            {
                SetMsg(msg);

                DateTime startTime = DateTime.Now;
                StringBuilder receivedData = new StringBuilder();

                while ((DateTime.Now - startTime).TotalMilliseconds < readTimeout)
                {
                    string data = ReadMsg(emptyTimeout);

                    if (data.StartsWith("设置失败"))
                    {
                        continue;
                    }

                    receivedData.Append(data);
                    string fullData = receivedData.ToString();

                    foreach (object rightMsg in rightMsgList)
                    {
                        if (fullData.Contains(rightMsg.ToString()))
                        {
                            return "设置成功：" + rightMsg.ToString();
                        }
                    }

                    Thread.Sleep(50);
                }

                return "设置失败：未读取到期望的返回值";
            }
            catch
            {
                return "设置失败：读取数据异常";
            }
        }

        #endregion

        #region 6. ReadMsg方法

        /// <summary>
        /// 读取SSH消息
        /// </summary>
        public string ReadMsg(int timeout)
        {
            try
            {
                lock (_sshLock)
                {
                    if (_shellStream != null && _shellStream.DataAvailable)
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        }
                    }
                    else
                    {
                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
                        {
                            if (_shellStream != null && _shellStream.DataAvailable)
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                }
                            }
                            Thread.Sleep(10);
                        }
                        return "设置失败：SSH接收缓冲区为空读取超时";
                    }
                }
            }
            catch
            {
                return "设置失败：读取数据异常";
            }
            return string.Empty;
        }

        #endregion

        #region 7. ReadRightMsg方法

        /// <summary>
        /// 读取正确的返回值
        /// </summary>
        public string ReadRightMsg(string rightMsg, int emptyTimeout, int readTimeout)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                StringBuilder receivedData = new StringBuilder();

                while ((DateTime.Now - startTime).TotalMilliseconds < readTimeout)
                {
                    string data = ReadMsg(emptyTimeout);

                    if (data.StartsWith("设置失败"))
                    {
                        if (data.Contains("读取超时"))
                        {
                            continue;
                        }
                        return data;
                    }

                    receivedData.Append(data);

                    if (receivedData.ToString().Contains(rightMsg))
                    {
                        return "设置成功：" + rightMsg;
                    }

                    Thread.Sleep(50);
                }

                return "设置失败：SSH读取错误";
            }
            catch
            {
                return "设置失败：读取数据异常";
            }
        }

        #endregion

        #region 8. PollingReadMsgAndReturnMsg方法

        /// <summary>
        /// 轮询读取消息直到读取到正确值
        /// </summary>
        public string PollingReadMsgAndReturnMsg(int maxCount, int interval, string rightMsg)
        {
            for (int i = 0; i < maxCount; i++)
            {
                try
                {
                    lock (_sshLock)
                    {
                        if (_shellStream != null && _shellStream.DataAvailable)
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                if (data.Contains(rightMsg))
                                {
                                    return data;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                Thread.Sleep(interval);
            }

            return "设置失败：轮询超时未读取到正确值";
        }

        #endregion

        #region 9. PollingReadMsg方法

        /// <summary>
        /// 轮询读取消息并拼接
        /// </summary>
        public string PollingReadMsg(int maxCount, int interval, string rightMsg)
        {
            StringBuilder fullData = new StringBuilder();

            for (int i = 0; i < maxCount; i++)
            {
                try
                {
                    lock (_sshLock)
                    {
                        if (_shellStream != null && _shellStream.DataAvailable)
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                fullData.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                            }
                        }
                    }
                }
                catch
                {
                }

                Thread.Sleep(interval);
            }

            return fullData.ToString();
        }

        #endregion

        #region 10. StartBackenMonitor方法

        /// <summary>
        /// 开启后台监听任务
        /// </summary>
        public void StartBackenMonitor()
        {
            if (_monitorTask != null && _monitorTask.Status == TaskStatus.Running)
            {
                return;
            }

            _monitorCts = new CancellationTokenSource();
            CancellationToken token = _monitorCts.Token;

            _monitorTask = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_isMonitorPaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    try
                    {
                        lock (_sshLock)
                        {
                            if (_shellStream != null && _shellStream.DataAvailable)
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                    lock (_monitorDataLock)
                                    {
                                        _monitorData.Append(data);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

                    Thread.Sleep(50);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion

        #region 11. GetMonitorData方法

        /// <summary>
        /// 获取监听到的数据
        /// </summary>
        public string GetMonitorData()
        {
            lock (_monitorDataLock)
            {
                return _monitorData.ToString();
            }
        }

        #endregion

        #region 12. SetMsgWhenMonitor方法

        /// <summary>
        /// 在监听时发送数据
        /// </summary>
        public void SetMsgWhenMonitor(string msg)
        {
            SetMsg(msg);
        }

        #endregion

        #region 13. ClearMonitorData方法

        /// <summary>
        /// 清空监听数据
        /// </summary>
        public void ClearMonitorData()
        {
            lock (_monitorDataLock)
            {
                _monitorData.Clear();
            }
        }

        #endregion

        #region 14. PauseMonitorListen方法

        /// <summary>
        /// 暂停监听任务
        /// </summary>
        public void PauseMonitorListen()
        {
            _isMonitorPaused = true;
        }

        #endregion

        #region 15. ResumeMonitorListen方法

        /// <summary>
        /// 恢复监听任务
        /// </summary>
        public void ResumeMonitorListen()
        {
            _isMonitorPaused = false;
        }

        #endregion

        #region 16. CloseMonit方法

        /// <summary>
        /// 停止监听任务
        /// </summary>
        public void CloseMonit()
        {
            if (_monitorCts != null)
            {
                _monitorCts.Cancel();
            }

            if (_monitorTask != null)
            {
                try
                {
                    _monitorTask.Wait(1000);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region 17. SetMsgBacken方法

        /// <summary>
        /// 开启后台发送任务
        /// </summary>
        public void SetMsgBacken(string msg, int interval)
        {
            if (_backenWriteTask != null && _backenWriteTask.Status == TaskStatus.Running)
            {
                return;
            }

            _backenWriteCts = new CancellationTokenSource();
            CancellationToken token = _backenWriteCts.Token;

            _backenWriteTask = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_isBackenWritePaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    try
                    {
                        SetMsg(msg);
                    }
                    catch
                    {
                    }

                    Thread.Sleep(interval);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion

        #region 18. PauseBackenWrite方法

        /// <summary>
        /// 暂停后台写入任务
        /// </summary>
        public void PauseBackenWrite()
        {
            _isBackenWritePaused = true;
        }

        #endregion

        #region 19. ResumeBackenWrite方法

        /// <summary>
        /// 恢复后台写入任务
        /// </summary>
        public void ResumeBackenWrite()
        {
            _isBackenWritePaused = false;
        }

        #endregion

        #region StopBackenWrite方法

        /// <summary>
        /// 停止后台写入任务
        /// </summary>
        public void StopBackenWrite()
        {
            if (_backenWriteCts != null)
            {
                _backenWriteCts.Cancel();
            }

            if (_backenWriteTask != null)
            {
                try
                {
                    _backenWriteTask.Wait(1000);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Free方法

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Free()
        {
            Dispose();
        }

        #endregion

        #region 辅助方法

        public bool IsOpen()
        {
            return _sshClient != null && _sshClient.IsConnected;
        }

        public void CloseSerialPort()
        {
            try
            {
                lock (_sshLock)
                {
                    if (_shellStream != null)
                    {
                        _shellStream.Close();
                        _shellStream.Dispose();
                        _shellStream = null;
                    }

                    if (_sshClient != null && _sshClient.IsConnected)
                    {
                        _sshClient.Disconnect();
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region IDisposable实现

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseMonit();
                    StopBackenWrite();
                    CloseSerialPort();

                    if (_monitorCts != null)
                    {
                        _monitorCts.Dispose();
                    }

                    if (_backenWriteCts != null)
                    {
                        _backenWriteCts.Dispose();
                    }

                    if (_sshClient != null)
                    {
                        _sshClient.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~SshHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
