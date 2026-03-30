using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlanszaKonfliktu
{
    public PionKonfliktu PionKonfliktu { get; protected set; }
    public List<ZetonPostepu> ZetonyPostepu { get; protected set; }
    public List<Strefa> Strefy { get; protected set; }
    public Gracz[] Gracze { get; protected set; }
    private Strefa ObecnaStrefa;

    private List<(int start, int end, Strefa strefa)> _mapaStref;

    public PlanszaKonfliktu(PionKonfliktu pionKonfliktu, List<ZetonPostepu> zetonyPostepu, List<Strefa> strefy, Gracz[] gracze)
    {
        PionKonfliktu = pionKonfliktu;
        ZetonyPostepu = zetonyPostepu;
        Strefy = strefy;
        ObecnaStrefa = Strefy.Find(strefa => strefa.Nazwa == "Startowa") ?? throw new InvalidOperationException("Brak strefy startowej na planszy.");
        ZbudujMapeStref();
        Gracze = gracze;
    }

    private PlanszaKonfliktu(PlanszaKonfliktu plansza)
    {
        PionKonfliktu = plansza.PionKonfliktu.Clone();
        ZetonyPostepu = plansza.ZetonyPostepu;
        Strefy = plansza.Strefy.Select(s => s.Clone()).ToList();
        ObecnaStrefa = Strefy.First(s => s.Nazwa == plansza.ObecnaStrefa.Nazwa);
        ZbudujMapeStref();
        Gracze = plansza.Gracze.Select(g => g.Clone()).ToArray();
    }

    public PlanszaKonfliktu Clone()
    {
        return new PlanszaKonfliktu(this);
    }

    private void ZbudujMapeStref()
    {
        _mapaStref = new List<(int, int, Strefa)>();

        int current = -9;
        foreach (var strefa in Strefy)
        {
            int start = current;
            int end = current + strefa.LiczbaPol;
            current = end;

            _mapaStref.Add((start, end, strefa));
        }
    }

    public Strefa PobierzStrefeDlaPozycji(int pozycja)
    {
        return _mapaStref
            .First(m => pozycja >= m.start && pozycja < m.end)
            .strefa;
    }

    public void PrzesunPion(int ile, Gracz gracz)
    {
        int poprzednia = PionKonfliktu.PobierzPozycje();
        if (gracz == Gracze[0])
            PionKonfliktu.Przesun(ile);
        else
            PionKonfliktu.Przesun(-ile);
        //PionKonfliktu.Przesun(ile);

        var nowaStrefa = PobierzStrefeDlaPozycji(PionKonfliktu.PobierzPozycje());

        if (!nowaStrefa.CzyJuzUzyta && PionKonfliktu.PobierzPozycje() != poprzednia)
        {
            if (nowaStrefa.LiczbaTraconychMonet > 0)
            {
                Console.WriteLine($"{gracz.Nazwa} traci {nowaStrefa.LiczbaTraconychMonet} monet z powodu wejœcia na strefê {nowaStrefa.Nazwa}!");
                if (gracz == Gracze[0])
                    Gracze[1].DodajMonety(-nowaStrefa.LiczbaTraconychMonet); // Przekazujemy karê przeciwnikowi
                else
                    Gracze[0].DodajMonety(-nowaStrefa.LiczbaTraconychMonet); // Przekazujemy karê przeciwnikowi
            }

            nowaStrefa.UzyjStrefy();
            ObecnaStrefa = nowaStrefa;
        }
    }

    public string WypiszStan()
    {
        return $"Pion konfliktu na pozycji: {PionKonfliktu.PobierzPozycje()}\n" +
               $"Zetony postepu: {string.Join(", ", ZetonyPostepu.Select(z => z.Nazwa))}\n" +
               $"Strefy: {string.Join(", ", Strefy.Select(s => s.Nazwa))}";
    }

    public string WypiszTorKonfliktu()
    {
        var sb = new StringBuilder();

        for (int i = -9; i <= 9; i++)
        {
            if (i == PionKonfliktu.PobierzPozycje())
                sb.Append("[X]");
            else
                sb.Append("[]");
        }

        sb.AppendLine();
        sb.AppendLine(" G1 <---------------------------------> G2");

        return sb.ToString();
    }
}
