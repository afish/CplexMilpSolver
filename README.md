# CplexMilpSolver
Basic implementation of MilpManager using CPLEX.


# What is it
This library implements MilpManager https://github.com/afish/MilpManager using CPLEX library. It handles creating the model and solving it. It doesn't support serializing model in file yet.

# How to use it
Add package to project. Add MilpManager package to project. Fix references to CPLEX (you need to have it installed). Next, you can create instance of CplexMilpSolver class and create models as described in https://github.com/afish/MilpManager .
