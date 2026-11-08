using System.Globalization;

namespace NewtonMethodApp.Infrastructure {
  public static class NumberParser {
    public static bool TryParse(string input, out double value) {
      value = 0;

      if (string.IsNullOrWhiteSpace(input)) return false;

      string normalized = input.Trim().Replace(',', '.');

      return double.TryParse(
        normalized,
        NumberStyles.Float | NumberStyles.AllowThousands,
        CultureInfo.InvariantCulture,
        out value
      );
    }
  }
}