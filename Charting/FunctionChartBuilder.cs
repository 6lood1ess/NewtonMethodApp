using System.Windows.Forms.DataVisualization.Charting;
using NewtonMethodApp.Models;
using NewtonMethodApp.Services;
using NewtonMethodApp.Theme;

namespace NewtonMethodApp.Charting {
  public class FunctionChartBuilder : IChartBuilder {
    private const int PointCount = 600;
    private const double YRangeFactor = 1.4;

    private static readonly Color MinimumColor = AppTheme.Success;
    private static readonly Color MaximumColor = Color.FromArgb(230, 140, 0);
    private static readonly Color BoundaryColor = Color.FromArgb(150, 150, 150);

    private readonly IFunctionEvaluator _evaluator;

    public FunctionChartBuilder(IFunctionEvaluator evaluator) {
      _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public void Build(Chart chart, string expression, double a, double b, double? extremum, ExtremumType extremumType) {
      if (chart == null) {
        throw new ArgumentNullException(nameof(chart));
      }

      if (a >= b) {
        throw new ArgumentException("a должно быть меньше b");
      }

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

          if (double.IsNaN(y) || double.IsInfinity(y)) {
            continue;
          }

          points.Add(new PointF((float)x, (float)y));

          if (y < yMin) yMin = y;
          if (y > yMax) yMax = y;

        } catch {
          // Пропуск точек, где функция не определена
        }
      }

      if (points.Count == 0) {
        return;
      }

      // Вычисление отступов по оси Y
      double spanY = yMax - yMin;
      if (spanY < 1e-9) {
        spanY = 1.0;
      }
      double paddingY = spanY * 0.15;
      double yLow = yMin - paddingY;
      double yHigh = yMax + paddingY;

      // Вычисление отступов по оси X,
      // чтобы крайние точки не прилипали к осям
      double spanX = b - a;
      double paddingX = spanX * 0.05;
      double xLow = a - paddingX;
      double xHigh = b + paddingX;

      // Настройка оси
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
      
      // Применение новых границ с отступами
      area.AxisX.Minimum = xLow;
      area.AxisX.Maximum = xHigh;
      area.AxisY.Minimum = yLow;
      area.AxisY.Maximum = yHigh;

      area.Position.Auto = false;
      area.Position = new ElementPosition(8, 8, 88, 84);
      area.InnerPlotPosition.Auto = false;
      area.InnerPlotPosition = new ElementPosition(14, 10, 84, 75);

      // Построение графика функции
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

        // Вертикальная пунктирная линия
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

        // Точка экстремума
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

        // Подпись к точке
        extremumSeries.Points[0].Label = $"x* = {mx:F4}";
        extremumSeries.Points[0].Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        extremumSeries.Points[0].LabelForeColor = highlightColor;
        extremumSeries.Points[0].LabelBackColor = Color.White;
        extremumSeries.Points[0].LabelBorderColor = highlightColor;
        extremumSeries.Points[0].LabelBorderDashStyle = ChartDashStyle.Solid;
        extremumSeries.Points[0].LabelBorderWidth = 1;
      }
    }

    // Отрисовка границ a и b
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