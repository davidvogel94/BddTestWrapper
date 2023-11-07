namespace BDDTestWrapper;

using Shouldly;
using Xunit.Abstractions;

public abstract class UnitTestBase
{
    protected UnitTestBase(ITestOutputHelper testOutputHelper)
    {
        this.TestOutputHelper = testOutputHelper;
    }

    protected Exception? ThrownException { get; set; }

    private ITestOutputHelper TestOutputHelper { get; }

    /// <summary>
    /// Static method to initiate Behaviour-Driven-Development-style testing wrapper.
    /// i.e. by calling this.Given(...)
    /// </summary>
    /// <param name="description">Description of the first test arrangement step ("given a ...").</param>
    /// <param name="givenActionDelegate">Delegate action to execute for this test step.</param>
    /// <returns><see cref="BddTestAction"/> object from which subsequent test steps may be created (builder pattern).</returns>
    protected GivenBddTestAction Given(string description, Action givenActionDelegate)
    {
        return new GivenBddTestAction(null, description, givenActionDelegate, this.TestOutputHelper);
    }

    protected void ThenNoExceptionsAreThrown()
    {
        this.ThrownException.ShouldBeNull();
    }
}
