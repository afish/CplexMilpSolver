using ILOG.Concert;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
    class CplexVariable : IVariable
    {
        public CplexVariable(IMilpManager manager, Domain domain, INumExpr var, string name)
        {
            MilpManager = manager;
            Domain = domain;
            Var = var;
            Name = name;
        }

        public IMilpManager MilpManager { get; }
        public Domain Domain { get; }
        public INumExpr Var { get; }
        public string Name { get; }
        public double? ConstantValue { get; set; }
    }
}
