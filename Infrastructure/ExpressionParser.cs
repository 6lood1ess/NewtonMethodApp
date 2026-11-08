using System.Globalization;

namespace NewtonMethodApp.Infrastructure {
  public class ExpressionParser {
    private readonly string _input;
    private int _pos;
    private double _x;

    public ExpressionParser(string input) {
      _input = (input ?? string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
    }

    public double Evaluate(double x) {
      _x = x;
      _pos = 0;

      double value = ParseExpression();

      if (_pos < _input.Length) {
        throw new FormatException($"Некорректное выражение в позиции {_pos}");
      }

      return value;
    }

    private double ParseExpression() {
      double value = ParseTerm();

      while (_pos < _input.Length) {
        char c = _input[_pos];

        if (c == '+') {
          _pos++;
          value += ParseTerm();

        } else if (c == '-') {
          _pos++;
          value -= ParseTerm();

        } else {
          break;
        }
      }

      return value;
    }

    private double ParseTerm() {
      double value = ParsePower();

      while (_pos < _input.Length) {
        char c = _input[_pos];

        if (c == '*') {
          _pos++;
          value *= ParsePower();

        } else if (c == '/') {
          _pos++;
          double divisor = ParsePower();

          if (Math.Abs(divisor) < double.Epsilon) {
            throw new DivideByZeroException("Деление на ноль");
          }

          value /= divisor;

        } else {
          break;
        }
      }

      return value;
    }

    private double ParsePower() {
      double value = ParseUnary();

      if (_pos < _input.Length && _input[_pos] == '^') {
        _pos++;
        double exponent = ParsePower();
        value = Math.Pow(value, exponent);
      }

      return value;
    }

    private double ParseUnary() {
      if (_pos < _input.Length && _input[_pos] == '-') {
        _pos++;
        return -ParseUnary();
      }

      if (_pos < _input.Length && _input[_pos] == '+') {
        _pos++;
        return ParseUnary();
      }

      return ParseAtom();
    }

    private double ParseAtom() {
      if (_pos >= _input.Length) {
        throw new FormatException("Неожиданный конец выражения");
      }

      char c = _input[_pos];

      if (c == '(') {
        _pos++;
        double value = ParseExpression();

        if (_pos >= _input.Length || _input[_pos] != ')') {
          throw new FormatException("Ожидалась закрывающая скобка");
        }

        _pos++;
        return value;
      }

      if (char.IsDigit(c) || c == '.') {
        return ParseNumber();
      }

      if (char.IsLetter(c)) {
        return ParseIdentifier();
      }

      throw new FormatException($"Неожиданный символ '{c}' в позиции {_pos}");
    }

    private double ParseNumber() {
      int start = _pos;

      while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '.')) {
        _pos++;
      }

      string token = _input.Substring(start, _pos - start);

      if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)) {
        throw new FormatException($"Некорректное число '{token}'");
      }

      return value;
    }

    private double ParseIdentifier() {
      int start = _pos;

      while (_pos < _input.Length && char.IsLetter(_input[_pos])) {
        _pos++;
      }

      string name = _input.Substring(start, _pos - start);

      if (name == "x") return _x;
      if (name == "pi") return Math.PI;
      if (name == "e") return Math.E;

      if (_pos < _input.Length && _input[_pos] == '(') {
        _pos++;
        double arg = ParseExpression();

        if (_pos >= _input.Length || _input[_pos] != ')') {
          throw new FormatException($"Ожидалась закрывающая скобка после '{name}'");
        }

        _pos++;
        return ApplyFunction(name, arg);
      }

      throw new FormatException($"Неизвестный идентификатор '{name}'");
    }

    private double ApplyFunction(string name, double arg) {
      switch (name) {
        case "sin": return Math.Sin(arg);
        case "cos": return Math.Cos(arg);
        case "tg": return Math.Tan(arg);
        case "tan": return Math.Tan(arg);
        case "ctg": return 1.0 / Math.Tan(arg);
        case "cot": return 1.0 / Math.Tan(arg);
        case "ln": return Math.Log(arg);
        case "log": return Math.Log10(arg);
        case "exp": return Math.Exp(arg);
        case "sqrt": return Math.Sqrt(arg);
        case "abs": return Math.Abs(arg);
        default: throw new FormatException($"Неизвестная функция '{name}'");
      }
    }
  }
}