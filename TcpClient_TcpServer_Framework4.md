# .NET Framework 4.0 下 TcpClient/TcpServer 多线程开发全解

面向 C# 4.0 / .NET Framework 4.0 环境，基于 `System.Net.Sockets` 的 TCP 通信与多线程实践。包含 API 讲解、线程模型、粘包处理、超时与示例代码。

## 1. 环境与项目基础
- 目标框架：`.NET Framework 4.0`，语言版本 C# 4.0。
- 必需命名空间：`System`, `System.Net`, `System.Net.Sockets`, `System.Threading`, `System.IO`.
- 线程工具：`Thread`, `ThreadPool`, `Task`（TPL 已随 4.0 提供），`ManualResetEvent`/`CancellationTokenSource`。
- 基本配置：
  - `App.config` 中若需支持 IPv6，确保 `<ipv6 enabled="true" />`。
  - 服务器常设 `listener.Start(backlog)`，`backlog` 控制未完成连接队列长度。
  - 关闭时调用 `TcpClient.Close()`/`Dispose()` 或 `Socket.Close()`，避免 TIME_WAIT 积压。

## 2. 核心类型与关键 API
### TcpListener（服务端）
- `TcpListener(IPAddress, int)` / `TcpListener(IPEndPoint)`：创建监听。
- `Start(int backlog = 0)` / `Stop()`：开启/关闭监听。
- `Pending()`：是否有等待的连接（可用于非阻塞轮询）。
- `AcceptTcpClient()` / `AcceptSocket()`：同步阻塞接收。
- `BeginAcceptTcpClient(AsyncCallback, object)` / `EndAcceptTcpClient(IAsyncResult)`：Apm 异步接收。

### TcpClient（客户端/服务端侧会话）
- `Connect(string host, int port)` / `Connect(IPAddress, int)`：同步连接。
- `BeginConnect(...)` / `EndConnect(...)`：Apm 异步连接，可配合 `IAsyncResult.AsyncWaitHandle.WaitOne(timeout)` 做超时。
- `GetStream()`：获取 `NetworkStream`。
- `Client`：底层 `Socket`，可设置选项：
  - `NoDelay`（禁用 Nagle 降低延迟）、`ReceiveBufferSize`/`SendBufferSize`、`ReceiveTimeout`/`SendTimeout`
  - `SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true)` 开启 TCP keep-alive。
- `Close()` / `Dispose()`：释放连接；`Close()` 会触发底层 `Socket.Shutdown`。

### NetworkStream（读写）
- `Read(byte[], int, int)` / `Write(...)`：同步阻塞。
- `BeginRead(...)` / `EndRead(...)`、`BeginWrite(...)` / `EndWrite(...)`：Apm 异步。
- `DataAvailable`：当前是否有可读数据（仅当前缓冲，不等于“对端在线”）。
- `CanRead`/`CanWrite`：检查流状态。

### Socket 常用选项（通过 `TcpClient.Client` 设置）
- `NoDelay`：`true` 适合小包实时通信。
- `KeepAlive`：保持探测；需要服务端配合超时策略。
- `LingerOption`：控制 `Close` 是否阻塞等待发送缓冲清空。
- `Blocking`：一般保持默认阻塞，非阻塞需自管状态与 EWOULDBLOCK 错误。

## 3. 通信模型选择
- **同步阻塞模型**：简单直观，`AcceptTcpClient` + `Read/Write` 阻塞。适合连接数不大、逻辑简单的场景。
- **Apm 异步模型（Begin/End）**：减少线程占用，适合高并发或需要超时控制的连接/收发。
- **线程模型建议**：
  - 独立线程执行 `Accept` 循环（或主线程 + `while`）。
  - 每个客户端最少 1 个接收线程/回调；发送可用线程安全队列 + 单发送线程（或 `lock` 保护写入）。
  - 共享状态用 `ConcurrentQueue<T>`（4.0 提供）或 `BlockingCollection<T>` 管理。

