using System.Windows;
using System.Windows.Controls;

namespace XOutput.UI.Component;

public class NumericTextBox : TextBox
{
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(decimal?), typeof(NumericTextBox));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(decimal?), typeof(NumericTextBox));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?),
        typeof(NumericTextBox), new PropertyMetadata(ValueChanged));

    public NumericTextBox()
    {
        MinWidth = 30;
    }

    public decimal? Minimum
    {
        get => (decimal?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal? Maximum
    {
        get => (decimal?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal? Value
    {
        get => (decimal?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    protected static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var numericTextBox = d as NumericTextBox;
        if (numericTextBox.Value.HasValue)
            numericTextBox.Text = decimal.Round(numericTextBox.Value.Value).ToString();
        else
            numericTextBox.Text = "";
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        decimal value;
        if (string.IsNullOrEmpty(Text))
        {
            Value = null;
        }
        else
        {
            if (decimal.TryParse(Text, out value))
            {
                if (Minimum.HasValue && value < Minimum)
                    Value = Minimum.Value;
                else if (Maximum.HasValue && value > Maximum)
                    Value = Maximum.Value;
                else
                    Value = value;
            }
            else
            {
                if (Minimum.HasValue)
                    Value = Minimum.Value;
                else if (Maximum.HasValue) Value = Maximum.Value;
            }

            Text = decimal.Round(Value.Value).ToString();
        }

        base.OnTextChanged(e);
    }
}