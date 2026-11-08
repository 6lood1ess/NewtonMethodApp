using NewtonMethodApp.Models;

namespace NewtonMethodApp.Services {
  // Интерфейс решателя методом Ньютона
  public interface INewtonSolver {
    NewtonResult Solve(NewtonParameters parameters);
  }
}