## 4. 粘包/半包处理
- TCP 是字节流，无消息边界，需要自定义协议：
  - **长度前缀**：`[4 字节长度][payload...]`，长度使用网络字节序（大端）或统一约定。
  - 读循环：先读取 4 字节长度，再循环读取正文直到满足长度。
  - 禁止假设一次 `Read` 就能拿到完整消息；可能 0~N 字节。
- 可选方案：分隔符（如 `\n`）或固定长度帧，但长度前缀最稳妥。

## 5. 服务器示例：同步 + 线程池
```csharp
// .NET Framework 4.0
class LengthPrefixServer
{
    private readonly TcpListener _listener;
    private volatile bool _running;

    public LengthPrefixServer(IPAddress ip, int port)
    {
        _listener = new TcpListener(ip, port);
    }

    public void Start(int backlog = 100)
    {
        _listener.Start(backlog);
        _running = true;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            while (_running)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient(); // 阻塞
                    client.NoDelay = true;
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
                catch (SocketException) { CloseClient(client); }
                catch (ObjectDisposedException) { break; } // Stop 后退出
            }
        });
    }

    private void HandleClient(object state)
    {
        using (var client = (TcpClient)state)
        using (var stream = client.GetStream())
        {
            var lenBuf = new byte[4];
            while (_running && client.Connected)
            {
                if (!ReadExact(stream, lenBuf, 4)) break;
                int len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
                var payload = new byte[len];
                if (!ReadExact(stream, payload, len)) break;
                // TODO: 处理 payload
                Echo(stream, payload); // 示例回显
            }
        }
    }

    private static bool ReadExact(NetworkStream stream, byte[] buf, int size)
    {
        int read = 0;
        while (read < size)
        {
            int n = stream.Read(buf, read, size - read); // 返回 0 代表对端关闭
            if (n == 0) return false;
            read += n;
        }
        return true;
    }

    private static void Echo(NetworkStream stream, byte[] payload)
    {
        var lenBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
        lock (stream) // 简单串行化写入
        {
            stream.Write(lenBytes, 0, lenBytes.Length);
            stream.Write(payload, 0, payload.Length);
        }
    }

    public void Stop()
    {
        _running = false;
        _listener.Stop();
    }

    private static void CloseClient(TcpClient c) { try { c.Close(); } catch { } }
}
```
- 要点：`AcceptTcpClient` 与 `Read` 阻塞在 ThreadPool 线程中；写入用 `lock` 避免交叉；使用长度前缀规避粘包。

## 6. 服务器：Apm 异步 Accept + 异步收发
```csharp
class AsyncAcceptServer
{
    private TcpListener _listener;
    private volatile bool _running;

    public void Start(IPAddress ip, int port)
    {
        _listener = new TcpListener(ip, port);
        _listener.Start();
        _running = true;
        _listener.BeginAcceptTcpClient(OnAccept, null);
    }

    private void OnAccept(IAsyncResult ar)
    {
        if (!_running) return;
        TcpClient client = null;
        try
        {
            client = _listener.EndAcceptTcpClient(ar);
            BeginRead(client);
        }
        catch (ObjectDisposedException) { return; }
        finally
        {
            if (_running) _listener.BeginAcceptTcpClient(OnAccept, null);
        }
    }

    private void BeginRead(TcpClient client)
    {
        var stream = client.GetStream();
        var state = new ClientState { Client = client, Stream = stream, Buffer = new byte[4096] };
        stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnRead, state);
    }

    private void OnRead(IAsyncResult ar)
    {
        var state = (ClientState)ar.AsyncState;
        try
        {
            int n = state.Stream.EndRead(ar);
            if (n == 0) { state.Dispose(); return; }
            // TODO: 处理 state.Buffer[0..n)
            state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnRead, state);
        }
        catch { state.Dispose(); }
    }

    public void Stop()
    {
        _running = false;
        _listener.Stop();
    }

    private class ClientState : IDisposable
    {
        public TcpClient Client; public NetworkStream Stream; public byte[] Buffer;
        public void Dispose() { try { Stream.Close(); } catch { } try { Client.Close(); } catch { } }
    }
}
```
- 要点：Apm 模式减少线程占用；注意 Stop 后 `ObjectDisposedException`；在回调中尽快返回，避免阻塞 IOCP 线程。

