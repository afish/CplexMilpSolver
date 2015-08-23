using ILOG.Concert;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
    class CplexVariable : IVariable
    {
        public CplexVariable(IMilpManager manager, Domain domain, INumExpr var, string name, double? precomputedValue = null)
        {
            MilpManager = manager;
            Domain = domain;
            Var = var;
            PrecomputedValue = precomputedValue;
            Name = name;
        }

        public IMilpManager MilpManager { get; }
        public Domain Domain { get; }
        public INumExpr Var { get; }
        public double? PrecomputedValue { get; set; }
        public string Name { get; }
    }
}
