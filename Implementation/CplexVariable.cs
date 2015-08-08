using ILOG.Concert;
using MilpManager.Abstraction;

namespace CplexMilpSolver.Implementation
{
    class CplexVariable : IVariable
    {
        public CplexVariable(IMilpManager manager, Domain domain, INumExpr var, double? constantValue = null)
        {
            MilpManager = manager;
            Domain = domain;
            Var = var;
            ConstantValue = constantValue;
        }

        public IMilpManager MilpManager { get; }
        public Domain Domain { get; private set; }
        public INumExpr Var { get; set; }
        public double? ConstantValue { get; set; }
    }
}
