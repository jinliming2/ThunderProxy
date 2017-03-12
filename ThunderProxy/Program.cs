using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ThunderProxy {
    class Program {
        const string CONFIG_FILE = "config.xml";
        string command;
        byte[] buffer = new byte[32768];

        static void Main(string[] args) {
            var program = new Program();
            program.LoadConfig();
            if(args.Length < 1) {
                Console.WriteLine("按回车键退出...");
                Console.ReadLine();
                return;
            }
            IEnumerator ie = program.Start(args[0]).GetEnumerator();
            ie.MoveNext();
            int port = (int)ie.Current;
            Process.Start(program.command, Regex.Replace(args[0], @":\d+/", ":" + port + "/"));
            ie.MoveNext();
            Console.ReadLine();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        void LoadConfig() {
            if(!File.Exists(CONFIG_FILE)) {
                File.WriteAllText(CONFIG_FILE, @"<?xml version=""1.0"" encoding=""utf-8""?>
<Config>
    <Command><![CDATA[C:\Program Files\VideoLAN\VLC\vlc.exe]]></Command>
</Config>");
                Console.WriteLine("已生成默认配置文件！");
            }
            XDocument xml = XDocument.Load(CONFIG_FILE);
            command = xml.Element("Config").Element("Command").Value;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        IEnumerable Start(string address) {
            TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            tcpListener.Start();
            yield return (tcpListener.LocalEndpoint as IPEndPoint).Port;
            while(true) {
                try {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    int p = int.Parse(Regex.Match(address, @":(\d+)/").Groups[1].Value);
                    TcpClient server = new TcpClient("127.0.0.1", int.Parse(Regex.Match(address, @":(\d+)/").Groups[1].Value));
                    NetworkStream c_stream = client.GetStream();
                    NetworkStream s_stream = server.GetStream();
                    StreamReader c_reader = new StreamReader(c_stream);
                    StreamWriter s_writer = new StreamWriter(s_stream);
                    try {
                        //c --> s
                        string line, header = "";
                        //bool flag = true;
                        while((line = c_reader.ReadLine()).Trim().Length > 0) {
                            //if(line.ToLower().StartsWith("connection:")) {
                            //    flag = false;
                            //    header += "Connection: close\r\n";
                            //} else {
                                header += line + "\r\n";
                            //}
                        }
                        //if(flag) {
                        //    header += "Connection: close\r\n";
                        //}
                        header += "\r\n";
                        s_writer.Write(header);
                        s_writer.Flush();
                        //s --> c
                        int length;
                        while((length = s_stream.Read(buffer, 0, buffer.Length)) > 0) {
                            c_stream.Write(buffer, 0, length);
                        }
                        c_stream.Flush();
                    } catch(Exception e) {
                        Console.WriteLine(e.Message);
                        c_reader.Dispose();
                        s_writer.Dispose();
                        c_stream.Dispose();
                        s_stream.Dispose();
                    }
                } catch(Exception e)  {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }
    }
}
