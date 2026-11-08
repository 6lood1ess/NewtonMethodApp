namespace NewtonMethodApp.Services {
  // Абстракция вычисления значения функции в точке
  public interface IFunctionEvaluator {
    double Evaluate(string expression, double x);
  }
}