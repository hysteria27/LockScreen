using System.Windows.Controls;

namespace LockScreen
{
    public partial class DateTime : UserControl
    {
        public DateTime()
        {
            InitializeComponent();

            Time.Content = System.DateTime.Now.ToShortTimeString();
            Date.Content = System.DateTime.Now.DayOfWeek + ", " + System.DateTime.Now.ToString("dd MMMM");
        }
    }
}
