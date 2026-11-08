using System.Windows.Forms.DataVisualization.Charting;
using NewtonMethodApp.Models;
using NewtonMethodApp.Services;
using NewtonMethodApp.Theme;

namespace NewtonMethodApp.Charting {
  public class FunctionChartBuilder : IChartBuilder {
    private const int PointCount = 600;

    private static readonly Color MinimumColor = AppTheme.Success;
    private static readonly Color MaximumColor = Color.FromArgb(230, 140, 0);
    private static readonly Color BoundaryColor = Color.FromArgb(150, 150, 150);

    private readonly IFunctionEvaluator _evaluator;

    public FunctionChartBuilder(IFunctionEvaluator evaluator) {
      _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public void Build(Chart chart, string expression, double a, double b, double? extremum, ExtremumType extremumType) {
      if (chart == null) throw new ArgumentNullException(nameof(chart));
      if (a >= b) throw new ArgumentException("a должно быть меньше b");

      chart.Series.Clear();
      chart.Titles.Clear();

      if (chart.ChartAreas.Count == 0) {
        chart.ChartAreas.Add(new ChartArea("MainArea"));
      }

      var area = chart.ChartAreas[0];

      // Сброс границ для пересчёта
      area.AxisX.Minimum = double.NaN;
      area.AxisX.Maximum = double.NaN;
      area.AxisY.Minimum = double.NaN;
      area.AxisY.Maximum = double.NaN;
      area.RecalculateAxesScale();

      double step = (b - a) / PointCount;
      var points = new List<PointF>(PointCount + 1);

      double yMin = double.PositiveInfinity;
      double yMax = double.NegativeInfinity;

      for (int pointIndex = 0; pointIndex <= PointCount; ++pointIndex) {
        double x = a + pointIndex * step;

        try {
          double y = _evaluator.Evaluate(expression, x);

          if (double.IsNaN(y) || double.IsInfinity(y)) continue;

          points.Add(new PointF((float)x, (float)y));

          if (y < yMin) yMin = y;
          if (y > yMax) yMax = y;

        } catch {
          // Пропуск точек, где функция не определена
        }
      }

      if (points.Count == 0) return;

      double spanY = yMax - yMin;
      if (spanY < 1e-9) spanY = 1.0;
      double paddingY = spanY * 0.15;
      double yLow = yMin - paddingY;
      double yHigh = yMax + paddingY;

      double spanX = b - a;
      double paddingX = spanX * 0.05;
      double xLow = a - paddingX;
      double xHigh = b + paddingX;

      area.BackColor = AppTheme.Card;
      area.AxisX.Title = "x";
      area.AxisY.Title = "f(x)";
      area.AxisX.LineColor = AppTheme.Border;
      area.AxisY.LineColor = AppTheme.Border;
      area.AxisX.LabelStyle.ForeColor = AppTheme.TextSecondary;
      area.AxisY.LabelStyle.ForeColor = AppTheme.TextSecondary;
      area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8f);
      area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8f);
      area.AxisX.TitleForeColor = AppTheme.TextSecondary;
      area.AxisY.TitleForeColor = AppTheme.TextSecondary;
      area.AxisX.TitleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
      area.AxisY.TitleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
      area.AxisX.MajorGrid.LineColor = AppTheme.Border;
      area.AxisY.MajorGrid.LineColor = AppTheme.Border;
      area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
      area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
      area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
      area.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
      
      area.AxisX.Minimum = xLow;
      area.AxisX.Maximum = xHigh;
      area.AxisY.Minimum = yLow;
      area.AxisY.Maximum = yHigh;

      area.Position.Auto = false;
      area.Position = new ElementPosition(8, 8, 88, 84);
      area.InnerPlotPosition.Auto = false;
      area.InnerPlotPosition = new ElementPosition(14, 10, 84, 68);

      var functionSeries = new Series("Function") {
        ChartType = SeriesChartType.Line,
        BorderWidth = 2,
        Color = AppTheme.Accent,
        ChartArea = "MainArea",
        LegendText = "f(x)"
      };

      foreach (var p in points) {
        functionSeries.Points.AddXY(p.X, p.Y);
      }

      chart.Series.Add(functionSeries);

      // Отрисовка границ промежутка [a, b]
      DrawBoundary(chart, "BoundaryA", a, yLow, yHigh, $"a = {a:G}");
      DrawBoundary(chart, "BoundaryB", b, yLow, yHigh, $"b = {b:G}");

