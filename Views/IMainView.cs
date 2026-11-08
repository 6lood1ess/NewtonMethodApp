using NewtonMethodApp.Models;

namespace NewtonMethodApp.Views {
  // Абстракция главного окна
  public interface IMainView {
    string FunctionExpression { get; }
    string AText { get; }
    string BText { get; }
    string EpsilonText { get; }

    ExtremumType SelectedExtremumType { get; }

    void ShowResult(string extremumText, ExtremumType extremumType);
    void ClearResult();

    void BuildChart(string expression, double a, double b, double? extremum, ExtremumType extremumType);

    void ShowError(string message);
    void ShowInfo(string message);

    event EventHandler CalculateRequested;
    event EventHandler ClearRequested;
    event EventHandler BuildChartRequested;
  }
}