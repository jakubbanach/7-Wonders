using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public interface IDecisionResolver
{
    Task<T> Resolve<T>(Gra gra, DecyzjaKontekst<T> decyzja);
}
