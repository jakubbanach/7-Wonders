using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

public static class GameDebugHelper
{
    public static void SaveVectorToFile(float[] vector, string filename)
    {

        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.Combine(projectDir, "EncoderResults");
        Directory.CreateDirectory(resultsDir);
        var fullPath = Path.Combine(resultsDir, filename);

        var content = string.Join("\n", vector.Select(v => v.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
        File.WriteAllText(fullPath, content);
    }
}

public class TestyEnkodera
{
    private readonly ITestOutputHelper _output;
    private readonly IRandom random = new RandomAdapter(12345);
    private const int IndexAktywnegoGracza = 0;
    private const int IndexPrzeciwnika = 1;
    // indeksy epok 2,3,4
    private const int IndexPozycjiKonfliktu = 5;
    private const int IndexKoncaGry = 6;
    // indeksy rodzajow zwyciestw 7,8,9,10
    // indeksy stref 3x9 11-37
    // indeksy zetonow postepu na planszy 38-47

    //playersi
    private const int IndexMonetAktywnegoGracza = 48;
    private const int IndexPunktyZwyciestwaAktywnegoGracza = 49;
    //surowce aktywnego gracza 50-54 - glina 50, kamien 51, drewno 52, szklo 53, papirus 54 (bez monet)
    //symbole naukowe aktywnego gracza 55-61 - globus 55, waga 56, zegar 57, mozdzierz 58, liczydlo 59, pismo 60, kolo 61
    //karty aktywnego gracza (73 karty) 62-134
    //cuda aktywnego gracza (12 kart x 2 cechy) 135-158
    //zetony postepu aktywnego gracza (10 zetonow) 159-168

    //przeciwnik
    private const int IndexMonetPrzeciwnika = 169;
    private const int IndexPunktyZwyciestwaPrzeciwnika = 170;
    //surowce przeciwnika 171-175 - glina 171, kamien 172, drewno 173, szklo 174, papirus 175 (bez monet)
    //symbole naukowe przeciwnika 176-182 - globus 176, waga 177, zegar 178, mozdzierz 179, liczydlo 180, pismo 181, kolo 182
    //karty przeciwnika (73 karty) 183-255
    //cuda przeciwnika (12 kart x 2 cechy) 256-279
    //zetony postepu przeciwnika (10 zetonow) 280-289

    //piramida
    //20 slotow x (4 cechy + 73 karty) 

    // karty odrzucone - 73 karty

    public TestyEnkodera(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Encode_ShouldCorrectlyNormalizeCoins()
    {
        var gra = Gra.StworzNowaGre(random: random);
        gra.AktywnyGracz.DodajMonety(3); // 10 przy MaxCoins = 20

        var vector = GameStateEncoder.Encode(gra);

        Assert.Equal(0.1f, vector[IndexMonetAktywnegoGracza]); // 10 monet ze 100 to 0.1
        Assert.Equal(0.07f, vector[IndexMonetPrzeciwnika]); // 7 monet ze 100 to 0.07
    }

    [Fact]
    public void Encode_ShouldCorrectlyAddResources()
    {
        var gra = Gra.StworzNowaGre(random: new RandomAdapter(1));
        var vector = GameStateEncoder.Encode(gra);

        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Skladowisko Kamienia").Clone();
        karta.OznaczJakoNiezagrana();

        var ruch = new Ruch(gra.AktywnyGracz, gra.Przeciwnik, karta!, TypRuchu.ZbudujKarte);

        gra.WykonajRuch(ruch, null, random);
        // po wykonaniu ruchu trzeba jeszcze zmienic ture, bo nastepuje automatyczne przejscie tury po wykonaniu ruchu
        gra.ZmienTure();
        var vectorPoRuchu = GameStateEncoder.Encode(gra);
        var IndexKamienAktywnegoGracza = 51; // indeks kamienia aktywnego gracza
        var IndexKamienPrzeciwnika = 172; // indeks kamienia przeciwnika

        GameDebugHelper.SaveVectorToFile(vector, "vector_state.txt");
        GameDebugHelper.SaveVectorToFile(vectorPoRuchu, "vector_state_after_move.txt");
        Assert.Equal(0.1f, vectorPoRuchu[IndexKamienAktywnegoGracza]); // 1 kamień ze 10 to 0.1
        Assert.Equal(0f, vectorPoRuchu[IndexKamienPrzeciwnika]); // kamień przeciwnika powinien być nadal 0
        Assert.Equal(0.07f, vector[IndexMonetAktywnegoGracza]); // 7 monet ze 100 to 0.07
        Assert.Equal(0.06f, vectorPoRuchu[IndexMonetAktywnegoGracza]); // 6 monet ze 100 to 0.06
    }
    [Fact]
    public void Encode_ShouldCorrectlyMoveConflict()
    {
        var gra = Gra.StworzNowaGre(random: random);
        var vector = GameStateEncoder.Encode(gra);

        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Wieza Straznicza").Clone();
        karta.OznaczJakoNiezagrana();

        var ruch = new Ruch(gra.AktywnyGracz, gra.Przeciwnik, karta!, TypRuchu.ZbudujKarte);

        gra.WykonajRuch(ruch, null, random);
        var vectorPoRuchu = GameStateEncoder.Encode(gra);

        Assert.Equal(10f/18f, vectorPoRuchu[IndexPozycjiKonfliktu]); 
        Assert.Equal(0.07f, vectorPoRuchu[IndexMonetAktywnegoGracza]); // 7 monet ze 100 to 0.07
        //_output.WriteLine($"Encoded vector: {string.Join("; ", vector)}");
        //_output.WriteLine($"Encoded vector: {string.Join("; ", vectorPoRuchu)}");
        GameDebugHelper.SaveVectorToFile(vector, "vector_state.txt");
        GameDebugHelper.SaveVectorToFile(vectorPoRuchu, "vector_state_after_move.txt");
    }

    [Fact]
    public void Encode_ShouldHaveSameLengthForSameState()
    {
        var gra1 = Gra.StworzNowaGre(random: random);
        var gra2 = Gra.StworzNowaGre(random: random);

        var vector1 = GameStateEncoder.Encode(gra1);
        var vector2 = GameStateEncoder.Encode(gra2);

        Assert.Equal(vector1.Length, vector2.Length);
        _output.WriteLine($"Encoded vector length: {vector1.Length}");
        _output.WriteLine($"Encoded vector: {string.Join("; ", vector1)}");
    }
}
