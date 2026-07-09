namespace GateMonitor.Tests.Blazor;

/// <summary>
/// Tests for Home.razor — the gate status and control component.
/// bUnit is used for rendering; NSubstitute mocks IRemootioService.
/// GateImageUrl is intentionally left empty to prevent the image-refresh
/// timer from starting inside OnInitialized during tests.
/// </summary>
[TestClass]
public class HomeComponentTests
{
    private BunitContext _ctx = null!;
    private IRemootioService _service = null!;

    [TestInitialize]
    public void Init()
    {
        _ctx = new BunitContext();
        _service = Substitute.For<IRemootioService>();

        _ctx.Services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Remootio:GateImageUrl"] = string.Empty
                })
                .Build());

        _ctx.Services.AddSingleton(_service);
    }

    [TestCleanup]
    public void Cleanup() => _ctx.Dispose();

    [TestMethod]
    public void WhenNotAuthenticated_ShowsConnectingAlert()
    {
        _service.IsAuthenticated.Returns(false);

        var cut = _ctx.RenderComponent<Home>();

        cut.Find(".alert-info");   // throws AngleSharp.Dom.DomException if not found
    }

    [TestMethod]
    public void WhenAuthenticated_DoesNotShowConnectingAlert()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = false });

        var cut = _ctx.RenderComponent<Home>();

        Assert.AreEqual(0, cut.FindAll(".alert-info").Count);
    }

    [TestMethod]
    public void WhenAuthenticated_AndGateOpen_CloseButtonEnabled_OpenButtonDisabled()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = true });

        var cut = _ctx.RenderComponent<Home>();

        var closeBtn = cut.Find("button.btn-danger");
        var openBtn  = cut.Find("button.btn-success");

        Assert.IsFalse(closeBtn.HasAttribute("disabled"), "Close button should be enabled when open");
        Assert.IsTrue(openBtn.HasAttribute("disabled"),   "Open button should be disabled when already open");
    }

    [TestMethod]
    public void WhenAuthenticated_AndGateClosed_OpenButtonEnabled_CloseButtonDisabled()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = false });

        var cut = _ctx.RenderComponent<Home>();

        var openBtn  = cut.Find("button.btn-success");
        var closeBtn = cut.Find("button.btn-danger");

        Assert.IsFalse(openBtn.HasAttribute("disabled"),  "Open button should be enabled when closed");
        Assert.IsTrue(closeBtn.HasAttribute("disabled"),  "Close button should be disabled when already closed");
    }

    [TestMethod]
    public void WhenAuthenticated_GateStatusDisplaysDescription()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = false });

        var cut = _ctx.RenderComponent<Home>();

        Assert.IsTrue(
            cut.Find(".gate-status").TextContent.Contains("Closed"),
            "Gate status element should show 'Closed'");
    }

    [TestMethod]
    public void ClickingOpenGate_InvokesServiceOpenGate()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = false });

        var cut = _ctx.RenderComponent<Home>();
        cut.Find("button.btn-success").Click();

        _service.Received(1).OpenGate();
    }

    [TestMethod]
    public void ClickingCloseGate_InvokesServiceCloseGate()
    {
        _service.IsAuthenticated.Returns(true);
        _service.CurrentGateState.Returns(new GateState { IsOpen = true });

        var cut = _ctx.RenderComponent<Home>();
        cut.Find("button.btn-danger").Click();

        _service.Received(1).CloseGate();
    }
}
