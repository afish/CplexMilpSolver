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

	    public CplexMilpSolverSettings(Cplex cplex)
	    {
	        Cplex = cplex;
	    }

		public Cplex Cplex { get; }
	}
}