      // Отрисовка экстремума
      if (extremum.HasValue && extremum.Value >= a && extremum.Value <= b) {
        double mx = extremum.Value;
        double my = _evaluator.Evaluate(expression, mx);

        Color highlightColor = extremumType == ExtremumType.Minimum ? MinimumColor : MaximumColor;
        string markerLabel = extremumType == ExtremumType.Minimum ? "min" : "max";

        var guideSeries = new Series("ExtremumGuide") {
          ChartType = SeriesChartType.Line,
          BorderWidth = 1,
          Color = highlightColor,
          BorderDashStyle = ChartDashStyle.Dash,
          ChartArea = "MainArea",
          LegendText = "x*"
        };

        guideSeries.Points.AddXY(mx, yLow);
        guideSeries.Points.AddXY(mx, yHigh);
        chart.Series.Add(guideSeries);

        var extremumSeries = new Series("Extremum") {
          ChartType = SeriesChartType.Point,
          MarkerStyle = MarkerStyle.Circle,
          MarkerSize = 12,
          MarkerColor = highlightColor,
          MarkerBorderColor = Color.White,
          MarkerBorderWidth = 2,
          ChartArea = "MainArea",
          LegendText = $"{markerLabel}: x* = {mx:F4}"
        };

        extremumSeries.Points.AddXY(mx, my);
        chart.Series.Add(extremumSeries);

        extremumSeries.Points[0].Label = $"x* = {mx:F4}";
        extremumSeries.Points[0].Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        extremumSeries.Points[0].LabelForeColor = highlightColor;
        extremumSeries.Points[0].LabelBackColor = Color.White;
        extremumSeries.Points[0].LabelBorderColor = highlightColor;
        extremumSeries.Points[0].LabelBorderDashStyle = ChartDashStyle.Solid;
        extremumSeries.Points[0].LabelBorderWidth = 1;
      }
    }

    public void DrawIterations(Chart chart, string expression, double a, double b,
                               List<NewtonIteration> iterations, int currentStep) {
      if (chart == null || iterations == null || iterations.Count == 0) return;

      // Удаление старых серий траектории
      var toRemove = chart.Series
        .Where(s => s.Name.StartsWith("NewtonStep") || s.Name.StartsWith("NewtonTrail"))
        .ToList();
      foreach (var s in toRemove) chart.Series.Remove(s);

      var area = chart.ChartAreas[0];
      double yLow = area.AxisY.Minimum;
      double yHigh = area.AxisY.Maximum;

      // 1. Отрисовка линии траектории (все пройденные шаги)
      var trail = new Series("NewtonTrail") {
        ChartType = SeriesChartType.Line,
        BorderWidth = 2,
        Color = AppTheme.Sidebar,
        BorderDashStyle = ChartDashStyle.Solid,
        ChartArea = "MainArea",
        LegendText = "Путь"
      };

      for (int stepIndex = 0; stepIndex <= currentStep && stepIndex < iterations.Count; ++stepIndex) {
        trail.Points.AddXY(iterations[stepIndex].X, iterations[stepIndex].Fx);
      }
      chart.Series.Add(trail);

      // 2. Точки шагов
      for (int stepIndex = 0; stepIndex <= currentStep && stepIndex < iterations.Count; ++stepIndex) {
        var it = iterations[stepIndex];
        bool isCurrent = (stepIndex == currentStep);
        
        Color c = isCurrent 
            ? AppTheme.Accent 
            : Color.FromArgb(200, 80, 120, 200);
        
        int size = isCurrent ? 10 : 7; 

        var pointSeries = new Series($"NewtonStep{stepIndex}") {
          ChartType = SeriesChartType.Point,
          MarkerStyle = MarkerStyle.Circle,
          MarkerSize = size,
          MarkerColor = c,
          MarkerBorderColor = Color.White,
          MarkerBorderWidth = 1,
          ChartArea = "MainArea",
          IsVisibleInLegend = false
        };
        pointSeries.Points.AddXY(it.X, it.Fx);

        if (isCurrent) {
            pointSeries.Points[0].Label = $"x_{stepIndex}";
            pointSeries.Points[0].Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            pointSeries.Points[0].LabelForeColor = c;
            pointSeries.Points[0].LabelBackColor = Color.White;
            pointSeries.Points[0].LabelBorderColor = c;
            pointSeries.Points[0].LabelBorderDashStyle = ChartDashStyle.Solid;
            pointSeries.Points[0].LabelBorderWidth = 1;
        }

        chart.Series.Add(pointSeries);
      }

      // 3. Вертикальная линия от текущей точки до оси X
      if (currentStep < iterations.Count) {
        var current = iterations[currentStep];
        var guide = new Series("NewtonStepGuide") {
          ChartType = SeriesChartType.Line,
          BorderWidth = 1,
          Color = AppTheme.Accent,
          BorderDashStyle = ChartDashStyle.Dot,
          ChartArea = "MainArea",
          IsVisibleInLegend = false
        };
        guide.Points.AddXY(current.X, yLow);
        guide.Points.AddXY(current.X, current.Fx);
        chart.Series.Add(guide);
      }
    }

    private void DrawBoundary(Chart chart, string seriesName, double x, double yLow, double yHigh, string label) {
      var series = new Series(seriesName) {
        ChartType = SeriesChartType.Line,
        BorderWidth = 1,
        Color = BoundaryColor,
        BorderDashStyle = ChartDashStyle.Dash,
        ChartArea = "MainArea",
        LegendText = label
      };

      series.Points.AddXY(x, yLow);
      series.Points.AddXY(x, yHigh);
      
      series.Points[0].Label = label;
      series.Points[0].Font = new Font("Segoe UI", 8f, FontStyle.Regular);
      series.Points[0].LabelForeColor = BoundaryColor;

      chart.Series.Add(series);
    }
  }
}