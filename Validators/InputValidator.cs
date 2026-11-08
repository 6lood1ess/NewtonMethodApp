using NewtonMethodApp.Infrastructure;
using NewtonMethodApp.Services;

namespace NewtonMethodApp.Validators {
  public class InputValidator : IInputValidator {
    public ValidationResult Validate(string functionExpr, string aText, string bText, string eText) {
      if (string.IsNullOrWhiteSpace(functionExpr)) {
        return ValidationResult.Fail("Введите функцию f(x)");
      }

      var evaluator = new FunctionEvaluator();

      try {
        var parser = new ExpressionParser(functionExpr);
        parser.Evaluate(1.0);
      } catch (Exception ex) {
        return ValidationResult.Fail($"Некорректная формула: {ex.Message}");
      }

      if (!NumberParser.TryParse(aText, out double a)) {
        return ValidationResult.Fail("Некорректное значение a");
      }

      if (!NumberParser.TryParse(bText, out double b)) {
        return ValidationResult.Fail("Некорректное значение b");
      }

      if (!NumberParser.TryParse(eText, out double e)) {
        return ValidationResult.Fail("Некорректное значение e");
      }

      if (a >= b) {
        return ValidationResult.Fail("Значение a должно быть меньше b");
      }

      if (e <= 0) {
        return ValidationResult.Fail("Точность e должна быть положительной");
      }

      // Проверка, что функция не является линейной на [a, b]
      const int sampleCount = 20;
      double step = (b - a) / sampleCount;
      bool hasNonZeroCurvature = false;

      for (int i = 0; i <= sampleCount; ++i) {
        double x = a + i * step;
        try {
          double f2 = evaluator.EvaluateSecondDerivative(functionExpr, x);
          if (Math.Abs(f2) > 1e-6) { // Если есть кривизна
            hasNonZeroCurvature = true;
            break;
          }
        } catch {
          // Игнорирование ошибок в отдельных точках, CheckContinuity их поймает
        }
      }

      if (!hasNonZeroCurvature) {
        return ValidationResult.Fail(
          "Функция линейна (вторая производная равна нулю на всём отрезке). " +
          "Метод Ньютона неприменим");
      }

      return ValidationResult.Ok();
    }
  }
}