## 7. 客户端示例：连接超时 + 长度前缀发送
```csharp
class LengthPrefixClient : IDisposable
{
    private readonly TcpClient _client = new TcpClient();

    public bool Connect(string host, int port, int timeoutMs)
    {
        var ar = _client.BeginConnect(host, port, null, null);
        if (!ar.AsyncWaitHandle.WaitOne(timeoutMs))
        {
            _client.Close();
            return false;
        }
        _client.EndConnect(ar);
        _client.NoDelay = true;
        return true;
    }

    public void Send(byte[] payload)
    {
        var stream = _client.GetStream();
        var lenBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
        lock (stream)
        {
            stream.Write(lenBytes, 0, lenBytes.Length);
            stream.Write(payload, 0, payload.Length);
        }
    }

    public byte[] ReceiveOnce()
    {
        var stream = _client.GetStream();
        var lenBuf = new byte[4];
        if (!ReadExact(stream, lenBuf, 4)) return null;
        int len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
        var payload = new byte[len];
        return ReadExact(stream, payload, len) ? payload : null;
    }

    private static bool ReadExact(NetworkStream s, byte[] buf, int size)
    {
        int read = 0;
        while (read < size)
        {
            int n = s.Read(buf, read, size - read);
            if (n == 0) return false;
            read += n;
        }
        return true;
    }

    public void Dispose() { try { _client.Close(); } catch { } }
}
```
- 连接超时：`BeginConnect` + `WaitOne`；成功后必须调用 `EndConnect`。
- 写入加锁，保证消息边界与顺序。

## 8. 多线程与并发安全
- 接收线程安全：每个连接独立读线程/回调即可；避免多个线程同时读同一 `NetworkStream`。
- 发送安全：
  - 最简单：`lock(stream)` 包裹写入。
  - 高频写场景：使用 `ConcurrentQueue<byte[]>` + 单发送线程或 `AutoResetEvent` 消费队列。
- 取消与关闭：
  - 使用 `_running` 标志或 `CancellationTokenSource`（4.0 可用）在循环中检查。
  - `Close` 后的 `Read` 会返回 0；注意避免在关闭后继续提交异步读。

## 9. 超时、心跳与存活检测
- `ReceiveTimeout`/`SendTimeout`：触发后 `Read/Write` 抛出 `IOException`（内部 `SocketException`）。
- 心跳：周期性发送短包，服务器在超时窗口未收到心跳则断开。
- TCP Keep-Alive：`SetSocketOption(KeepAlive)`；具体间隔由系统 TCP 栈决定，可通过 P/Invoke 调整（可选）。
- 拒绝阻塞关闭：设置 `LingerOption(false, 0)` 以快速丢弃未发送数据（需要业务允许）。

## 10. 调试与排错清单
- 端口占用：`netstat -ano | findstr <port>`。
- 防火墙：确保监听端口允许入站。
- 粘包：确认每个消息有明确长度/分隔符；抓包（Wireshark）核对。
- 线程泄漏：监控线程数，确认所有连接 `Dispose`；异步回调捕获异常并关闭。
- 阻塞点：大部分问题来自未读/未写阻塞；必要时为 `Read` 设置超时或改为异步。

## 11. 设计清单（快速复用）
- 协议：长度前缀（int32 大端）。
- 服务端：独立 Accept 线程 + 每连接读线程（或异步回调），发送 `lock` 或队列。
- 客户端：`BeginConnect` 做超时；`NoDelay = true`；写入加锁。
- 资源：`using`/`Close`，避免悬挂流；Stop 时先置 `_running=false` 再 `Stop()`。
- 日志：在 Accept、Connect、Read 0 字节、异常处记录，方便排查。 
