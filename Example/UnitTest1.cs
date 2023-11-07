namespace BDDTestWrapper.Example;

using Xunit;
using Xunit.Abstractions;

public class UnitTest1 : UnitTestBase
{
    public UnitTest1(ITestOutputHelper testOutputHelper)
	    : base(testOutputHelper)
    {
        // foo
    }

    [Fact]
    public void ItShouldDoTheThing()
    {
        this
            .Given("the thing", () => {
                // foo
            })
            .And("the other thing", () => {
                // foo
            })
            .When("the thing is done to the thing", () => {
                // do the thing to the thing
            })
            .And("the other thing is done to the other thing", () => {
                // do the other thing to the other thing
            })
            .Then("the thing does the thing to the thing", () => {
                // check that the thing did the thing to the thing
            })
            .And("the other thing does the other thing to the other thing", () => {
                // check that the other thing did the other thing to the other thing
            })
            .Execute();
    }
}
