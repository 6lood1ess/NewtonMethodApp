namespace NewtonMethodApp.Models {
  // Описание математической функции: строка-выражение, зависящая от x
  public class FunctionDefinition {
    public string Expression { get; }

    public FunctionDefinition(string expression) {
      Expression = expression ?? string.Empty;
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Expression);
  }
}