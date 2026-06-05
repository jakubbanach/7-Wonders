using System.Collections.Generic;
using System.Linq;
using System;

public class PlanszaEpoki
{
    public Epoka Epoka { get; set; }
    public List<PoleKarty> Pola { get; set; }

    public PlanszaEpoki(Epoka epoka)
    {
        Epoka = epoka;
        Pola = new List<PoleKarty>();
    }

    private PlanszaEpoki(PlanszaEpoki plansza)
    {
        Epoka = plansza.Epoka;
        Pola = ClonePola(plansza.Pola);
    }

    public PlanszaEpoki Clone()
    {
        return new PlanszaEpoki(this);
    }
    public void CopyFrom(PlanszaEpoki source)
    {
        Epoka = source.Epoka;
        Pola = ClonePola(source.Pola);
    }

    public IEnumerable<PoleKarty> DostepneKarty =>
        Pola.Where(p => p.CzyDostepna && p.Karta != null);

    public IEnumerator<PoleKarty> GetEnumerator()
    {
        return Pola.GetEnumerator();
    }

    public IEnumerable<PoleKarty> WidoczneKarty =>
        Pola.Where(p => !p.CzyZakryta);

    private IEnumerable<PoleKarty> ZakryteKarty =>
        Pola.Where(p => p.CzyZakryta);

    public void PotasujZakryteKarty(IRandom random)
    {
        var zakryte = ZakryteKarty.ToList();
        var kartyDoWylosowania = zakryte
            .Where(p => p.Karta != null)
            .Select(p => p.Karta)
            .Concat(PozostaleKartyEpoki(Epoka))
            .ToList();

        kartyDoWylosowania.Shuffle(random);

        for (int i = 0; i < zakryte.Count; i++)
        {
            zakryte[i].Karta = i < kartyDoWylosowania.Count ? kartyDoWylosowania[i] : null;
        }
    }

    private static List<Karta> PozostaleKartyEpoki(Epoka epoka)
    {
        var taliaKart = epoka switch
        {
            Epoka.EpokaI => ZbiorPlanszEpok.PozostaleKartyEpokiI.Select(k => k.Clone()).ToList(),
            Epoka.EpokaII => ZbiorPlanszEpok.PozostaleKartyEpokiII.Select(k => k.Clone()).ToList(),
            Epoka.EpokaIII => ZbiorPlanszEpok.PozostaleKartyEpokiIII.Select(k => k.Clone()).ToList(),
            _ => throw new ArgumentException("Nieznana epoka")
        };
        return taliaKart;
    }

    public void ZmienPlansze(PlanszaEpoki nowaPlansza)
    {
        Epoka = nowaPlansza.Epoka;
        Pola = ClonePola(nowaPlansza.Pola);
    }

    public PoleKarty ZnajdzPole(Karta karta)
    {
        return Pola.FirstOrDefault(p => p.Karta != null && p.Karta.Equals(karta));
    }

    public void UsunPole(PoleKarty pole)
    {
        pole.UsunKarte();
        foreach (var p in Pola)
        {
            if (p.BlokujacePola.Contains(pole) && p.CzyZakryta)
            {
                if (p.BlokujacePola.All(bp => bp.Karta == null))
                {
                    p.Odkryj();
                }
            }
        }
    }

    public int[] Uklad()
    {
        return Epoka switch
        {
            Epoka.EpokaI => new[] { 6, 5, 4, 3, 2 },
            Epoka.EpokaII => new[] { 2, 3, 4, 5, 6 },
            Epoka.EpokaIII => new[] { 2, 3, 4, 2, 4, 3, 2 },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string PlanszaDoStringa()
    {
        var wynik = new System.Text.StringBuilder();
        int maxSzerokosc = 11;
        int[] uklad = Uklad();
        int index = 0;
        foreach (int liczbaPol in uklad)
        {
            int szerokoscWiersza = liczbaPol * 2;
            int paddingLewy = (maxSzerokosc - szerokoscWiersza) / 2;
            wynik.Append(new string(' ', paddingLewy));
            for (int i = 0; i < liczbaPol; i++)
            {
                var pole = Pola[index++];
                string symbol =
                    pole.CzyDostepna ? "D" :
                    pole.CzyZakryta ? "X" :
                    "O";
                if (pole.Karta == null)
                {
                    symbol = "-";
                }
                wynik.Append(symbol + " ");
            }
            wynik.AppendLine();
        }

        wynik.AppendLine("[D] Dostepna | [X] Zakryta | [O] Odkryta | [-] Puste pole");
        return wynik.ToString();
    }

    private static List<PoleKarty> ClonePola(IReadOnlyList<PoleKarty> sourcePola)
    {
        var map = new Dictionary<PoleKarty, PoleKarty>(sourcePola.Count);
        var clonedPoles = new List<PoleKarty>(sourcePola.Count);

        for (int i = 0; i < sourcePola.Count; i++)
        {
            var clonedPole = sourcePola[i].Clone(map);
            clonedPoles.Add(clonedPole);
        }

        return clonedPoles;
    }
}