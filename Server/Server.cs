using OS4_p2;
using OS4_p2.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static OS4_p2.WinApi.Functions;

namespace Server
{
    class Server
    {
		static Menu mainMenu = new Menu("Server",
			   new IMenuItem[] {
				new MenuItem("Create named", CreatePipe),
				new MenuItem("Send ", SendMessage),
				new MenuItem("Disconnect", DisconnectPipe)
			   });

		static bool pipeCreated = false;
		static uint outBufSz = 512, inBufSz = 512;
		static IntPtr pipe;
		static NativeOverlapped overlapped = new NativeOverlapped();
		static IntPtr evt;
		unsafe static void CreatePipe()
		{
			evt = CreateEvent(IntPtr.Zero, false, false, "myevt");
			pipe = CreateNamedPipe("\\\\.\\pipe\\mypipe",
				(uint)PipeOpenModeFlags.PIPE_ACCESS_DUPLEX,
				(uint)PipeModeFlags.PIPE_TYPE_MESSAGE | (uint)PipeModeFlags.PIPE_READMODE_MESSAGE | (uint)PipeModeFlags.PIPE_WAIT,
				Constants.PIPE_UNLIMITED_INSTANCES,
				outBufSz,
				inBufSz,
				0,
				null);

			NativeOverlapped syncPipe = new NativeOverlapped() { EventHandle = evt };

			var res = ConnectNamedPipe(pipe, ref syncPipe);

			WaitForSingleObject(evt, Constants.INFINITE);

			if (res)
			{
				Console.WriteLine("Pipe created successfully");
				pipeCreated = true;
			}
			else
				Console.WriteLine($"Error creating pipe! Error {GetLastError()}");
		}

		static unsafe void SendMessage()
		{
			if (!pipeCreated)
			{
				Console.WriteLine("Please, create pipe");
				return;
			}

			Console.WriteLine("Input message:");
			string str = Console.ReadLine();
			var outputStr = Encoding.UTF8.GetBytes(str);
			Array.Resize(ref outputStr, (int)outBufSz);

			fixed (NativeOverlapped* o = &overlapped)
			{
				var res = WriteFile(pipe, outputStr, outBufSz, out uint written, o);
				if (res)
					Console.WriteLine("Message received successfully");
				else
					Console.WriteLine($"Error writing message. Error  {GetLastError()}");
			}
		}

		static void DisconnectPipe()
		{
			if (!pipeCreated)
			{
				Console.WriteLine("Please, create pipe ");
				return;
			}
			if (DisconnectNamedPipe(pipe))
				Console.WriteLine("Disconnected!");
			else
				Console.WriteLine($"Error disconnecting pipe! Error {GetLastError()}");
			pipeCreated = false;
		}
		static void Main(string[] args)
		{
			mainMenu.Select();
		}
	}
}
