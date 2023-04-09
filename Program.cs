// See https://aka.ms/new-console-template for more information
using System.Buffers;
var sync = new object();
var arraySize = 10;
using IMemoryOwner<int> owner = MemoryPool<int>.Shared.Rent(arraySize);

var intArray = GetRandomArray(arraySize);
var intArray2 = GetRandomArray(arraySize);

var memory = owner.Memory.Slice(0, arraySize);
var memory2 = owner.Memory.Slice(0, arraySize);
WriteToBuffer<int>(intArray, memory.Span);

DisplayBuffer<int>(memory.Span);

var task = WriteToBufferAsync<int>(intArray, memory, true);
var task2 = WriteToBufferAsync<int>(intArray2, memory);
Task.WaitAll(task, task2);

DisplayBuffer<int>(memory.Span);

var task3 = WriteToBufferAsyncNoLock<int>(intArray, memory2, true);
var task4 = WriteToBufferAsyncNoLock<int>(intArray2, memory2);
Task.WaitAll(task3, task4);

DisplayBuffer<int>(memory2.Span);
Console.ReadLine();


static void WriteToBuffer<T>(T[] values, Span<T> span) where T : struct
{

    for (int i = 0; i < values.Length; i++)
    {
        span[i] = values[i];
    }

}


static int[] GetRandomArray(int size)
{
    var array = new int[size];
    for (int i = 0; i < size; i++)
    {
        var rnd = Random.Shared.Next(10);
        array[i] = rnd;

        Console.WriteLine($"Случайное число - {rnd}");
    }
    Console.WriteLine($"Генерация закончена");
    return array;
}

unsafe Task WriteToBufferAsync<T>(T[] values, Memory<T> memory, bool onSleep = false) where T : struct
{
    var memoryHandle = memory.Pin();
    var x = (T*)memoryHandle.Pointer;

    return Task.Run(() =>
    {
        lock (sync)
        {
            for (int i = 0; i < values.Length; i++)
            {
                x[i] = values[i];
                Thread.Sleep(10);
                if (onSleep)
                {
                    Thread.Sleep(10);
                }
            }
        }
    });
}


unsafe Task WriteToBufferAsyncNoLock<T>(T[] values, Memory<T> memory, bool onSleep = false) where T : struct
{
    var memoryHandle = memory.Pin();

    var x = (T*)memoryHandle.Pointer;

    return Task.Run(() =>
    {
        for (int i = 0; i < values.Length; i++)
        {

            x[i] = values[i];
            Thread.Sleep(10);
            if (onSleep)
            {
                Thread.Sleep(10);
            }
        }
    });
}



static void DisplayBuffer<T>(ReadOnlySpan<T> span) where T : struct
{
    var str = string.Join(",", span.ToArray());
    Console.WriteLine($"Значения в буфере({typeof(T)}) - {str} ");
}


