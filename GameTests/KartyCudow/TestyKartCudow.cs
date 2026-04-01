public class TestyKartCudow
{
    [Fact]
    public void Test_Inicjalizacja_Kart()
    {
        var kartyCudow = ZbiorKart.TaliaKartyCudow;
        Assert.Equal(12, kartyCudow.Count);
        Assert.Equal(12, kartyCudow.ToList().Count());
    }
    //[Fact]
    //public void Test_KartaCudu_BudowlaZSurowcow()
    //{
    //    var surowce = new Dictionary<Surowiec, int>
    //    {
    //        { Surowiec.Drewno, 2 },
    //        { Surowiec.Kamie�, 3 }
    //    };
    //    var efekt = new Efekt(
    //        TypEfektu.Surowiec, 
    //        new Dictionary<Surowiec, int>
    //        {
    //            { Surowiec.Monety, 3 }
    //        }, 
    //        symbolNaukowy : SymbolNaukowy.Globus
    //    );

    //    var kartaCudu = new KartaCudu("Wielki Mur", new Dictionary<Surowiec, int>
    //    {
    //        { Surowiec.Drewno, 2 },
    //        { Surowiec.Kamie�, 1 }
    //    }, new List<Efekt> { efekt });

    //    //kartaCudu.Zagraj(surowce);
    //    Assert.True(kartaCudu.CzyZagrana);
    //}
    [Fact]
    public void Test_KartaCudu_BrakSurowcow()
    {
        var surowce = new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 }
        };
        var efekt = new Efekt(
            TypEfektu.Surowiec, 
            new Dictionary<Surowiec, int>
            {
                { Surowiec.Monety, 3 }
            }, 
            symbolNaukowy : SymbolNaukowy.Globus
        );

        var kartaCudu = new KartaCudu("Wielki Mur", new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Kamień, 1 }
        }, new List<Efekt> { efekt });

        //kartaCudu.Zagraj(surowce);
        Assert.False(kartaCudu.CzyZagrana);
    }
    [Fact]
    public void Test_KartaCudu_BrakDopelnieniaMonet()
    {
        var surowce = new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Monety, 1 }
        };
        var efekt = new Efekt(
            TypEfektu.Surowiec, 
            new Dictionary<Surowiec, int>
            {
                { Surowiec.Monety, 3 }
            }, 
            symbolNaukowy : SymbolNaukowy.Globus
        );

        var kartaCudu = new KartaCudu("Wielki Mur", new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Kamień, 1 }
        }, new List<Efekt> { efekt });

        //kartaCudu.Zagraj(surowce);
        Assert.False(kartaCudu.CzyZagrana);

        // powrot do stanu cudu sprzed gry
        kartaCudu.OznaczJakoNiezagrana();
    }
    //[Fact]
    //public void Test_KartaCudu_DopelnienieMonet()
    //{
    //    var surowce = new Dictionary<Surowiec, int>
    //    {
    //        { Surowiec.Drewno, 2 },
    //        { Surowiec.Monety, 4 }
    //    };
    //    var efekt = new Efekt(
    //        TypEfektu.Surowiec, 
    //        new Dictionary<Surowiec, int>
    //        {
    //            { Surowiec.Monety, 3 }
    //        }, 
    //        symbolNaukowy : SymbolNaukowy.Globus
    //    );

    //    var kartaCudu = new KartaCudu("Wielki Mur", new Dictionary<Surowiec, int>
    //    {
    //        { Surowiec.Drewno, 2 },
    //        { Surowiec.Kamie�, 1 }
    //    }, new List<Efekt> { efekt });

    //    //kartaCudu.Zagraj(surowce);
    //    Assert.True(kartaCudu.CzyZagrana);
    //}
    [Fact]
    public void Test_KartaCudu_WypiszKoszt()
    {
        var efekt = new Efekt(
            TypEfektu.Surowiec, 
            new Dictionary<Surowiec, int>
            {
                { Surowiec.Monety, 3 }
            }, 
            symbolNaukowy : SymbolNaukowy.Globus
        );
        var kartaCudu = new KartaCudu("Wielki Mur", new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Kamień, 1 }
        }, new List<Efekt> { efekt });
        var kosztString = kartaCudu.WypiszKoszt();
        Assert.Equal("2xDrewno + 1xKamień", kosztString);
    }
}