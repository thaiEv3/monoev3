using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using MonoBrickFirmware.Native;
using System.Net.Sockets;
namespace MonoBrickFirmware.Services
{
	public class WebServer : IDisposable
	{
		private const string webserPath = "/home/root/webserver/";
		private const string xspName = "xsp4.exe";
		private const string xspPath = "/usr/local/lib/mono/gac/xsp4/2.10.2.0__0738eb9f132ed756/"+xspName;
		private int port;
		private Thread serverThread;
		private void CheckWebServer(){
			Console.WriteLine("Check web server");
			if(!File.Exists(webserPath+xspName)){
				ProcessHelper.RunAndWaitForProcess ("cp",xspPath + " " + webserPath+xspName);
				Console.WriteLine("Copy XSP");
			}
			if(!AOTCompiling.IsFileCompiled(webserPath+xspName)){
				CompilingServer();
				Console.WriteLine("AOT compiling");
				AOTCompiling.Compile(webserPath+xspName);
			}
		}
		
		private void ServerThread ()
		{
			Console.WriteLine("Server thread");
			Process proc = new System.Diagnostics.Process ();
			proc.EnableRaisingEvents = false; 
			proc.StartInfo.FileName = "/usr/local/bin/mono";
			proc.StartInfo.Arguments = webserPath + xspName +" --port " + port;
			proc.Start();
			proc.WaitForExit ();
			Stopped();
			Console.WriteLine("Server thread end");
			
		}
		private bool ServerReady()
		{
			int attempts = 0;
			bool serverReady = false;
			Console.WriteLine("Server ready");
			while(attempts < 90 && !serverReady)// 90 second timeout
			{
				try
			    {
					  if(!serverThread.IsAlive)
						  return false;
					  new TcpClient("127.0.0.1",port);
					  serverReady = true;
			    }
			    catch
			    {
			    }
				Thread.Sleep(1000);
				attempts++;
			}
			return serverReady;
		}
		
		private bool LoadPage ()
		{
			int attemps = 0;
			bool loaded = false;
			while(attemps < 2 && !loaded){
				try {
					/*Console.WriteLine("Load webpage");
					WebClient client = new WebClient();
					client.DownloadString("http://127.0.0.1"+":"+port);
					loaded = true;*/
					/*HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(@"http://127.0.0.1:"+ port);
					myRequest.Method = "GET";
					WebResponse myResponse = myRequest.GetResponse();
					StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
					string result = sr.ReadToEnd();
					sr.Close();
					myResponse.Close(); //- See more at: http://www.tech-recipes.com/rx/1954/get_web_page_contents_in_code_with_csharp/#sthash.1xwHk1P3.dpuf
					loaded = true;*/
				} 
				catch(Exception e) 
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
				attemps++;
			}
			return true;
			//return loaded;
		}
		
		public Action CompilingServer = delegate {};
		public Action StartingServer = delegate {};
		public Action LoadingPage = delegate {};
		public Action Running = delegate {};
		public Action Stopped = delegate {};
		
		public WebServer (int port)
		{
			this.port = port;
		}
		
		
		public bool Start ()
		{
			if (!IsRunning()) 
			{
				try{
				  	CheckWebServer();
					StartingServer();//Action
					if(serverThread != null && serverThread.IsAlive)
						serverThread.Abort();//this should never happen
					serverThread = new Thread(ServerThread);
					serverThread.Start();
					if(!ServerReady()){
						Stop();
						return false;
					}
					System.Threading.Thread.Sleep(1000);
					LoadingPage();//Action
					if(!LoadPage()){
						Stop();
						return false;
					}
					Running();//Action
				}
				catch
				{
					return false;
				}
				return true;			
					
			}
			return false;
		}
		
		public bool Restart ()
		{
			Stop();
			Thread.Sleep(1000);
			return Start();
		}
		
		public void Stop ()
		{
			if (IsRunning ()) {
				ProcessHelper.KillProcess (xspName);
				if (serverThread != null)
					serverThread.Join ();
			}
		}
		
		public static bool IsRunning ()
		{
			return ProcessHelper.IsProcessRunning(xspName);
		}
		
		public void Dispose()
		{			
			ProcessHelper.KillProcess(xspName);
			GC.SuppressFinalize(this);	
		}
		
		~WebServer()
		{
			ProcessHelper.KillProcess(xspName);
		}
	}
}

