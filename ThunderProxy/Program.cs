using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace ThunderProxy {
    class Program {
        string CONFIG_FILE = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.xml";
        string command;
        byte[] buffer = new byte[32768];

        static Process player;

        static void Main(string[] args) {
            var program = new Program();
            program.LoadConfig();
            if(args.Length < 1) {
                log("按回车键退出...");
                Console.ReadLine();
                return;
            }
            var arg = args[0][0] != '"' ? ('"' + args[0] + '"') : args[0];
            if(Regex.IsMatch(args[0], @"^[a-zA-Z]:")) {
                Process.Start(program.command, arg.Replace('\'', '/').Replace("//", "/"));
                return;
            }
            if(Regex.IsMatch(args[0], @"^file://")) {
                Process.Start(program.command, arg);
                return;
            }
            Console.Title = "Thunder Proxy - 按 Ctrl + C 退出";
            IEnumerator ie = program.Start(args[0]).GetEnumerator();
            ie.MoveNext();
            int port = (int)ie.Current;
            log("开始监听本地" + port + "端口...");
            player = Process.Start(program.command, Regex.Replace(arg, @":\d+/", ":" + port + "/"));
            log("已执行启动播放器命令...");
            ie.MoveNext();
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
                log("已生成默认配置文件！");
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
            new Thread(() => {
                while(true) {
                    player.WaitForExit();
                    try {
                        log("播放器已退出，退出状态码：" + player.ExitCode);
                        tcpListener.Stop();
                        break;
                    } catch(InvalidOperationException) {
                    } catch(Exception e) {
                        error(e.Message + "\n" + e.StackTrace);
                        break;
                    }
                }
            }).Start();
            while(true) {
                try {
                    log("等待连接...");
                    TcpClient client = tcpListener.AcceptTcpClient();
                    log("已接受连接...");
                    int port = int.Parse(Regex.Match(address, @":(\d+)/").Groups[1].Value);
                    TcpClient server = new TcpClient("127.0.0.1", port);
                    NetworkStream c_stream = client.GetStream();
                    NetworkStream s_stream = server.GetStream();
                    new Thread(() => {
                        log("已分配线程：" + Thread.CurrentThread.ManagedThreadId);
                        StreamReader c_reader = new StreamReader(c_stream);
                        StreamWriter c_writer = new StreamWriter(c_stream);
                        StreamReader s_reader = new StreamReader(s_stream);
                        StreamWriter s_writer = new StreamWriter(s_stream);
                        try {
                            //c --> s
                            string line, header = "";
                            while((line = c_reader.ReadLine()).Trim().Length > 0) {
                                if(line.ToLower().StartsWith("host:")) {
                                    header += "Host: 127.0.0.1:" + port + "\r\n";
                                } else {
                                    header += line + "\r\n";
                                }
                            }
                            header += "\r\n";
                            s_writer.Write(header);
                            s_writer.Flush();
                            //s --> c
                            header = "";
                            long total = long.MaxValue;
                            while((line = s_reader.ReadLine()).Trim().Length > 0) {
                                if(line.ToLower().StartsWith("content-length:")) {
                                    if(!long.TryParse(line.ToLower().Substring(15), out total)) {
                                        total = long.MaxValue;
                                    }
                                }
                                header += line + "\r\n";
                            }
                            header += "\r\n";
                            c_writer.Write(header);
                            c_writer.Flush();
                            int length;
                            while(total > 0 && (length = s_stream.Read(buffer, 0, buffer.Length)) > 0) {
                                c_stream.Write(buffer, 0, length);
                                total -= length;
                            }
                            c_stream.Flush();
                        } catch(IOException e) {
                            warn(e.Message);
                            c_reader.Dispose();
                            s_writer.Dispose();
                            c_stream.Dispose();
                            s_stream.Dispose();
                        } catch(Exception e) {
                            error(e.Message + "\n" + e.StackTrace);
                            c_reader.Dispose();
                            s_writer.Dispose();
                            c_stream.Dispose();
                            s_stream.Dispose();
                        } finally {
                            c_reader.Close();
                            c_writer.Close();
                            s_reader.Close();
                            s_writer.Close();
                            c_stream.Close();
                            s_stream.Close();
                            c_reader.Dispose();
                            c_writer.Dispose();
                            s_reader.Dispose();
                            s_writer.Dispose();
                            c_stream.Dispose();
                            s_stream.Dispose();
                            log("线程" + Thread.CurrentThread.ManagedThreadId + "已退出");
                        }
                    }).Start();
                } catch(Exception e) {
                    error(e.Message + "\n" + e.StackTrace);
                    break;
                }
            }
        }

        static void log(string message) {
            Console.Out.WriteLine(DateTime.Now.ToString() + " " + message);
        }

        static void error(string message) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(DateTime.Now.ToString() + " " + message);
            Console.ResetColor();
        }

        static void warn(string message) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.Out.WriteLine(DateTime.Now.ToString() + " " + message);
            Console.ResetColor();
        }
    }
}
