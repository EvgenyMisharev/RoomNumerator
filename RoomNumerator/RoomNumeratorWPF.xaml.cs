using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RoomNumerator
{
    public partial class RoomNumeratorWPF : Window
    {
        public string NumberPrefix;
        public string StartFrom;
        public string SelectedNumberingDirection;
        public RoomNumeratorWPF()
        {
            InitializeComponent();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            NumberPrefix = textBox_NumberPrefix.Text;
            StartFrom = textBox_StartFrom.Text;
            SelectedNumberingDirection = (groupBox_NumberingDirection.Content as System.Windows.Controls.Grid)
                .Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                .Name;
            DialogResult = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void RoomNumeratorWPF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                NumberPrefix = textBox_NumberPrefix.Text;
                StartFrom = textBox_StartFrom.Text;
                SelectedNumberingDirection = (groupBox_NumberingDirection.Content as System.Windows.Controls.Grid)
                    .Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked.Value == true)
                    .Name;
                DialogResult = true;
                Close();
            }

            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
