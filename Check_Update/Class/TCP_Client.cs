﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Check_Update
{
    public class TCP_Client
    {
        //DATAコマンド受信時のコールバック
        public delegate void RecieveDATACallback(string data);
        public event RecieveDATACallback DataReceive;
        //CMD1コマンド受信時のコールバック
        public delegate void RecieveCMD1Callback(string data);
        //未定義コマンド受信時のコールバック
        public delegate void RecieveFreeStrCallback(string data);
        private TcpClient client;
        private Encoding encoding;
        public TCP_Client()
        {
            client = new TcpClient();
            encoding = Encoding.UTF8;
        }
        public void Connect()
        {
            ConnectStart(SRTTbacon_Server.IP, SRTTbacon_Server.Port);
        }
        public void Dispose()
        {
            if (connecting_flg)
                return;
            client.Close();
            client.Dispose();
        }
        public void Close()
        {
            if (connecting_flg)
                return;
            client.Close();
        }
        public void Send(string message)
        {
            if (!this.IsConnected)
            {
                try
                {
                    Connect();
                }
                catch
                {
                    return;
                }
            }
            NetworkStream ns = client.GetStream();
            byte[] message_byte = encoding.GetBytes(message + "\r\n");
            ns.Write(message_byte, 0, message_byte.Length);
        }
        public bool IsConnected
        {
            get
            {
                bool b = false;
                if (client != null && client.Client != null)
                    b = client.Connected;
                return b;
            }
        }
        private bool connecting_flg;
        private void ConnectStart(string ip, int port)
        {
            if (client != null && client.Client != null && client.Connected)
                return;
            if (connecting_flg)
                return;
            client = new TcpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            byte[] tcp_keepalive = new byte[12];
            BitConverter.GetBytes((Int32)1).CopyTo(tcp_keepalive, 0);
            BitConverter.GetBytes((Int32)2000).CopyTo(tcp_keepalive, 4);
            BitConverter.GetBytes((Int32)500).CopyTo(tcp_keepalive, 8);
            client.Client.IOControl(IOControlCode.KeepAliveValues, tcp_keepalive, null);
            try
            {
                connecting_flg = true;
                client.Connect(ip, port);
            }
            catch (System.Net.Sockets.SocketException)
            {
                connecting_flg = false;
                client.Close();
                return;
            }
            catch
            {
                connecting_flg = false;
                client.Close();
                return;
            }
            connecting_flg = true;
            _ = Recievewait_Async();
        }
        //非同期でクライアントから文字列受信を待ち受ける
        private async Task Recievewait_Async()
        {
            NetworkStream ns = client.GetStream();
            while (connecting_flg)
            {
                var ms = new MemoryStream();
                byte[] result_bytes = new byte[16];
                do
                {
                    int result_size = 0;
                    try
                    {
                        result_size = await ns.ReadAsync(result_bytes, 0, result_bytes.Length);
                    }
                    catch (System.IO.IOException)
                    {
                        //一定期間サーバーとの通信がなかったら自動で切断されるため、例外を吐く
                        Connect();
                    }
                    if (result_size == 0)
                    {
                        client.Close();
                        ms.Close();
                        ms.Dispose();
                        return;
                    }
                    ms.Write(result_bytes, 0, result_size);
                } while (ns.DataAvailable);
                string message = encoding.GetString(ms.ToArray());
                Received(message);
                ms.Close();
                ms.Dispose();
            }
        }
        //受信した文字列処理
        //      複数のコマンドがくっついている可能性があるので改行で分解する
        private void Received(string message)
        {
            string[] lines = message.Split('\n');
            foreach (string line in lines)
            {
                string trimline = line.Trim();
                if (trimline.Length == 0)
                    continue;
                DataReceive(trimline);
            }
        }
    }
}