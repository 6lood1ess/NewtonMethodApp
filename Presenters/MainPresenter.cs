using NewtonMethodApp.Infrastructure;
using NewtonMethodApp.Models;
using NewtonMethodApp.Services;
using NewtonMethodApp.Validators;
using NewtonMethodApp.Views;

namespace NewtonMethodApp.Presenters {
  public class MainPresenter {
    private readonly IMainView _view;
    private readonly IInputValidator _validator;
    private readonly INewtonSolver _solver;

    public MainPresenter(
      IMainView view,
      IInputValidator validator,
      INewtonSolver solver) {

      _view = view ?? throw new ArgumentNullException(nameof(view));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
      _solver = solver ?? throw new ArgumentNullException(nameof(solver));

      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearRequested += OnClearRequested;
      _view.BuildChartRequested += OnBuildChartRequested;
    }

    private void OnCalculateRequested(object sender, EventArgs e) {
      var validation = _validator.Validate(
        _view.FunctionExpression,
        _view.AText,
        _view.BText,
        _view.EpsilonText);

      if (!validation.IsValid) {
        _view.ShowError(validation.ErrorMessage);
        return;
      }

      NumberParser.TryParse(_view.AText, out double a);
      NumberParser.TryParse(_view.BText, out double b);
      NumberParser.TryParse(_view.EpsilonText, out double eps);

      try {
        var parameters = new NewtonParameters(
          new FunctionDefinition(_view.FunctionExpression), a, b, eps, _view.SelectedExtremumType);

        var result = _solver.Solve(parameters);

        string extremumText =
          $"x* = {result.Extremum:F6}    f(x*) = {result.FunctionValueAtExtremum:F6}{Environment.NewLine}" +
          $"Итераций: {result.IterationsCount}";

        _view.ShowResult(extremumText, result.ExtremumType);
        _view.BuildChart(_view.FunctionExpression, a, b, result.Extremum, result.ExtremumType);

      } catch (Exception ex) {
        _view.ShowError(ex.Message);
      }
    }

    private void OnBuildChartRequested(object sender, EventArgs e) {
      var validation = _validator.Validate(
        _view.FunctionExpression,
        _view.AText,
        _view.BText,
        _view.EpsilonText);

      if (!validation.IsValid) {
        _view.ShowError(validation.ErrorMessage);
        return;
      }

      NumberParser.TryParse(_view.AText, out double a);
      NumberParser.TryParse(_view.BText, out double b);

      try {
        _view.BuildChart(_view.FunctionExpression, a, b, null, _view.SelectedExtremumType);

      } catch (Exception ex) {
        _view.ShowError($"Ошибка построения графика: {ex.Message}");
      }
    }

    private void OnClearRequested(object sender, EventArgs e) {
      _view.ClearResult();
      _view.BuildChart(_view.FunctionExpression, 0, 1, null, _view.SelectedExtremumType);
    }
  }
}