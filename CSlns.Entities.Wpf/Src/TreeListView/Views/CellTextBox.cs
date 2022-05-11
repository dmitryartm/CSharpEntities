using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace CSlns.Entities.Wpf.TreeListView.Views {
    public class CellTextBox : TextBox {
        public CellTextBox() {
            this.BorderThickness = new Thickness(0);
            this.Background = Brushes.Transparent;
            
            
            var text = this.Text;
            
            
            this.GotKeyboardFocus += (sender, args) => {
                StartEditing();
            };
            
            
            this.KeyDown += (sender, args) => {
                switch (args.Key) {
                    case Key.Enter:
                        args.Handled = true;
                        StopEditing();
                        break;
                    case Key.Escape:
                        args.Handled = true;
                        this.Text = text;
                        StopEditing();
                        break;
                }
            };

            
            void StopEditing() {
                Keyboard.ClearFocus();
                this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
            }

            
            void StartEditing() {
                text = this.Text;
            }
        }
    }
}