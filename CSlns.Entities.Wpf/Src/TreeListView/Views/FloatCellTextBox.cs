using System.Globalization;
using System.Windows;


namespace CSlns.Entities.Wpf.TreeListView.Views {
    public class FloatCellTextBox : CellTextBox {

        public static readonly DependencyProperty FloatValueProperty =
            DependencyProperty.Register(
                nameof(FloatValue),
                typeof(double?),
                typeof(FloatCellTextBox),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (obj, args) => {
                        var txt = (FloatCellTextBox) obj;
                        txt.FloatValue = (double?) args.NewValue;
                    }));


        public FloatCellTextBox() {
            var text = this.Text;

            this.TextChanged += (sender, args) => {
                var newText = this.Text;
                if (TryGetFloatValue(newText, out var value)) {
                    text = newText;
                }
                else {
                    args.Handled = true;
                    var caretIndex = this.CaretIndex;
                    this.Text = text;
                    this.CaretIndex = caretIndex;
                }
            };
            
            this.PreviewTextInput += (sender, args) => {
                var newString = this.Text.Insert(this.CaretIndex, args.Text);
                args.Handled = !TryGetFloatValue(newString, out _);
            };

            this.LostFocus += (sender, args) => {
                TryGetFloatValue(this.Text, out var number);
                this.FloatValue = number;
            };
        }

        
        public double? FloatValue {
            get => (double?)this.GetValue(FloatValueProperty);
            set {
                this.SetValue(FloatValueProperty, value);
                this.Text = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }
        }
            

        public static bool TryGetFloatValue(string str, out double? value) {
            value = null;
            if (string.IsNullOrEmpty(str)) {
                value = null;
                return true;
            } else if (double.TryParse(str.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) {
                value = v;
                return true;
            }
            else {
                return false;
            }
        }
        
    }
}