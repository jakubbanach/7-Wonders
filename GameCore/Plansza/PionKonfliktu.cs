
public class PionKonfliktu
{
    public const int MaksymalnaPozycja = 9;
    private int Pozycja { get; set; }
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
}
