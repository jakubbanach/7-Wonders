using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StanGry
{
    public bool CzyZakonczona { get; private set; }
    public TypZwyciestwa TypZwyciestwa { get; private set; }
    public Gracz? Zwyciezca { get; private set; }
    
    public StanGry()
    {
        CzyZakonczona = false;
        TypZwyciestwa = TypZwyciestwa.Brak;
        Zwyciezca = null;
    }

    public void CzyZwyciestwoMilitarne(Gracz[] gracze, int pozycjaPionu)
    {
        if (pozycjaPionu >= 9)
            ZakonczGre(gracze[0], TypZwyciestwa.Militarne);

        if (pozycjaPionu <= -9)
            ZakonczGre(gracze[1], TypZwyciestwa.Militarne);
    }

    public void CzyZwyciestwoNaukowe(Gracz[] gracze)
    {
        foreach (var gracz in gracze)
            if (gracz.SymboleNaukowe.Distinct().Count() >= 6)
            {
                ZakonczGre(gracz, TypZwyciestwa.Naukowe);
            }
    }

    public void ZakonczGre(Gracz zwyciezca, TypZwyciestwa typ)
    {
        CzyZakonczona = true;
        TypZwyciestwa = typ;
        Zwyciezca = zwyciezca;
    }
}
