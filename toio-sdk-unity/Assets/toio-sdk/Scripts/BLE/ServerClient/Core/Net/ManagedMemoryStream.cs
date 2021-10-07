using System;
using System.IO;

public class ManagedMemoryStream : IDisposable
{
    public MemoryStream memory;
    public int writeSize;
    public int readSize;

    public ManagedMemoryStream()
    {
        this.writeSize = 0;
        this.readSize = 0;
    }
    public ManagedMemoryStream(int capacity)
    {
        this.memory = new MemoryStream(capacity);
        this.writeSize = 0;
        this.readSize = 0;
    }
    public ManagedMemoryStream(byte[] buffer)
    {
        this.memory = new MemoryStream(buffer);
        this.writeSize = 0;
        this.readSize = 0;
    }

    public void Write(byte[] buffer, int size)
    {
        this.memory.Write(buffer, 0, size);
        this.writeSize += size;
    }

    public int Read(byte[] buffer, int size)
    {
        this.readSize += size;
        return this.memory.Read(buffer, 0, size);
    }

    public int ReadAll(byte[] buffer)
    {
        this.readSize = this.writeSize;
        this.memory.Position = 0;
        var r = this.memory.Read(buffer, 0, this.writeSize);
        if (r != this.writeSize) { UnityEngine.Debug.Log("error"); }
        return r;
    }

    public void Reset()
    {
        this.memory.Position = 0;
        this.writeSize = 0;
        this.readSize = 0;
    }

    public void Dispose()
    {
        this.memory.Dispose();
    }
}
