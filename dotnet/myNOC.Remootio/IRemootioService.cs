namespace myNOC.Remootio;

public interface IRemootioService
{
    GateState? CurrentGateState { get; }
    bool IsAuthenticated { get; }
    event EventHandler<GateState>? GateStateChanged;
    event EventHandler<bool>? ConnectionChanged;
    void OpenGate();
    void CloseGate();
}
