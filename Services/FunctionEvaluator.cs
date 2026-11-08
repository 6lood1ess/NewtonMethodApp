using NewtonMethodApp.Infrastructure;

namespace NewtonMethodApp.Services {
  public class FunctionEvaluator : IFunctionEvaluator {
    public double Evaluate(string expression, double x) {
      if (string.IsNullOrWhiteSpace(expression)) {
        throw new ArgumentException("Пустое выражение функции", nameof(expression));
      }

      try {
        var parser = new ExpressionParser(expression);
        return parser.Evaluate(x);

      } catch (Exception ex) {
        throw new FormatException($"Не удалось вычислить '{expression}': {ex.Message}", ex);
      }
    }

    // Первая производная (центральная разность)
    public double EvaluateFirstDerivative(string expression, double x, double h = 1e-6) {
      double fPlus = Evaluate(expression, x + h);
      double fMinus = Evaluate(expression, x - h);
      return (fPlus - fMinus) / (2 * h);
    }

    // Вторая производная (центральная разность)
    public double EvaluateSecondDerivative(string expression, double x, double h = 1e-5) {
      double fPlus = Evaluate(expression, x + h);
      double fMinus = Evaluate(expression, x - h);
      double f = Evaluate(expression, x);
      
      return (fPlus - 2 * f + fMinus) / (h * h);
    }
  }
}