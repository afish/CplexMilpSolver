using System;
using ILOG.Concert;
using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpSolver.Implementation
{
    public class CplexMilpSolver : BaseMilpSolver
    {
        private readonly Cplex _cplex;
        private const int BoundaryValue = Int32.MaxValue;
        private int _nameId;

        public CplexMilpSolver(int integerWidth) : base(integerWidth)
        {
            _cplex = new Cplex();
        }

        private INumExpr ToNumExpr(IVariable variable)
        {
            return ((CplexVariable) variable).Var;
        }

        public override IVariable SumVariables(IVariable first, IVariable second, Domain domain)
        {
            return new CplexVariable(this, domain, _cplex.Sum(new[] { ToNumExpr(first), ToNumExpr(second) }));
        }

        public override IVariable NegateVariable(IVariable variable, Domain domain)
        {
            return new CplexVariable(this, domain, _cplex.Negative(ToNumExpr(variable)));
        }

        public override IVariable MultiplyVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            return new CplexVariable(this, Domain.AnyInteger, _cplex.Prod(ToNumExpr(variable), ToNumExpr(constant)));
        }

        public override IVariable DivideVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            var constantValue = ((CplexVariable)constant).ConstantValue;
            if (constantValue != null)
                return new CplexVariable(this, domain, _cplex.Prod(ToNumExpr(variable), 1.0 / constantValue.Value));

            throw new InvalidOperationException("Variable is not constant, cannot perform divison");
        }

        public override void SetLessOrEqual(IVariable variable, IVariable bound)
        {
            _cplex.AddLe(ToNumExpr(variable), ToNumExpr(bound));
        }

        public override void SetGreaterOrEqual(IVariable variable, IVariable bound)
        {
            _cplex.AddGe(ToNumExpr(variable), ToNumExpr(bound));
        }

        public override void SetEqual(IVariable variable, IVariable bound)
        {
            _cplex.AddEq(ToNumExpr(variable), ToNumExpr(bound));
        }

        public override IVariable FromConstant(int value, Domain domain)
        {
            var intVar = _cplex.Constant(value);
            return new CplexVariable(this, domain, intVar, value);
        }

        public override IVariable FromConstant(double value, Domain domain)
        {
            var numVar = _cplex.Constant(value);
            return new CplexVariable(this, domain, numVar, value);
        }

        public override IVariable Create(string name, Domain domain)
        {
            return CreateAnonymous(domain);
        }

        public override IVariable CreateAnonymous(Domain domain)
        {
            INumVar variable;
            if (domain == Domain.AnyConstantInteger || domain == Domain.AnyInteger)
            {
                variable = _cplex.IntVar(-BoundaryValue, BoundaryValue, GetVariableName());
            }
            else if (domain == Domain.PositiveOrZeroConstantInteger || domain == Domain.PositiveOrZeroInteger)
            {
                variable = _cplex.IntVar(0, BoundaryValue, GetVariableName());
            }
            else if (domain == Domain.BinaryConstantInteger || domain == Domain.BinaryInteger)
            {
                variable = _cplex.BoolVar(GetVariableName());
            }
            else
            {
                variable = _cplex.NumVar(-BoundaryValue, BoundaryValue, GetVariableName());
            }

            _cplex.Add(variable);
            return new CplexVariable(this, domain, variable);
        }

        public override void AddGoal(string name, IVariable operation)
        {
            _cplex.Add(_cplex.Maximize(ToNumExpr(operation)));
        }

        public override string GetGoalExpression(string name)
        {
            throw new NotImplementedException();
        }

        public override void SaveModelToFile(string modelPath)
        {
            _cplex.ExportModel(modelPath);
        }

        public override void LoadModelFromFile(string modelPath, string solverDataPath)
        {
            throw new NotImplementedException();
        }

        public override void SaveSolverDataToFile(string solverOutput)
        {
            throw new NotImplementedException();
        }

        public override IVariable GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public override IVariable TryGetByName(string name)
        {
            throw new NotImplementedException();
        }

        public override void Solve()
        {
            _cplex.Solve();
        }

        public override double GetValue(IVariable variable)
        {
            return _cplex.GetValue(ToNumExpr(variable));
        }

        public override SolutionStatus GetStatus()
        {
            var status = _cplex.GetStatus();
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

        private string GetVariableName()
        {
            return "x_" + (_nameId++);
        }
    }
}
