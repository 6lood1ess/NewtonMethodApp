using NewtonMethodApp.Models;

namespace NewtonMethodApp.Services {
  public class NewtonSolver : INewtonSolver {
    private const int MaxIterations = 10_000;

    private readonly FunctionEvaluator _evaluator;

    public NewtonSolver(FunctionEvaluator evaluator) {
      _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public NewtonResult Solve(NewtonParameters parameters) {
      if (parameters == null) throw new ArgumentNullException(nameof(parameters));
      if (parameters.A >= parameters.B) throw new ArgumentException("Левая граница должна быть меньше правой");
      if (parameters.Epsilon <= 0) throw new ArgumentException("Точность должна быть положительной");

      CheckContinuity(parameters.Function.Expression, parameters.A, parameters.B);

      string expr = parameters.Function.Expression;
      double sign = parameters.ExtremumType == ExtremumType.Minimum ? 1.0 : -1.0;

      // Начальное приближение - середина отрезка
      double x = (parameters.A + parameters.B) / 2.0;
      int n = 0;

      while (n < MaxIterations) {
        double f1 = _evaluator.EvaluateFirstDerivative(expr, x);
        double f2 = _evaluator.EvaluateSecondDerivative(expr, x);

        f1 *= sign;
        f2 *= sign;

        if (Math.Abs(f2) < 1e-12) {
          throw new InvalidOperationException(
            $"Вторая производная близка к нулю в точке x = {x:F6}. Метод Ньютона не может продолжить работу");
        }

        double nextX = x - f1 / f2;

        // Если метод ушел за [a, b], значит стационарной точки внутри нет
        double tolerance = 1e-5; // небольшой допуск на погрешность
        if (nextX < parameters.A - tolerance || nextX > parameters.B + tolerance) {
          throw new InvalidOperationException(
            $"Метод Ньютона вышел за пределы отрезка [a, b] (x = {nextX:F6}). " +
            "Вероятно, на заданном отрезке нет искомого экстремума");
        }

        if (Math.Abs(nextX - x) < parameters.Epsilon || Math.Abs(f1) < parameters.Epsilon) {
          x = nextX;
          break;
        }

        x = nextX;
        n++;
      }

      if (n >= MaxIterations) {
        throw new InvalidOperationException("Превышено максимальное число итераций. Метод Ньютона не сошелся");
      }

      // Убеждается, что найденная точка действительно является минимумом (f'' > 0) 
      // или максимумом (f'' < 0)
      double f2Final = _evaluator.EvaluateSecondDerivative(expr, x) * sign;
      if (f2Final <= 0) {
        string typeName = parameters.ExtremumType == ExtremumType.Minimum ? "минимумом" : "максимумом";
        throw new InvalidOperationException(
          $"Найдена стационарная точка x = {x:F6}, но она не является {typeName}. " +
          "Вторая производная имеет неверный знак (найдена точка перегиба или противоположный экстремум)");
      }

      var result = new NewtonResult {
        Extremum = x,
        FunctionValueAtExtremum = _evaluator.Evaluate(expr, x),
        ExtremumType = parameters.ExtremumType,
        IterationsCount = n
      };

      return result;
    }

    private void CheckContinuity(string expression, double a, double b) {
      const int SampleCount = 200;
      double step = (b - a) / SampleCount;

      for (int sampleIndex = 0; sampleIndex <= SampleCount; ++sampleIndex) {
        double x = a + sampleIndex * step;
        double y;

        try {
          y = _evaluator.Evaluate(expression, x);
        } catch (Exception ex) {
          throw new InvalidOperationException($"Функция не определена в точке x = {x:F6}: {ex.Message}", ex);
        }

        if (double.IsNaN(y) || double.IsInfinity(y)) {
          throw new InvalidOperationException($"Функция не определена в точке x = {x:F6} (значение = {y})");
        }
      }
    }
  }
}