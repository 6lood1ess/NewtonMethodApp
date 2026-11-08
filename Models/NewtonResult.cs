namespace NewtonMethodApp.Models {
  // Итог работы метода Ньютона
  public class NewtonResult {
    public double Extremum { get; set; }
    public double FunctionValueAtExtremum { get; set; }
    public ExtremumType ExtremumType { get; set; }
    public int IterationsCount { get; set; }
  }
}