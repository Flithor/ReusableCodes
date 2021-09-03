using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyReusableCodes
{
    /// <summary>
    /// Use TcpListener to wait Tcplink
    /// </summary>
    class TcpLinker
    {
        readonly List<TcpListener> tcpListeners;
        CancellationTokenSource stopToken;
        
        public bool IsRunning => stopToken != null;
        
        public TcpLinker(IEnumerable<int> ports)
        {
            tcpListeners = ports.Select(port => new TcpListener(IPAddress.Loopback, port)).ToList();
        }
        /// <summary>
        /// Start listening
        /// </summary>
        public async Task StartAsync(CancellationTokenSource tokenSource = null)
        {
            if (stopToken != null) return;
            stopToken = new CancellationTokenSource();
            await TcpRequestMonitor(stopToken.Token);
        }
        public void Stop()
        {
            stopToken.Cancel();
            tcpListeners.ForEach(tcpListeners => tcpListeners.Stop());
        }
        /// <summary>
        /// Waiting for Tcp link request
        /// </summary>
        /// <param name="token"></param>
        private async Task TcpRequestMonitor(CancellationToken token)
        {
            await Task.Run(delegate
            {
                tcpListeners.ForEach(tcpListener => _ = RunTcpListener(tcpListener, token));
            });
        }
        
        private async Task RunTcpListener(TcpListener tcpListener, CancellationToken token)
        {
            try
            {
                tcpListener.Start();
                while (!token.IsCancellationRequested)
                {
                    if (!await WaitPending(tcpListener, token)) return;
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();

                    TcpLinkMonitor(tcpClient, token);
                }
            }
            finally
            {
                if (stopToken?.IsCancellationRequested ?? false)
                {
                    stopToken?.Dispose();
                    stopToken = null;
                }
            }
        }
        
        /// <summary>
        /// Waiting for pended link
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> WaitPending(TcpListener tcpListener, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (tcpListener.Pending()) return true;
                await Task.Delay(25);
            }
            return false;
        }

        /// <summary>
        /// Monitor single Tcp link
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="token"></param>
        private async void TcpLinkMonitor(TcpClient tcpClient, CancellationToken token)
        {
            var remote = tcpClient.Client.RemoteEndPoint.ToString();
            try
            {
                const int bufferLength = 1024;
                var remote = tcpClient.Client.RemoteEndPoint.ToString();
                using (var tcpStream = tcpClient.GetStream())
                {
                    Console.WriteLine($"Tcp link connected, remote is: {remote}");
                    while (!token.IsCancellationRequested)
                    {
                        byte[] buffer = new byte[bufferLength];
                        // Read or ReadAsync will pending when the connection alived until disconnect or received avaliable data
                        var length = await tcpStream.ReadAsync(buffer, 0, bufferLength, token);
                        // If disconnect, Read or ReadAsync returns 0
                        if (length == 0 || token.IsCancellationRequested) break;
                        // Trim the array to the actual data length
                        Array.Resize(ref buffer, length);
                        // Process received data
                        ProcessTcpData(buffer);
                    }
                }
                Console.WriteLine($"Tcp link disconnected, remote is:{remote}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + $"\r\nDisconnected by exception, remote is:{remote}");
            }
        }
        /// <summary>
        /// Process received data
        /// </summary>
        /// <param name="buffer"></param>
        private void ProcessTcpData(byte[] buffer)
        {
            Console.WriteLine(Encoding.UTF8.GetString(buffer));
        }
    }
}
