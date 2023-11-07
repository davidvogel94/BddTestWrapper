namespace BDDTestWrapper;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The enum member is printed to the test output in fully upper case anyway")]
public enum BddTestBehaviourType
{
    GIVEN,
    WHEN,
    THEN,
    AND,
}

public interface IGivenBddTestAction
{
    IGivenBddTestAction And(string description, Action andActionDelegate);

    IWhenBddTestAction When(string description, Action whenActionDelegate);
}

public interface IWhenBddTestAction
{
    IWhenBddTestAction And(string description, Action whenActionDelegate);

    IThenBddTestAction Then(string description, Action thenActionDelegate);
}

public interface IThenBddTestAction
{
    IThenBddTestAction And(string description, Action thenActionDelegate);

    void Execute();
}

public abstract class BddTestAction
{
    private const int BaseIndentationLevel = 1;

    protected BddTestAction(BddTestBehaviourType type, BddTestAction? parentAction, string description, Action actionDelegate, ITestOutputHelper testOutputHelper)
    {
        this.TestOutputHelper = testOutputHelper;

        this.ParentAction = parentAction;
        this.CurrentActionType = type;

        if (type == BddTestBehaviourType.AND)
        {
            if (this.ParentAction?.CurrentActionType is null)
            {
                throw new InvalidOperationException("Cannot use AND without prior action.");
            }

            // Indent the AND under its parent
            this.OutputIndentationLevel = BaseIndentationLevel + 1;
        }
        else
        {
            this.OutputIndentationLevel = BaseIndentationLevel;
        }

        // Set the action delegate to call the parent and output the test stage description before calling the current step
        // (this goes up to the first step and they then subsequently all execute in order)
        this.ActionDelegate = () =>
        {
            var outputBuilder = new StringBuilder();
            try
            {
                this.ParentAction?.ActionDelegate();

                var indentation = new string('\t', this.OutputIndentationLevel);

                outputBuilder.Append($"{indentation}{type} {description}");
                actionDelegate();
            }
            catch (Exception ex)
            {
                this.Exceptions.Add(ex); // will be thrown later in ThenBddTestAction.Execute()
                outputBuilder.Append($"\t-- FAIL - {ex.GetType().Name}");
            }

            this.WriteOutputLine(outputBuilder.ToString());
        };
    }

    public List<Exception> Exceptions { get; } = new ();

    public BddTestAction? ParentAction { get; }

    protected Action ActionDelegate { get; }

    protected ITestOutputHelper TestOutputHelper { get; }

    protected string CurrentTestNameWithSpaces =>
        Regex.Replace(
            Regex.Replace(
                this.CurrentTest?.TestCase?.DisplayName ?? string.Empty,
                @"(\P{Ll})(\P{Ll}\p{Ll})",
                "$1 $2"),
            @"(\p{Ll})(\P{Ll})",
            "$1 $2");

    private int OutputIndentationLevel { get; }

    private BddTestBehaviourType CurrentActionType { get; }

    private ITest? CurrentTest => this.TestOutputHelper
        .GetType()
        .GetField("test", BindingFlags.Instance | BindingFlags.NonPublic)
        ?.GetValue(this.TestOutputHelper) as ITest;

    protected void WriteOutputLine(string text) => this.TestOutputHelper.WriteLine(text);
}

public sealed class GivenBddTestAction : BddTestAction, IGivenBddTestAction
{
    public GivenBddTestAction(BddTestAction? parentAction, string description, Action givenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(BddTestBehaviourType.GIVEN, parentAction, description, givenActionDelegate, testOutputHelper)
    {
    }

    private GivenBddTestAction(BddTestBehaviourType bddTestBehaviourType, BddTestAction? parent, string description, Action givenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(bddTestBehaviourType, parent, description, givenActionDelegate, testOutputHelper)
    {
    }

    public IGivenBddTestAction And(string description, Action andActionDelegate)
    {
        return new GivenBddTestAction(BddTestBehaviourType.AND, this, description, andActionDelegate, this.TestOutputHelper);
    }

    public IWhenBddTestAction When(string description, Action whenActionDelegate)
    {
        return new WhenBddTestAction(this, description, whenActionDelegate, this.TestOutputHelper);
    }
}

public sealed class WhenBddTestAction : BddTestAction, IWhenBddTestAction
{
    public WhenBddTestAction(BddTestAction? parent, string description, Action whenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(BddTestBehaviourType.WHEN, parent, description, whenActionDelegate, testOutputHelper)
    {
    }

    private WhenBddTestAction(BddTestBehaviourType bddTestBehaviourType, BddTestAction? parent, string description, Action whenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(bddTestBehaviourType, parent, description, whenActionDelegate, testOutputHelper)
    {
    }

    public IWhenBddTestAction And(string description, Action andActionDelegate)
    {
        return new WhenBddTestAction(BddTestBehaviourType.AND, this, description, andActionDelegate, this.TestOutputHelper);
    }

    public IThenBddTestAction Then(string description, Action thenActionDelegate)
    {
        return new ThenBddTestAction(this, description, thenActionDelegate, this.TestOutputHelper);
    }
}

public sealed class ThenBddTestAction : BddTestAction, IThenBddTestAction
{
    public ThenBddTestAction(BddTestAction? parent, string description, Action thenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(BddTestBehaviourType.THEN, parent, description, thenActionDelegate, testOutputHelper)
    {
    }

    private ThenBddTestAction(BddTestBehaviourType bddTestBehaviourType, BddTestAction? parent, string description, Action thenActionDelegate, ITestOutputHelper testOutputHelper)
        : base(bddTestBehaviourType, parent, description, thenActionDelegate, testOutputHelper)
    {
    }

    public IThenBddTestAction And(string description, Action andActionDelegate)
    {
        return new ThenBddTestAction(BddTestBehaviourType.AND, this, description, andActionDelegate, this.TestOutputHelper);
    }

    public void Execute()
    {
        var spacerLine = new string('-', 80);

        this.WriteOutputLine(spacerLine);
        this.WriteOutputLine($"TEST :\t{this.CurrentTestNameWithSpaces}");
        this.WriteOutputLine(spacerLine);

        try
        {
            this.ActionDelegate();

            var exceptionList = new List<Exception>(this.Exceptions);

            var parentAction = this.ParentAction;
            while (parentAction is not null)
            {
                exceptionList.AddRange(parentAction.Exceptions);
                parentAction = parentAction.ParentAction;
            }

            if (exceptionList.Any())
            {
                throw new AggregateException(exceptionList);
            }
        }
        catch (Exception ex)
        {
            this.WriteOutputLine($"\n--- FAIL: {ex.Message}");
            this.WriteOutputLine(spacerLine);
            throw;
        }

        this.WriteOutputLine("\n--- PASS.");
        this.WriteOutputLine(spacerLine);
    }
}
