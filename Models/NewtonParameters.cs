namespace NewtonMethodApp.Models {
  // Входные параметры задачи поиска экстремума
  public class NewtonParameters {
    public FunctionDefinition Function { get; }
    public double A { get; }
    public double B { get; }
    public double Epsilon { get; }
    public ExtremumType ExtremumType { get; }

    public NewtonParameters(FunctionDefinition function, double a, double b, double epsilon, ExtremumType extremumType) {
      Function = function;
      A = a;
      B = b;
      Epsilon = epsilon;
      ExtremumType = extremumType;
    }
  }
}