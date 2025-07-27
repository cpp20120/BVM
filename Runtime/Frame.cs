using VM.Core.Instructions;

namespace VM.Core;
/// <summary>
/// Represents a single function call frame during virtual machine execution.
/// Stores the return address, local variables, and number of passed arguments.
/// </summary>
public class Frame
{
    /// <summary>
    /// The instruction pointer to return to after function call completes.
    /// </summary>
    public int ReturnAddress { get; set; }

    /// <summary>
    /// Map of local variable slots (indexed by variable index) to values.
    /// </summary>
    public Dictionary<int, VmValue> Locals { get; set; } = new();

    /// <summary>
    /// Number of arguments passed to the function.
    /// Used to track stack layout.
    /// </summary>
    public int ArgumentCount { get; set; }
}

/// <summary>
/// Manages the call stack for function execution.
/// Provides push/pop/peek access to individual frames.
/// </summary>
public class FrameStack
{
    private readonly Stack<Frame> _stack = new();

    /// <summary>
    /// Pushes a new frame onto the stack.
    /// </summary>
    public void Push(Frame frame) => _stack.Push(frame);

    /// <summary>
    /// Pops the top frame from the stack.
    /// </summary>
    public Frame Pop() => _stack.Pop();

    /// <summary>
    /// Returns the top frame without removing it.
    /// </summary>
    public Frame Peek() => _stack.Peek();

    /// <summary>
    /// Clears all frames from the stack.
    /// </summary>
    public void Clear() => _stack.Clear();

    /// <summary>
    /// Gets the number of frames currently on the stack.
    /// </summary>
    public int Count => _stack.Count;

    /// <summary>
    /// Dumps debug info about the current call stack.
    /// </summary>
    public void Dump()
    {
        var i = 0;
        foreach (var frame in _stack)
        {
            Console.WriteLine($"Frame #{i++}: ReturnAddress={frame.ReturnAddress}, Locals={frame.Locals.Count}");
        }
    }
}