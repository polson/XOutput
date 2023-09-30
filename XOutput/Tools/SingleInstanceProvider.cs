using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Serilog;


namespace XOutput.Tools;

public class SingleInstanceProvider
{
    private const string MutexName = "XOutputRunningAlreadyMutex";
    private const string PipeName = "XOutputRunningAlreadyNamedPipe";
    private const string ShowCommand = "Show";
    private const string OkResponse = "OK";
    private const string ErrorResponse = "ERROR";
    private readonly Mutex mutex = new(false, MutexName);

    private Thread notifyThread;

    public event Action ShowEvent;

    public bool TryGetLock()
    {
        return mutex.WaitOne(0, false);
    }

    public void ReleaseLock()
    {
        mutex.ReleaseMutex();
    }

    public void Close()
    {
        mutex.Close();
    }

    public void StartNamedPipe()
    {
        notifyThread = new Thread(() => ReadPipe());
        notifyThread.IsBackground = true;
        notifyThread.Name = "XOutputRunningAlreadyNamedPipe reader";
        notifyThread.Start();
    }

    public void StopNamedPipe()
    {
        notifyThread?.Interrupt();
    }

    public bool Notify()
    {
        using (var client = new NamedPipeClientStream(PipeName))
        {
            client.Connect();
            var ss = new StreamString(client);
            ss.WriteString(ShowCommand);
            return ss.ReadString() == OkResponse;
        }
    }

    private void ReadPipe()
    {
        var running = true;
        while (running)
            using (var notifyWait = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1))
            {
                notifyWait.WaitForConnection();
                try
                {
                    var ss = new StreamString(notifyWait);
                    var command = ss.ReadString();
                    ss.WriteString(ProcessCommand(command));
                }
                catch (ThreadInterruptedException)
                {
                    running = false;
                }
                catch (IOException e)
                {
                    Log.Error(e, "Exception");
                }
            }
    }

    private string ProcessCommand(string request)
    {
        if (request == ShowCommand)
        {
            ShowEvent?.Invoke();
            return OkResponse;
        }

        return ErrorResponse;
    }
}

internal class StreamString
{
    private readonly Stream ioStream;
    private readonly UnicodeEncoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UnicodeEncoding();
    }

    public string ReadString()
    {
        int len;
        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        var inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);

        return streamEncoding.GetString(inBuffer);
    }

    public int WriteString(string outString)
    {
        var outBuffer = streamEncoding.GetBytes(outString);
        var len = outBuffer.Length;
        if (len > ushort.MaxValue) len = ushort.MaxValue;
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }
}