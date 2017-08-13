using ILOG.Concert;
using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
	public interface ICplexVariable : IVariable
	{
		INumExpr Var { get; }
		CplexIndex Index { get; }
	}
}