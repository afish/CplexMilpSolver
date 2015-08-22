﻿using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;
using MilpManager.Abstraction;

namespace CplexMilpManager.Implementation
{
    public class CplexMilpSolver : BaseMilpSolver
    {
        public Cplex Cplex { get; }
        private const int BoundaryValue = int.MaxValue;
        private int _nameId;
        private readonly IDictionary<string, CplexVariable> _variables;

        public CplexMilpSolver(int integerWidth) : base(integerWidth)
        {
            Cplex = new Cplex();
            _variables = new Dictionary<string, CplexVariable>();
        }

        private INumExpr ToNumExpr(IVariable variable)
        {
            return ((CplexVariable) variable).Var;
        }

        public override IVariable SumVariables(IVariable first, IVariable second, Domain domain)
        {
            var firstValue = (first as CplexVariable).PrecomputedValue;
            var secondValue = (second as CplexVariable).PrecomputedValue;
            return new CplexVariable(this, domain, Cplex.Sum(new[] { ToNumExpr(first), ToNumExpr(second) }), GetVariableName(), firstValue + secondValue);
        }

        public override IVariable NegateVariable(IVariable variable, Domain domain)
        {
            var firstValue = (variable as CplexVariable).PrecomputedValue;
            return new CplexVariable(this, domain, Cplex.Negative(ToNumExpr(variable)), GetVariableName(), -firstValue);
        }

        public override IVariable MultiplyVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            var firstValue = (variable as CplexVariable).PrecomputedValue;
            var secondValue = (constant as CplexVariable).PrecomputedValue;
            return new CplexVariable(this, domain, Cplex.Prod(ToNumExpr(variable), ToNumExpr(constant)), GetVariableName(), firstValue * secondValue);
        }

        public override IVariable DivideVariableByConstant(IVariable variable, IVariable constant, Domain domain)
        {
            var firstValue = (variable as CplexVariable).PrecomputedValue;
            var secondValue = (constant as CplexVariable).PrecomputedValue;
            if (secondValue.HasValue)
                return new CplexVariable(this, domain, Cplex.Prod(ToNumExpr(variable), 1.0 / (constant as CplexVariable).PrecomputedValue.Value), GetVariableName(), firstValue / secondValue);

            throw new InvalidOperationException("Variable is not constant, cannot perform divison");
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
            if ((bound as CplexVariable).PrecomputedValue.HasValue)
            {
                (variable as CplexVariable).PrecomputedValue = (bound as CplexVariable).PrecomputedValue;
            }
            else
            {
                (bound as CplexVariable).PrecomputedValue = (variable as CplexVariable).PrecomputedValue;
            }
        }

        public override IVariable FromConstant(int value, Domain domain)
        {
            var intVar = Cplex.Constant(value);
            return new CplexVariable(this, domain, intVar, GetVariableName(), value);
        }

        public override IVariable FromConstant(double value, Domain domain)
        {
            var numVar = Cplex.Constant(value);
            return new CplexVariable(this, domain, numVar, GetVariableName(), value);
        }

        public override IVariable Create(string name, Domain domain)
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
            else
            {
                variable = Cplex.NumVar(-BoundaryValue, BoundaryValue, name);
            }

            Cplex.Add(variable);
            var result = new CplexVariable(this, domain, variable, GetVariableName());
            _variables[name] = result;
            return result;
        }

        public override IVariable CreateAnonymous(Domain domain)
        {
            return Create(GetVariableName(), domain);
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
            return _variables[name];
        }

        public override IVariable TryGetByName(string name)
        {
            CplexVariable variable;
            _variables.TryGetValue(name, out variable);
            return variable;
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

        private string GetVariableName()
        {
            return "x_" + (_nameId++);
        }
    }
}
