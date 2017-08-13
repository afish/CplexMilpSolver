using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
	public class CplexMilpSolverSettings : MilpManagerSettings
	{
		public CplexMilpSolverSettings()
		{
			Cplex = new Cplex();
		}

		public Cplex Cplex { get; set; }
	}
}