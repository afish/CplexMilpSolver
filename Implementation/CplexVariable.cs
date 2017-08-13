using System;
using System.Reflection;
using ILOG.Concert;
using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
	[Serializable]
	public class CplexVariable : ICplexVariable
	{
		[NonSerialized]
		private IMilpManager _milpManager;

		[NonSerialized]
		internal static readonly FieldInfo IndexField = typeof (CpxNumVar).GetField("_varIndex", BindingFlags.NonPublic | BindingFlags.Instance);

		[NonSerialized]
		internal static readonly FieldInfo CplexIField = typeof(CpxExtractable).GetField("_cplexi", BindingFlags.NonPublic | BindingFlags.Instance);

		public CplexVariable(IMilpManager manager, Domain domain, INumExpr var, string name)
		{
			_milpManager = manager;
			Domain = domain;
			Var = var;
			Name = name;
			if (Var is CpxNumVar)
			{
				// We need to store variable index in order to be able to deserialize the problem later
				Index = (CplexIndex) IndexField.GetValue(Var);
			}
		}

		public IMilpManager MilpManager
		{
			get { return _milpManager; }
			set { _milpManager = value; }
		}

		public Domain Domain { get; set; }
		public INumExpr Var { get; set; }
		public string Name { get; set; }
		public double? ConstantValue { get; set; }
		public string Expression { get; set; }
		public CplexIndex Index { get; set; }

		public override string ToString()
		{
			return $"[Name = {Name}, Domain = {Domain}, ConstantValue = {ConstantValue}, Var = {Var}";
		}
	}
}
