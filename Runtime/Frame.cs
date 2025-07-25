using VM.Core.Instructions;

namespace VM.Core;

public class Frame
{
    public int ReturnAddress { get; set; }
    public Dictionary<int, VmValue> Locals { get; set; } = new();
    public int ArgumentCount { get; set; }
}

public class FrameStack
{
    private readonly Stack<Frame> _stack = new();

    public void Push(Frame frame) => _stack.Push(frame);
    public Frame Pop() => _stack.Pop();
    public Frame Peek() => _stack.Peek();
    public void Clear() => _stack.Clear();
    public int Count => _stack.Count;

    public void Dump()
    {
        var i = 0;
        foreach (var frame in _stack)
        {
            Console.WriteLine($"Frame #{i++}: ReturnAddress={frame.ReturnAddress}, Locals={frame.Locals.Count}");
        }
    }
}