namespace myNOC.Tests.Remootio;

[TestClass]
public class GateStateTests
{
    [DataTestMethod]
    [DataRow(true, "Open")]
    [DataRow(false, "Closed")]
    public void Description_ReflectsIsOpen(bool isOpen, string expected)
    {
        var state = new GateState { IsOpen = isOpen };
        Assert.AreEqual(expected, state.Description);
    }

    [TestMethod]
    public void IsOpen_True_DescriptionIsOpen()
    {
        var state = new GateState { IsOpen = true };
        Assert.IsTrue(state.IsOpen);
        Assert.AreEqual("Open", state.Description);
    }

    [TestMethod]
    public void IsOpen_False_DescriptionIsClosed()
    {
        var state = new GateState { IsOpen = false };
        Assert.IsFalse(state.IsOpen);
        Assert.AreEqual("Closed", state.Description);
    }
}
