using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class MctsCloneTests
{
    private void AssertGamesEqual(Gra a, Gra b)
    {
        Assert.Equal(a.AktywnyGracz.Nazwa, b.AktywnyGracz.Nazwa);
        // Players
        Assert.Equal(a.Gracze.Length, b.Gracze.Length);
        for (int i = 0; i < a.Gracze.Length; i++)
        {
            var ga = a.Gracze[i];
            var gb = b.Gracze[i];
            Assert.Equal(ga.Nazwa, gb.Nazwa);
            Assert.Equal(ga.PunktyZwyciestwa, gb.PunktyZwyciestwa);
            Assert.Equal(ga.ZbudowaneKarty.Count, gb.ZbudowaneKarty.Count);
            Assert.Equal(ga.KartyCudow.Select(k=>k.Nazwa), gb.KartyCudow.Select(k=>k.Nazwa));
            Assert.Equal(ga.ZetonyPostepu.Select(z=>z.Nazwa), gb.ZetonyPostepu.Select(z=>z.Nazwa));
            // compare resources
            foreach (var key in Enum.GetValues<Surowiec>())
            {
                Assert.Equal(ga.Surowce.ContainsKey(key) ? ga.Surowce[key] : 0,
                             gb.Surowce.ContainsKey(key) ? gb.Surowce[key] : 0);
            }
        }

        // Plansza epoki
        Assert.Equal(a.PlanszaEpoki.Pola.Count, b.PlanszaEpoki.Pola.Count);
        for (int i = 0; i < a.PlanszaEpoki.Pola.Count; i++)
        {
            var pa = a.PlanszaEpoki.Pola[i];
            var pb = b.PlanszaEpoki.Pola[i];
            var na = pa.Karta == null ? null : pa.Karta.Nazwa;
            var nb = pb.Karta == null ? null : pb.Karta.Nazwa;
            Assert.Equal(na, nb);
            Assert.Equal(pa.CzyZakryta, pb.CzyZakryta);
        }

        // Plansza konfliktu basics
        Assert.Equal(a.PlanszaKonfliktu.PionKonfliktu.PobierzPozycje(), b.PlanszaKonfliktu.PionKonfliktu.PobierzPozycje());
        Assert.Equal(a.PlanszaKonfliktu.ZetonyPostepu.Select(z=>z.Nazwa), b.PlanszaKonfliktu.ZetonyPostepu.Select(z=>z.Nazwa));
    }

    private void AssertPlanszaTopologyEqual(PlanszaEpoki source, PlanszaEpoki clone)
    {
        Assert.Equal(source.Epoka, clone.Epoka);
        Assert.Equal(source.Pola.Count, clone.Pola.Count);

        for (int i = 0; i < source.Pola.Count; i++)
        {
            var sourcePole = source.Pola[i];
            var clonePole = clone.Pola[i];

            Assert.Equal(sourcePole.CzyZakryta, clonePole.CzyZakryta);
            Assert.Equal(sourcePole.Karta?.Nazwa, clonePole.Karta?.Nazwa);

            var sourceBlockerIndices = sourcePole.BlokujacePola.Select(p => source.Pola.IndexOf(p)).OrderBy(x => x).ToArray();
            var cloneBlockerIndices = clonePole.BlokujacePola.Select(p => clone.Pola.IndexOf(p)).OrderBy(x => x).ToArray();
            Assert.Equal(sourceBlockerIndices, cloneBlockerIndices);

            Assert.Equal(sourcePole.BlokujacePola.Count, clonePole.BlokujacePola.Count);
        }
    }

    [Fact]
    public void DeepEquality_Clone_vs_CopyFrom()
    {
        var rng = new RandomAdapter(12345);
        var g = Gra.StworzNowaGre("A", "B", rng);

        var source = g.Clone();
        var target = g.Clone();
        // mutate target a bit to ensure CopyFrom restores
        target.PlanszaEpoki.Pola[0].UsunKarte();

        target.CopyFrom(source);

        AssertGamesEqual(source, target);

        // CopyFrom must not alias the top-level mutable objects.
        Assert.False(object.ReferenceEquals(source, target));
        Assert.False(object.ReferenceEquals(source.PlanszaEpoki, target.PlanszaEpoki));
        Assert.False(object.ReferenceEquals(source.PlanszaKonfliktu, target.PlanszaKonfliktu));
        Assert.False(object.ReferenceEquals(source.StanGry, target.StanGry));

        for (int i = 0; i < source.Gracze.Length; i++)
        {
            Assert.False(object.ReferenceEquals(source.Gracze[i], target.Gracze[i]));
            Assert.False(object.ReferenceEquals(source.Gracze[i].KartyCudow, target.Gracze[i].KartyCudow));
            Assert.False(object.ReferenceEquals(source.Gracze[i].ZbudowaneKarty, target.Gracze[i].ZbudowaneKarty));
            Assert.False(object.ReferenceEquals(source.Gracze[i].Efekty, target.Gracze[i].Efekty));
            Assert.False(object.ReferenceEquals(source.Gracze[i].ZetonyPostepu, target.Gracze[i].ZetonyPostepu));
        }

        // Mutating the source afterwards must not change the copied target state.
        source.Gracze[0].DodajMonety(3);
        source.PlanszaEpoki.Pola[0].UsunKarte();
        Assert.NotEqual(source.Gracze[0].Surowce[Surowiec.Monety], target.Gracze[0].Surowce[Surowiec.Monety]);
        Assert.NotEqual(source.PlanszaEpoki.Pola[0].Karta == null, target.PlanszaEpoki.Pola[0].Karta == null);
    }

    [Fact]
    public void PlanszaEpoki_Clone_Preserves_BlockingTopology()
    {
        var rng = new RandomAdapter(12345);
        var game = Gra.StworzNowaGre("A", "B", rng);

        var source = game.PlanszaEpoki;
        var clone = source.Clone();

        AssertPlanszaTopologyEqual(source, clone);

        // Mutating the source should not affect the clone topology.
        var sourceHidden = source.Pola.FirstOrDefault(p => p.CzyZakryta && p.Karta != null);
        if (sourceHidden != null)
        {
            source.UsunPole(sourceHidden);
            AssertPlanszaTopologyEqual(clone, clone.Clone());
        }
    }

    [Fact]
    public void PlanszaEpoki_Clone_Creates_Distinct_References_For_Cards_And_Blockers()
    {
        var rng = new RandomAdapter(12345);
        var game = Gra.StworzNowaGre("A", "B", rng);

        var source = game.PlanszaEpoki;
        var clone = source.Clone();

        Assert.Equal(source.Pola.Count, clone.Pola.Count);

        for (int i = 0; i < source.Pola.Count; i++)
        {
            var sourcePole = source.Pola[i];
            var clonePole = clone.Pola[i];

            Assert.False(object.ReferenceEquals(sourcePole, clonePole));

            if (sourcePole.Karta != null && clonePole.Karta != null)
            {
                Assert.Equal(sourcePole.Karta.Nazwa, clonePole.Karta.Nazwa);
                Assert.False(object.ReferenceEquals(sourcePole.Karta, clonePole.Karta));
            }

            Assert.Equal(sourcePole.BlokujacePola.Count, clonePole.BlokujacePola.Count);
            for (int b = 0; b < sourcePole.BlokujacePola.Count; b++)
            {
                var sourceBlocker = sourcePole.BlokujacePola[b];
                var cloneBlocker = clonePole.BlokujacePola[b];

                Assert.False(object.ReferenceEquals(sourceBlocker, cloneBlocker));
                Assert.Equal(source.Pola.IndexOf(sourceBlocker), clone.Pola.IndexOf(cloneBlocker));

                if (sourceBlocker.Karta != null && cloneBlocker.Karta != null)
                {
                    Assert.Equal(sourceBlocker.Karta.Nazwa, cloneBlocker.Karta.Nazwa);
                    Assert.False(object.ReferenceEquals(sourceBlocker.Karta, cloneBlocker.Karta));
                }
            }
        }
    }

    [Fact]
    public void Aliasing_Check_In_Mcts_Tree()
    {
        var rng = new RandomAdapter(42);
        var g = Gra.StworzNowaGre("A", "B", rng);
        var rootGame = g.Clone();

        // Use reflection to access internal MctsNode
        Type mctsType = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            mctsType = Array.Find(asm.GetTypes(), t => t.Name == "MctsNode");
            if (mctsType != null) break;
        }
        Assert.NotNull(mctsType);

        var createMethod = mctsType.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(createMethod);

        var root = createMethod.Invoke(null, new object[] { rootGame, null, null });

        // create two children from different moves
        var moves = rootGame.DostepneRuchy().ToList();
        Assert.True(moves.Count >= 2, "Not enough moves to build test tree");
        var dzieciField = mctsType.GetField("Dzieci", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        var graField = mctsType.GetField("Gra", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(dzieciField);
        Assert.NotNull(graField);

        for (int i = 0; i < 2; i++)
        {
            var mv = moves[i];
            var newGame = rootGame.Clone();
            newGame.WykonajRuch(mv, new SimulationDecisionResolver(new RandomAgent(rng)), rng);
            var child = createMethod.Invoke(null, new object[] { newGame, root, mv });
            var dzieciList = (System.Collections.IList)dzieciField.GetValue(root);
            dzieciList.Add(child);
        }

        // collect nodes as objects
        var nodes = new System.Collections.Generic.List<object>();
        nodes.Add(root);
        var dzieciListRoot = (System.Collections.IList)dzieciField.GetValue(root);
        foreach (var c in dzieciListRoot) nodes.Add(c);

        // ensure no two nodes reference the same Gra instance
        for (int i = 0; i < nodes.Count; i++)
        for (int j = i + 1; j < nodes.Count; j++)
        {
            var graA = (Gra)graField.GetValue(nodes[i]);
            var graB = (Gra)graField.GetValue(nodes[j]);
            Assert.False(object.ReferenceEquals(graA, graB), "Nodes share same Gra instance");

            // ensure PoleKarty instances are distinct between game copies
            var polaA = graA.PlanszaEpoki.Pola;
            var polaB = graB.PlanszaEpoki.Pola;
            for (int pa = 0; pa < polaA.Count; pa++)
                for (int pb = 0; pb < polaB.Count; pb++)
                    Assert.False(object.ReferenceEquals(polaA[pa], polaB[pb]), "PoleKarty instance shared between nodes");
        }
    }

    [Fact]
    public void Simulate_DoesNot_Mutate_Input_Game()
    {
        var rng = new RandomAdapter(777);
        var agent = new MctsAgent(rng)
        {
            Iterations = 10,
            UseRootDeterminization = false,
            ReshuffleInRollout = true
        };

        var g = Gra.StworzNowaGre("A", "B", rng);
        var before = g.Clone();

        // call private Simulate via reflection
        var mi = typeof(MctsAgent).GetMethod("Simulate", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(mi);
        var result = mi.Invoke(agent, new object[] { g });

        Assert.NotNull(result);
        Assert.IsType<Gracz>(result);

        // after simulate, original game should be unchanged
        AssertGamesEqual(before, g);

        // and the original game must still be independent from the snapshot.
        Assert.False(object.ReferenceEquals(before, g));
        Assert.False(object.ReferenceEquals(before.PlanszaEpoki, g.PlanszaEpoki));
        Assert.False(object.ReferenceEquals(before.PlanszaKonfliktu, g.PlanszaKonfliktu));
    }
}
