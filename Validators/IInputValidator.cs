using System.ComponentModel.DataAnnotations;

namespace NewtonMethodApp.Validators {
  // Проверка входных данных пользователя
  public interface IInputValidator {
    ValidationResult Validate(string functionExpr, string aText, string bText, string eText);
  }
}