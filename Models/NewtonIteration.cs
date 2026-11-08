namespace NewtonMethodApp.Models {
  public class NewtonIteration {
    public int Step { get; set; }
    public double X { get; set; }
    public double Fx { get; set; }
    public double F1x { get; set; }
    public double F2x { get; set; }
  }
}