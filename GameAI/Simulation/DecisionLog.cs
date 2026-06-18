using System;
using System.Collections.Generic;
public class DecisionLog
{
    public string TypDecyzji { get; set; } = null!;

    public List<string> Opcje { get; set; } = new List<string>();

    public string Wybor { get; set; } = null!;
    public float[]? State { get; set; }
    public float[]? LegalMask { get; set; }
    public float[]? ChoiceMask { get; set; }
}