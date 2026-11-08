using System.Windows.Forms.DataVisualization.Charting;
using NewtonMethodApp.Charting;
using NewtonMethodApp.Models;
using NewtonMethodApp.Presenters;
using NewtonMethodApp.Services;
using NewtonMethodApp.Theme;
using NewtonMethodApp.Validators;
using NewtonMethodApp.Views;
using GdiFont = System.Drawing.Font;

namespace NewtonMethodApp.Forms {
  public class MainForm : Form, IMainView {
    private Panel _sidebar;
    private Panel _workspace;
    private Panel _chartCard;
    private Panel _resultCard;
    private Panel _iterationPanel;

    private TextBox _txtFunction;
    private TextBox _txtA;
    private TextBox _txtB;
    private TextBox _txtE;

    private RadioButton _rbMinimum;
    private RadioButton _rbMaximum;

    private Chart _chart;
    private Label _lblResultHeader;
    private Label _lblResultValue;

    private Button _btnCalculate;
    private Button _btnClear;
    private Button _btnBuild;

    private Button _btnPrevStep;
    private Button _btnNextStep;
    private Button _btnResetStep;
    private Label _lblStepInfo;

    private readonly IChartBuilder _chartBuilder;
    private readonly MainPresenter _presenter;

    public MainForm() {
      BuildUi();

      var evaluator = new FunctionEvaluator();
      var validator = new InputValidator();
      var solver = new NewtonSolver(evaluator);
      _chartBuilder = new FunctionChartBuilder(evaluator);

      _presenter = new MainPresenter(this, validator, solver);
    }

    public string FunctionExpression => _txtFunction.Text;
    public string AText => _txtA.Text;
    public string BText => _txtB.Text;
    public string EpsilonText => _txtE.Text;

    public ExtremumType SelectedExtremumType =>
      _rbMaximum.Checked ? ExtremumType.Maximum : ExtremumType.Minimum;

    public event EventHandler CalculateRequested;
    public event EventHandler ClearRequested;
    public event EventHandler BuildChartRequested;
    public event EventHandler IterateForwardRequested;
    public event EventHandler IterateBackRequested;
    public event EventHandler ResetIterationRequested;

    public void ShowResult(string resultText, ExtremumType extremumType) {
      _lblResultHeader.Text = extremumType == ExtremumType.Minimum
        ? "Найденный минимум"
        : "Найденный максимум";

      string[] lines = resultText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

      _lblResultValue.Text = lines.Length > 0 ? lines[0] : string.Empty;
    }

    public void ClearResult() {
      _lblResultValue.Text = "—";
      _lblResultHeader.Text = SelectedExtremumType == ExtremumType.Minimum
        ? "Найденный минимум"
        : "Найденный максимум";
    }

    public void BuildChart(string expression, double a, double b, double? extremum, ExtremumType extremumType) {
      _chartBuilder.Build(_chart, expression, a, b, extremum, extremumType);
    }

    public void BuildChartWithIterations(string expression, double a, double b,
                                         List<NewtonIteration> iterations, int currentStep) {
      _chartBuilder.Build(_chart, expression, a, b, null, SelectedExtremumType);
      _chartBuilder.DrawIterations(_chart, expression, a, b, iterations, currentStep);
    }

    public void ShowIterationControls(bool visible) {
      _iterationPanel.Visible = visible;
    }

    public void ShowCurrentIteration(int step, int totalSteps, double x, double fx) {
      _lblStepInfo.Text = $"Шаг {step} из {totalSteps}:  x = {x:F6},  f(x) = {fx:F6}";
      _btnPrevStep.Enabled = step > 0;
      _btnNextStep.Enabled = step < totalSteps;
    }

