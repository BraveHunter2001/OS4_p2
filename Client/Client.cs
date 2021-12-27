using OS4_p2;
using OS4_p2.WinApi;
using System;
using System.Text;
using System.Threading;
using static OS4_p2.WinApi.Functions;
namespace Client
{
	class Client
	{
		static Menu mainMenu = new Menu("Client",
			new IMenuItem[] {
				new MenuItem("Connect pipe", ConnectPipe),
				new MenuItem("Receive message", GetMessage),
				new MenuItem("Disconnect from pipe", DisconnectPipe)
			});

		static NativeOverlapped overlapped;
		
		static IntPtr pipe;
		static uint bufSz = 512;
		static unsafe IOCompletionCallback completionCallback;
		static bool connected = false;

		static unsafe void Callback(uint errCode, uint bytes, NativeOverlapped* ov)
		{
			Console.WriteLine("Data received!");
		}
		static unsafe void ConnectPipe()
		{
			if (completionCallback == null)
				completionCallback += Callback;

		

			pipe = CreateFile(@"\\.\pipe\mypipe",
				(uint)DesiredAccess.GENERIC_READ | (uint)DesiredAccess.GENERIC_WRITE,
				(uint)ShareMode.None,
				null,
				(uint)CreationDisposition.OPEN_EXISTING,
				(uint)PipeOpenModeFlags.FILE_FLAG_OVERLAPPED,
				IntPtr.Zero);
			connected = true;
		}
		static void GetMessage()
		{
			if (!connected)
			{
				Console.WriteLine("You need to connect pipe at first!");
				return;
			}
			
			byte[] buf = new byte[bufSz];
			var res = ReadFileEx(pipe, buf, bufSz, ref overlapped, completionCallback);
			SleepEx(Constants.INFINITE, true);
			if (res)
			{
				string str = Encoding.UTF8.GetString(buf);
				str = str.Trim('\0');
				Console.WriteLine(str);
			}
			else
				Console.WriteLine($"Error occured while reading!. Code: {GetLastError()}");
		}
		static void DisconnectPipe()
		{
			if (!connected)
			{
				Console.WriteLine("You need to connect pipe at first!");
				return;
			}
			if (CloseHandle(pipe))
				Console.WriteLine("Disconnection successful");
			else
				Console.WriteLine($"Error occured. Code: {GetLastError()}");
			connected = false;
		}

		static void Main(string[] args)
		{
			mainMenu.Select();
			
		}
	}
}
