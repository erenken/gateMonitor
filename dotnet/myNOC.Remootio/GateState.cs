namespace myNOC.Remootio;

public sealed class GateState
{
    public bool IsOpen { get; init; }
    public string Description => IsOpen ? "Open" : "Closed";
}
