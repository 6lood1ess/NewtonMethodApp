using NewtonMethodApp.Models;
using System.Windows.Forms.DataVisualization.Charting;

namespace NewtonMethodApp.Charting {
  public interface IChartBuilder {
    void Build(Chart chart, string expression, double a, double b, double? extremum, ExtremumType extremumType);

    void DrawIterations(Chart chart, string expression, double a, double b, 
                        List<NewtonIteration> iterations, int currentStep);
  }
}