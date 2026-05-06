using System;

public sealed class GamePolicyEncoding
{
    public float[] State { get; }
    public string[] ActionCatalog { get; }
    public float[] ActionMask { get; }

    public GamePolicyEncoding(
        float[] state,
        string[] actionCatalog,
        float[] actionMask)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        ActionCatalog = actionCatalog ?? throw new ArgumentNullException(nameof(actionCatalog));
        ActionMask = actionMask ?? throw new ArgumentNullException(nameof(actionMask));
    }
}

public sealed class DecisionEncoding
{
    public string DecisionType { get; }
    public string[] Options { get; }
    public float[] LegalMask { get; }
    public float[]? ChoiceMask { get; }

    public DecisionEncoding(
        string decisionType,
        string[] options,
        float[] legalMask,
        float[]? choiceMask)
    {
        DecisionType = decisionType ?? throw new ArgumentNullException(nameof(decisionType));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        LegalMask = legalMask ?? throw new ArgumentNullException(nameof(legalMask));
        ChoiceMask = choiceMask;
    }
}