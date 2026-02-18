public class Strefa
{
    public string Nazwa { get; }
    public int LiczbaPol { get; }
    public int LiczbaTraconychMonet { get; }
    public int LiczbaPunktow { get; }
    public bool CzyZajeta { get; protected set; }
    public bool CzyJuzUzyta { get; private set; }

    public Strefa(string nazwa, int liczbaPol, int liczbaTraconychMonet, int liczbaPunktow, bool czyZajeta, bool czyJuzUzyta)
    {
        Nazwa = nazwa;
        LiczbaPol = liczbaPol;
        LiczbaTraconychMonet = liczbaTraconychMonet;
        LiczbaPunktow = liczbaPunktow;
        CzyZajeta = czyZajeta;
        CzyJuzUzyta = czyJuzUzyta;
    }

    public void UzyjStrefy()
    {
        CzyJuzUzyta = true;
    }
}
