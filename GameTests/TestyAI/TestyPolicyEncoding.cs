using System;
using System.Linq;
using Xunit;

public class TestyPolicyEncoding
{
    private readonly IRandom random = new RandomAdapter(12345);

    [Fact]
    public void EncodePolicy_ShouldReturnActionAndWonderMasks()
    {
        var gra = Gra.StworzNowaGre(random: random);

        var encoding = GameStateEncoder.EncodePolicy(gra);

        Assert.Equal(ActionSpace.TotalPrimaryActions, encoding.ActionMask.Length);
        Assert.Equal(encoding.ActionMask.Length, encoding.ActionCatalog.Length);
        Assert.Contains(encoding.ActionMask, value => value == 1f);
        Assert.All(encoding.ActionMask, value => Assert.Contains(value, new[] { 0f, 1f }));
    }

    [Fact]
    public void EncodeState_ShouldReturnFixedLengthTensor()
    {
        var gra = Gra.StworzNowaGre(random: random);

        var state = GameStateEncoder.Encode(gra);

        Assert.Equal(ActionSpace.StateVectorSize, state.Length);
    }

    [Fact]
    public void EncodePolicy_ShouldMarkLegalMoveFromAvailableMoves()
    {
        var gra = Gra.StworzNowaGre(random: random);
        var encoding = GameStateEncoder.EncodePolicy(gra);
        var legalMove = gra.DostepneRuchy().First();

        var expectedIndex = FindExpectedActionIndex(gra, legalMove);

        Assert.True(expectedIndex >= 0);
        Assert.Equal(1f, encoding.ActionMask[expectedIndex]);
    }

    [Fact]
    public void EncodeDecisionLog_ShouldCreateOneHotChoiceMask()
    {
        var decisionLog = new DecisionLog
        {
            TypDecyzji = "WybierzZetonPostepu",
            Opcje = { "A", "B", "C" },
            Wybor = "B"
        };

        var encoding = GameStateEncoder.EncodeDecision(decisionLog);

        Assert.Equal(3, encoding.LegalMask.Length);
        Assert.Equal(new[] { 1f, 1f, 1f }, encoding.LegalMask);
        Assert.Equal(new[] { 0f, 1f, 0f }, encoding.ChoiceMask);
    }

    private static int FindExpectedActionIndex(Gra gra, Ruch ruch)
    {
        var slotIndex = gra.PlanszaEpoki.Pola
            .Select((pole, index) => new { pole, index })
            .FirstOrDefault(x => x.pole.Karta != null && (ReferenceEquals(x.pole.Karta, ruch.KartaDoZagrania) || string.Equals(x.pole.Karta.Nazwa, ruch.KartaDoZagrania.Nazwa, StringComparison.Ordinal)))
            ?.index ?? -1;

        if (slotIndex < 0)
        {
            return -1;
        }

        return ruch.TypRuchu switch
        {
            TypRuchu.ZbudujKarte => ActionSpace.GetActionIndex(slotIndex, TypRuchu.ZbudujKarte),
            TypRuchu.OdrzucKarte => ActionSpace.GetActionIndex(slotIndex, TypRuchu.OdrzucKarte),
            TypRuchu.ZbudujCud when ruch.KartaCudu != null =>
                ActionSpace.GetActionIndex(slotIndex, TypRuchu.ZbudujCud, ActionSpace.FindWonderIndex(gra.AktywnyGracz.KartyCudow, ruch.KartaCudu.Nazwa)),
            TypRuchu.ZbudujCud => ActionSpace.GetActionIndex(slotIndex, TypRuchu.ZbudujCud, 0),
            _ => -1
        };
    }
}