    public void ShowError(string message) {
      MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowInfo(string message) {
      MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuildUi() {
      this.Text = "Метод Ньютона";
      this.ClientSize = new Size(760, 760);
      this.MinimumSize = new Size(760, 760);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.BackColor = AppTheme.Background;
      this.Font = AppTheme.FontRegular;

      _workspace = new Panel {
        Dock = DockStyle.Fill,
        Padding = new Padding(0),
        BackColor = AppTheme.Background,
        AutoScroll = true
      };
      this.Controls.Add(_workspace);

      _sidebar = new Panel {
        Dock = DockStyle.Left,
        Width = 360,
        BackColor = AppTheme.Sidebar,
        Padding = new Padding(0)
      };
      this.Controls.Add(_sidebar);

      BuildSidebar();
      BuildWorkspace();
    }

    private void BuildSidebar() {
      int top = 10;

      var lblTitle = new Label {
        Text = "Метод Ньютона",
        Font = AppTheme.FontTitle,
        ForeColor = Color.White,
        Location = new Point(103, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(lblTitle);

      top += 30;

      var lblSub = new Label {
        Text = "Поиск экстремума f(x) на [a, b]",
        Font = AppTheme.FontRegular,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(89, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(lblSub);

      top += 48;

      _txtFunction = CreateSidebarField("Функция f(x)", ref top);
      _txtFunction.Text = "(x - 2)^2";

      _txtA = CreateSidebarField("Левая граница a", ref top);
      _txtA.Text = "0";

      _txtB = CreateSidebarField("Правая граница b", ref top);
      _txtB.Text = "5";

      _txtE = CreateSidebarField("Точность e", ref top);
      _txtE.Text = "0,0001";

      top += 4;

      var lblExtremum = new Label {
        Text = "Искать",
        Font = AppTheme.FontBold,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(22, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(lblExtremum);

      top += 24;

      _rbMinimum = new RadioButton {
        Text = "Минимум",
        Font = AppTheme.FontRegular,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(80, top),
        AutoSize = true,
        Checked = true
      };
      _sidebar.Controls.Add(_rbMinimum);

      _rbMaximum = new RadioButton {
        Text = "Максимум",
        Font = AppTheme.FontRegular,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(196, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(_rbMaximum);

      top += 34;

      _btnCalculate = CreateSidebarButton("Рассчитать", AppTheme.Accent, ref top);
      _btnCalculate.Click += (s, e) => CalculateRequested?.Invoke(this, EventArgs.Empty);

      _btnBuild = CreateSidebarButton("Построить график", AppTheme.AccentHover, ref top);
      _btnBuild.Click += (s, e) => BuildChartRequested?.Invoke(this, EventArgs.Empty);

      _btnClear = CreateSidebarButton("Очистить", Color.FromArgb(58, 68, 90), ref top);
      _btnClear.Click += (s, e) => {
        ClearRequested?.Invoke(this, EventArgs.Empty);
        _txtFunction.Clear();
        _txtA.Clear();
        _txtB.Clear();
        _txtE.Clear();
      };
    }

    private TextBox CreateSidebarField(string caption, ref int top) {
      var label = new Label {
        Text = caption,
        Font = AppTheme.FontBold,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(22, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(label);

      top += 24;

      var textBox = new TextBox {
        Location = new Point(24, top),
        Width = _sidebar.Width - 48,
        Font = AppTheme.FontRegular,
        BorderStyle = BorderStyle.FixedSingle
      };
      _sidebar.Controls.Add(textBox);

      top += 42;

      return textBox;
    }

    private Button CreateSidebarButton(string text, Color backColor, ref int top) {
      var button = new Button {
        Text = text,
        Location = new Point(24, top),
        Width = _sidebar.Width - 48,
        Height = 40,
        FlatStyle = FlatStyle.Flat,
        BackColor = backColor,
        ForeColor = Color.White,
        Font = AppTheme.FontBold,
        Cursor = Cursors.Hand
      };
      button.FlatAppearance.BorderSize = 0;
      _sidebar.Controls.Add(button);

      top += 50;

      return button;
    }

    private void BuildWorkspace() {
      _chartCard = CreateCard();
      _chartCard.Dock = DockStyle.Left;
      _chartCard.Width = 400;
      _chartCard.Padding = new Padding(16, 12, 16, 16);

      var lblChartHeader = new Label {
        Text = "График функции",
        Font = AppTheme.FontHeader,
        ForeColor = AppTheme.TextPrimary,
        Dock = DockStyle.Top,
        Height = 28,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _chart = new Chart {
        Dock = DockStyle.Fill,
        BackColor = AppTheme.Card
      };

      var area = new ChartArea("MainArea");
      area.BackColor = AppTheme.Card;
      _chart.ChartAreas.Add(area);

      var legend = new Legend("MainLegend") {
        Docking = Docking.Bottom,
        Alignment = StringAlignment.Center,
        BackColor = Color.Transparent,
        ForeColor = AppTheme.TextSecondary,
        Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
        Position = new ElementPosition(2, 84, 96, 8),
        LegendStyle = LegendStyle.Row,
        IsTextAutoFit = false
      };
      legend.Position.Auto = false;
      _chart.Legends.Add(legend);

      _chartCard.Controls.Add(_chart);
      _chartCard.Controls.Add(lblChartHeader);

      _resultCard = CreateCard();
      _resultCard.Dock = DockStyle.Bottom;
      _resultCard.Height = 90;
      _resultCard.Padding = new Padding(16, 10, 16, 12);

      _lblResultHeader = new Label {
        Text = "Найденный минимум",
        Font = AppTheme.FontBold,
        ForeColor = AppTheme.TextSecondary,
        Dock = DockStyle.Top,
        Height = 22,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _lblResultValue = new Label {
        Text = "—",
        Font = new GdiFont("Consolas", 14f, FontStyle.Bold),
        ForeColor = AppTheme.Success,
        Dock = DockStyle.Top,
        Height = 34,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _resultCard.Controls.Add(_lblResultValue);
      _resultCard.Controls.Add(_lblResultHeader);

      _iterationPanel = new Panel {
        Dock = DockStyle.Bottom,
        Height = 44,
        BackColor = AppTheme.Card,
        Padding = new Padding(16, 4, 16, 4),
        Visible = false
      };

      _btnPrevStep = new Button {
        Text = "◀",
        Width = 40,
        Dock = DockStyle.Left,
        FlatStyle = FlatStyle.Flat,
        BackColor = AppTheme.Card,
        ForeColor = AppTheme.TextPrimary,
        Font = AppTheme.FontBold,
        Cursor = Cursors.Hand
      };
      _btnPrevStep.FlatAppearance.BorderColor = AppTheme.Border;
      _btnPrevStep.Click += (s, e) => IterateBackRequested?.Invoke(this, EventArgs.Empty);

      _btnNextStep = new Button {
        Text = "▶",
        Width = 40,
        Dock = DockStyle.Right,
        FlatStyle = FlatStyle.Flat,
        BackColor = AppTheme.Card,
        ForeColor = AppTheme.TextPrimary,
        Font = AppTheme.FontBold,
        Cursor = Cursors.Hand
      };
      _btnNextStep.FlatAppearance.BorderColor = AppTheme.Border;
      _btnNextStep.Click += (s, e) => IterateForwardRequested?.Invoke(this, EventArgs.Empty);

      _btnResetStep = new Button {
        Text = "Сброс",
        Width = 70,
        Dock = DockStyle.Right,
        FlatStyle = FlatStyle.Flat,
        BackColor = AppTheme.Card,
        ForeColor = AppTheme.TextPrimary,
        Font = AppTheme.FontRegular,
        Cursor = Cursors.Hand
      };
      _btnResetStep.FlatAppearance.BorderColor = AppTheme.Border;
      _btnResetStep.Click += (s, e) => ResetIterationRequested?.Invoke(this, EventArgs.Empty);

      _lblStepInfo = new Label {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = AppTheme.FontRegular,
        ForeColor = AppTheme.TextSecondary
      };

      _iterationPanel.Controls.Add(_lblStepInfo);
      _iterationPanel.Controls.Add(_btnResetStep);
      _iterationPanel.Controls.Add(_btnNextStep);
      _iterationPanel.Controls.Add(_btnPrevStep);

      _workspace.Controls.Add(_chartCard);
      _workspace.Controls.Add(_resultCard);
      _workspace.Controls.Add(_iterationPanel);
    }

    private Panel CreateCard() {
      var panel = new Panel {
        BackColor = AppTheme.Card,
        Padding = new Padding(16)
      };

      panel.Paint += (s, e) => {
        var p = (Panel)s;
        using (var pen = new Pen(AppTheme.Border, 1)) {
          e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }
      };

      return panel;
    }
  }
}