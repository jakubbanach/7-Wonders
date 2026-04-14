using System;
using System.Collections.Generic;
using System.Text;

public interface IDecisionResolver
{
    T Resolve<T>(Gra gra, DecyzjaKontekst<T> decyzja);
}
