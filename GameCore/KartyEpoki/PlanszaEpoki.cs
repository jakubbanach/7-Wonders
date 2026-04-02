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
        Pola = plansza.Pola.Select(p => p.Clone()).ToList();
    }

    public PlanszaEpoki Clone()
    {
        return new PlanszaEpoki(this);
    }

    public IEnumerable<PoleKarty> DostepneKarty =>
        Pola.Where(p => p.CzyDostepna && p.Karta != null);

    public IEnumerator<PoleKarty> GetEnumerator()
    {
        return Pola.GetEnumerator();
    }

    public IEnumerable<PoleKarty> WidoczneKarty =>
        Pola.Where(p => !p.CzyZakryta);

    public void ZmienPlansze(PlanszaEpoki nowaPlansza)
    {
        Epoka = nowaPlansza.Epoka;
        Pola = nowaPlansza.Pola.Select(p => p.Clone()).ToList();
    }

    public PoleKarty ZnajdzPole(Karta karta)
    {
        return Pola.FirstOrDefault(p => p.Karta == karta);
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

        wynik.AppendLine("[D] Dostêpna | [X] Zakryta | [O] Odkryta | [-] Puste pole");
        return wynik.ToString();
    }
}