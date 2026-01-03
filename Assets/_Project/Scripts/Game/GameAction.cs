using System;
using System.Collections;

public class GameAction
{
    public string DebugName { get; }
    public Func<bool> IsValid { get; }
    public Func<IEnumerator> Execute { get; }



    public GameAction(
        string debugName,
        Func<bool> isValid,
        Func<IEnumerator> execute)
    {
        DebugName = debugName;
        IsValid = isValid;
        Execute = execute;
    }
}
