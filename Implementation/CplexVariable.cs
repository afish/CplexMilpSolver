using System;
using ILOG.Concert;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
    [Serializable]
    class CplexVariable : IVariable
    {
        [NonSerialized]
        private IMilpManager _milpManager;

        public CplexVariable(IMilpManager manager, Domain domain, INumExpr var, string name)
        {
            _milpManager = manager;
            Domain = domain;
            Var = var;
            Name = name;
        }

        public IMilpManager MilpManager
        {
            get { return _milpManager; }
            set { _milpManager = value; }
        }

        public Domain Domain { get; }
        public INumExpr Var { get; }
        public string Name { get; }
        public double? ConstantValue { get; set; }
        public override string ToString()
        {
            return $"[Name = {Name}, Domain = {Domain}, ConstantValue = {ConstantValue}, Var = {Var}";
        }
    }
}
