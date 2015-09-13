using System;
using System.IO;
using ILOG.Concert;
using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
    public class CplexMilpSolver : BaseMilpSolver
    {
        public Cplex Cplex { get; }
        private const int BoundaryValue = int.MaxValue;

        public CplexMilpSolver(int integerWidth) : base(integerWidth)
        {
            Cplex = new Cplex();
        }

        private INumExpr ToNumExpr(IVariable variable)
        {
            return ((CplexVariable) variable).Var;
        }

        protected override IVariable InternalSumVariables(IVariable first, IVariable second, Domain domain)
        {
            return new CplexVariable(this, domain, Cplex.Sum(new[] { ToNumExpr(first), ToNumExpr(second) }), NewVariableName());
        }

        protected override IVariable InternalNegateVariable(IVariable variable, Domain domain)
        {
            return new CplexVariable(this, domain, Cplex.Negative(ToNumExpr(variable)), NewVariableName());
        }

        protected override IVariable InternalMultiplyVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            return new CplexVariable(this, domain, Cplex.Prod(ToNumExpr(variable), ToNumExpr(constant)), NewVariableName());
        }

        protected override IVariable InternalDivideVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            return new CplexVariable(this, domain, Cplex.Prod(ToNumExpr(variable), 1.0 / constant.ConstantValue.Value), NewVariableName());
        }

        public override void SetLessOrEqual(IVariable variable, IVariable bound)
        {
            Cplex.AddLe(ToNumExpr(variable), ToNumExpr(bound));
        }

        public override void SetGreaterOrEqual(IVariable variable, IVariable bound)
        {
            Cplex.AddGe(ToNumExpr(variable), ToNumExpr(bound));
        }

        public override void SetEqual(IVariable variable, IVariable bound)
        {
            Cplex.AddEq(ToNumExpr(variable), ToNumExpr(bound));
        }

        protected override IVariable InternalFromConstant(string name, int value, Domain domain)
        {
            var intVar = Cplex.Constant(value);
            return new CplexVariable(this, domain, intVar, name);
        }

        protected override IVariable InternalFromConstant(string name, double value, Domain domain)
        {
            var numVar = Cplex.Constant(value);
            return new CplexVariable(this, domain, numVar, name);
        }

        protected override IVariable InternalCreate(string name, Domain domain)
        {
            INumVar variable;
            if (domain == Domain.AnyConstantInteger || domain == Domain.AnyInteger)
            {
                variable = Cplex.IntVar(-BoundaryValue, BoundaryValue, name);
            }
            else if (domain == Domain.PositiveOrZeroConstantInteger || domain == Domain.PositiveOrZeroInteger)
            {
                variable = Cplex.IntVar(0, BoundaryValue, name);
            }
            else if (domain == Domain.BinaryConstantInteger || domain == Domain.BinaryInteger)
            {
                variable = Cplex.BoolVar(name);
            }
            else if (domain == Domain.PositiveOrZeroConstantReal || domain == Domain.PositiveOrZeroReal)
            {
                variable = Cplex.NumVar(0, double.PositiveInfinity);
            }
            else
            {
                variable = Cplex.NumVar(double.NegativeInfinity, double.PositiveInfinity, name);
            }

            Cplex.Add(variable);
            var result = new CplexVariable(this, domain, variable, name);
            return result;
        }

        public override void AddGoal(string name, IVariable operation)
        {
            Cplex.Add(Cplex.Maximize(ToNumExpr(operation)));
        }

        public override string GetGoalExpression(string name)
        {
            throw new NotImplementedException();
        }

        public override void SaveModelToFile(string modelPath)
        {
            Cplex.ExportModel(modelPath);
        }

        protected override object GetObjectsToSerialize()
        {
            return null;
        }

        protected override void InternalDeserialize(object o)
        {
        }

        protected override void InternalLoadModelFromFile(string modelPath)
        {
            throw new NotImplementedException();
        }

        public override void Solve()
        {
            Cplex.Solve();
        }

        public override double GetValue(IVariable variable)
        {
            return Cplex.GetValue(ToNumExpr(variable));
        }

        public override SolutionStatus GetStatus()
        {
            var status = Cplex.GetStatus();
            if (status == Cplex.Status.Optimal)
            {
                return SolutionStatus.Optimal;
            }

            if (status == Cplex.Status.Unbounded || status == Cplex.Status.InfeasibleOrUnbounded)
            {
                return SolutionStatus.Unbounded;
            }

            if (status == Cplex.Status.Infeasible)
            {
                return SolutionStatus.Infeasible;
            }

            if (status == Cplex.Status.Feasible || status == Cplex.Status.Bounded)
            {
                return SolutionStatus.Feasible;
            }

            return SolutionStatus.Unknown;
        }
    }
}
