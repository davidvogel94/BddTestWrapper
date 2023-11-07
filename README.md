# Simple BDD Test Wrapper

I wrote this for fun as a personal snippet that's - albeit simpler - analogous to other libraries such as BDDfy.

# Usage

## Setup
The `UnitTestBase` class should be inherited by the test class. `ITestOutputHelper` should be added to the test class constructor parameters and passed into the base constructor.

```cs
public class MyTestClass : UnitTestBase
{
    public MyTestClass(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        // foo
    }
}
```

## Writing tests

An example has been provided [HERE](./Example/UnitTest1.cs).

Tests can be written as follows:

```cs

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
```

## Output

The above example outputs the following in the test log:

```
--------------------------------------------------------------------------------
TEST :    It should do the thing
--------------------------------------------------------------------------------
   GIVEN the thing
       AND the other thing
   WHEN the thing is done to the thing
       AND the other thing is done to the other thing
   THEN the thing does the thing to the thing
       AND the other thing does the other thing to the other thing
 
--- PASS.
--------------------------------------------------------------------------------
```

Where tests fail, `FAIL` will be printed next to the relevant step.

