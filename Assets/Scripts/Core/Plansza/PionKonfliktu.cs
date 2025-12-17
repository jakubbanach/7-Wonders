
public class PionKonfliktu
{
    public const int MaksymalnaPozycja = 9;
    public int Pozycja { get; private set; }
    //public Strefa AktualnaStrefa { get; protected set; }

    public PionKonfliktu(int pozycja)
    {
        Pozycja = pozycja;
    }

    public void Przesun(int iloscPol)
    {
        Pozycja += iloscPol;
    }

    public int PobierzPozycje()
    {
        return Pozycja;
    }

    public bool CzyZwyciestwo()
    {
        return Pozycja >= MaksymalnaPozycja || Pozycja <= -MaksymalnaPozycja;
    }
}
