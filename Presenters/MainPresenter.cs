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

    // Состояние пошагового просмотра
    private List<NewtonIteration> _currentIterations = new();
    private int _currentStep = 0;
    private string _lastExpression = string.Empty;
    private double _lastA, _lastB;

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
      
      _view.IterateForwardRequested += OnIterateForwardRequested;
      _view.IterateBackRequested += OnIterateBackRequested;
      _view.ResetIterationRequested += OnResetIterationRequested;
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

        // Сохранение траектории для пошагового просмотра
        _currentIterations = result.Iterations;
        _currentStep = _currentIterations.Count - 1;
        _lastExpression = _view.FunctionExpression;
        _lastA = a;
        _lastB = b;

        string extremumText =
          $"x* = {result.Extremum:F6}    f(x*) = {result.FunctionValueAtExtremum:F6}{Environment.NewLine}" +
          $"Итераций: {result.IterationsCount}";

        _view.ShowResult(extremumText, result.ExtremumType);
        
        // Построение графика с траекторией (на последнем шаге)
        _view.BuildChartWithIterations(_lastExpression, a, b, _currentIterations, _currentStep);
        _view.ShowIterationControls(_currentIterations.Count > 1);
        _view.ShowCurrentIteration(_currentStep, _currentIterations.Count - 1,
                                   _currentIterations[_currentStep].X,
                                   _currentIterations[_currentStep].Fx);

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
        // Сбрасывание траектории при простом построении графика
        _currentIterations = new List<NewtonIteration>();
        _view.ShowIterationControls(false);

        _view.BuildChart(_view.FunctionExpression, a, b, null, _view.SelectedExtremumType);

      } catch (Exception ex) {
        _view.ShowError($"Ошибка построения графика: {ex.Message}");
      }
    }

    private void OnClearRequested(object sender, EventArgs e) {
      _currentIterations = new List<NewtonIteration>();
      _view.ShowIterationControls(false);
      _view.ClearResult();
      _view.BuildChart(_view.FunctionExpression, 0, 1, null, _view.SelectedExtremumType);
    }

    private void OnIterateForwardRequested(object sender, EventArgs e) {
      if (_currentIterations.Count == 0) return;
      if (_currentStep < _currentIterations.Count - 1) {
        _currentStep++;
        _view.BuildChartWithIterations(_lastExpression, _lastA, _lastB, _currentIterations, _currentStep);
        _view.ShowCurrentIteration(_currentStep, _currentIterations.Count - 1,
                                   _currentIterations[_currentStep].X,
                                   _currentIterations[_currentStep].Fx);
      }
    }

    private void OnIterateBackRequested(object sender, EventArgs e) {
      if (_currentIterations.Count == 0) return;
      if (_currentStep > 0) {
        _currentStep--;
        _view.BuildChartWithIterations(_lastExpression, _lastA, _lastB, _currentIterations, _currentStep);
        _view.ShowCurrentIteration(_currentStep, _currentIterations.Count - 1,
                                   _currentIterations[_currentStep].X,
                                   _currentIterations[_currentStep].Fx);
      }
    }

    private void OnResetIterationRequested(object sender, EventArgs e) {
      if (_currentIterations.Count == 0) return;
      _currentStep = _currentIterations.Count - 1;
      _view.BuildChartWithIterations(_lastExpression, _lastA, _lastB, _currentIterations, _currentStep);
      _view.ShowCurrentIteration(_currentStep, _currentIterations.Count - 1,
                                 _currentIterations[_currentStep].X,
                                 _currentIterations[_currentStep].Fx);
    }
  }
}