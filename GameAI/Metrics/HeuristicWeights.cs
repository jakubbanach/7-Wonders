using System;
using System.Collections.Generic;
using System.Text;

public class HeuristicWeights
{
    public double PunktyZwyciestwa { get; set; } = 3.0;
    public double Cuda { get; set; } = 3.0;
    public double Monety { get; set; } = 1.0;
    public double SymboleNaukowe { get; set; } = 4.0;
    public double Wojsko { get; set; } = 2.0;
    public double SurowceBrazowe { get; set; } // Wartość 1 jednostki drewna/gliny/kamienia
    public double SurowceSzare { get; set; }    // Wartość papirusu/szkła
    public double SynergiaGildii { get; set; }  // Jak bardzo AI ma "polować" na fioletowe karty
    public double MonopolBonus { get; set; }    // Bonus, gdy mamy surowiec, którego NIE ma przeciwnik
    public HeuristicWeights() { }

    public HeuristicWeights(double punktyZwyciestwa, double cuda, double monety, double symboleNaukowe, double wojsko,
                            double surowceBrazowe, double surowceSzare, double synergiaGildii, double monopolBonus)
    {
        PunktyZwyciestwa = punktyZwyciestwa;
        Cuda = cuda;
        Monety = monety;
        SymboleNaukowe = symboleNaukowe;
        Wojsko = wojsko;
        SurowceBrazowe = surowceBrazowe;
        SurowceSzare = surowceSzare;
        SynergiaGildii = synergiaGildii;
        MonopolBonus = monopolBonus;
    }
}
public static class HeuristicWeightPresets
{
    public static HeuristicWeights Balanced() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 10.0,
        Cuda = 5.0,
        Monety = 2.0,
        SymboleNaukowe = 6.0,
        Wojsko = 4.0,
        SurowceBrazowe = 1.0,
        SurowceSzare = 1.5,
        MonopolBonus = 2.0,
        SynergiaGildii = 3.0,
    };

    public static HeuristicWeights Military() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 8.0,
        Cuda = 2.0,
        Monety = 1.0,
        SymboleNaukowe = 2.0,
        Wojsko = 10.0,
        SurowceBrazowe = 0.5,
        SurowceSzare = 0.5,
        MonopolBonus = 1.0,
        SynergiaGildii = 1.0,
    };
    public static HeuristicWeights Personal() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 8.0,
        Cuda = 8.0,
        Monety = 3.0,
        SymboleNaukowe = 7.0,
        Wojsko = 5.0,
        SurowceBrazowe = 10.0,
        SurowceSzare = 10.0,
        MonopolBonus = 20.0,
        SynergiaGildii = 5.0,
    };
    public static HeuristicWeights GeneticInt() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 18,
        Cuda = 16,
        Monety = 6,
        SymboleNaukowe = -4,
        Wojsko = 2
    };
    public static HeuristicWeights GeneticDoubleOld() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 10,
        Cuda = 11.596767050492,
        Monety = 3.1600782452896596,
        SymboleNaukowe = 0,
        Wojsko = 18.522820105553983,
        SurowceBrazowe = 12.698871781909313,
        SurowceSzare = 8.97790353557929,
        SynergiaGildii = 0.5771097850879234,
        MonopolBonus = 5.932608522443385
    };
    public static HeuristicWeights GeneticDouble() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 10,
        Cuda = 11.424219292786072,
        Monety = 2.868570710471165,
        SymboleNaukowe = 3.0918379421773543,
        Wojsko = 10.472908335492438,
        SurowceBrazowe = 18.46504559715514,
        SurowceSzare = 10.936882555455375,
        SynergiaGildii = 11.692723024027758,
        MonopolBonus = 10.65803921765556
    };

}