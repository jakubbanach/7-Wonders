using System;
using System.Collections.Generic;
using System.Text;
public class MoveLog
{
    public string Agent;
    public Ruch Ruch;

    public MoveLog(string agent, Ruch ruch)
    {
        Agent = agent;
        Ruch = ruch;
    }
}