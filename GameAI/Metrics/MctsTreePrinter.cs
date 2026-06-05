// MctsTreePrinter.cs — rozszerzenie o wizualizację piramidy

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class MctsTreePrinter
{
    private const int TopN = 3;

    public static void Print(MctsNode root, string tytul = "Drzewo MCTS")
    {
        Console.WriteLine();
        Console.WriteLine($"╔══ {tytul} ══ Iteracji w korzeniu: {root.Wizyty} ══╗");
        PrintNode(root, prefix: "", indentPrefix: "  ", isBest: true);
        Console.WriteLine();
    }

    static void PrintNode(MctsNode node, string prefix, string indentPrefix, bool isBest)
    {
        string ruchLabel = FormatRuch(node.Ruch);
        double winPct = node.Wizyty > 0 ? 100.0 * node.Wygrane / node.Wizyty : 0;
        string stats = node.Wizyty > 0
            ? $"[{node.Wizyty,5} wizyt | {winPct,5:F1}%]"
            : "[  --- nieodwiedzony ---  ]";
        string marker = isBest ? "* " : "  ";

        Console.WriteLine($"{prefix}{marker}{ruchLabel,-38} {stats}  {WinBar(winPct)}");

        // Piramida — pokazuj tylko dla węzłów z rodzicem (nie dla korzenia)
        // i tylko dla głębokości 1 żeby nie zaśmiecać (możesz zmienić warunek)
        if (node.Ruch != null && node.Rodzic != null)
            PrintPiramida(node, indentPrefix + "    ");
        
        if (!node.Dzieci.Any()) return;

        var topDzieci = node.Dzieci
            .OrderByDescending(c => c.Wizyty)
            .ThenByDescending(c => c.Wizyty > 0 ? c.Wygrane / c.Wizyty : 0)
            .Take(TopN)
            .ToList();

        var best = topDzieci.First();

        for (int i = 0; i < topDzieci.Count; i++)
        {
            bool isLast = (i == topDzieci.Count - 1);
            string connector = isLast ? "└─" : "├─";
            string childIndent = indentPrefix + (isLast ? "   " : "│  ");

            PrintNode(
                node: topDzieci[i],
                prefix: prefix + connector,
                indentPrefix: childIndent,
                isBest: topDzieci[i] == best
            );
        }

        int pominiete = node.Dzieci.Count - topDzieci.Count;
        if (pominiete > 0)
            Console.WriteLine($"{prefix}    ... (+{pominiete} innych)");
    }

    static void PrintPiramida(MctsNode node, string prefix)
    {
        // Stan planszy PRZED ruchem tego węzła = stan rodzica
        var gra = node.Rodzic!.Gra;

        // ← podmień na właściwą nazwę metody/pola w Gra
        var plansza = gra.PlanszaEpoki;
        if (plansza == null) return;

        var pola = plansza.Pola; // List<PoleKarty>

        // Pogrupuj po rzędach — zakładam że PoleKarty ma pole Rzad lub
        // że kolejność w liście odpowiada rzędom (1,2,3... od góry)
        // Jeśli masz pole Rzad/Row — zamień GroupBy na .Rzad
        var rzedy = PogrupujPoRzedach(pola, plansza);

        // Karta wybrana w tym węźle (żeby ją oznaczyć gwiazdką)
        string? wybranaKarta = node.Ruch?.KartaDoZagrania?.Nazwa;

        Console.WriteLine($"{prefix}┌{new string('─', 50)}┐");

        int offset = 0;
        foreach (var (rzad, numRzad) in rzedy.Select((r, i) => (r, i + 1)))
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{prefix}│ Rząd {numRzad}: ");

            for (int i = 0; i < rzad.Count; i++)
            {
                var pole = rzad[i];
                int globalnyIndex = offset + i;

                if (pole.Karta == null)
                {
                    sb.Append("[------] ");
                    continue;
                }

                bool dostepna = !pole.CzyZakryta && pole.BlokujacePola.All(b => b.Karta == null);
                bool wybrana = pole.Karta.Nazwa == wybranaKarta;
                bool zakryta = pole.CzyZakryta;
                string nazwaSkrocona = Skroc(pole.Karta.Nazwa, 10);

                if (zakryta)
                {
                    string zawartosc = "░░░░░░░░░░";

                    if (node.ZakrytePola != null
                        && node.ZakrytePola.TryGetValue(globalnyIndex, out var liczniki)
                        && liczniki.Count > 0)
                    {
                        int total = liczniki.Values.Sum();
                        var (nazwa, ile) = liczniki.OrderByDescending(kv => kv.Value).First();
                        int pct = (int)(100.0 * ile / total);
                        string skrot = Skroc(nazwa, 7);
                        zawartosc = $"~{skrot,-7}{pct,3}%";
                    }

                    sb.Append($"[{zawartosc}] ");
                }
                else if (wybrana)
                    sb.Append($"\x1b[33m[{nazwaSkrocona,-10}*]\x1b[0m ");
                else if (dostepna)
                    sb.Append($"\x1b[32m[{nazwaSkrocona,-10}]\x1b[0m ");
                else
                    sb.Append($"[{nazwaSkrocona,-10}] ");
            }

            Console.WriteLine(sb.ToString());
            offset += rzad.Count;
        }

        Console.WriteLine($"{prefix}└{new string('─', 50)}┘");
    }

    static List<List<PoleKarty>> PogrupujPoRzedach(List<PoleKarty> pola, PlanszaEpoki plansza)
    {
        // Rozmiary rzędów w 7WD Epoka I: 2,3,4,5,6 — Epoka II: 2,3,4,5,6
        // Epoka III jest inna — dostosuj jeśli trzeba
        int[] rozmiary = plansza.Epoka switch
        {
            Epoka.EpokaI => new[] { 6, 5, 4, 3, 2 },
            Epoka.EpokaII => new[] { 2, 3, 4, 5, 6 },
            Epoka.EpokaIII => new[] { 2, 3, 4, 2, 4, 3, 2 },
            _ => throw new ArgumentOutOfRangeException()
        };

        var wynik = new List<List<PoleKarty>>();
        int idx = 0;

        foreach (int r in rozmiary)
        {
            if (idx >= pola.Count) break;
            wynik.Add(pola.Skip(idx).Take(r).ToList());
            idx += r;
        }

        // Jeśli zostały jakieś pola poza standardowymi rzędami
        if (idx < pola.Count)
            wynik.Add(pola.Skip(idx).ToList());

        return wynik;
    }

    static string FormatRuch(Ruch? ruch)
    {
        if (ruch == null) return "(korzeń)";
        string karta = ruch.KartaDoZagrania?.Nazwa ?? "?";
        string typ = ruch.TypRuchu switch
        {
            TypRuchu.ZbudujKarte => "buduj",
            TypRuchu.OdrzucKarte => "odrzuc",
            TypRuchu.ZbudujCud => $"cud:{ruch.KartaCudu?.Nazwa ?? "?"}",
            _ => ruch.TypRuchu.ToString()
        };
        return $"[{typ}] {karta}";
    }

    static string Skroc(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";

    static string WinBar(double winPct)
    {
        int filled = Math.Clamp((int)Math.Round(winPct / 10.0), 0, 10);
        string color = winPct >= 60 ? "\x1b[32m" : winPct >= 40 ? "\x1b[33m" : "\x1b[31m";
        return $"{color}{new string('█', filled)}{new string('░', 10 - filled)}\x1b[0m";
    }
}