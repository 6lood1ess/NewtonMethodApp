namespace NewtonMethodApp.Validators {
  // Результат валидации (успех/ошибка + сообщение)
  public class ValidationResult {
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool valid, string message) {
      IsValid = valid;
      ErrorMessage = message;
    }

    public static ValidationResult Ok() => new ValidationResult(true, string.Empty);

    public static ValidationResult Fail(string message) => new ValidationResult(false, message);
  }
}