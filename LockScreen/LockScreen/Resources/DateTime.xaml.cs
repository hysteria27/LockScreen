using System.Windows.Controls;

namespace LockScreen
{
    public partial class DateTime : UserControl
    {
        private System.Windows.Threading.DispatcherTimer Timer;

        public DateTime()
        {
            InitializeComponent();

            Timer = new System.Windows.Threading.DispatcherTimer() { Interval = System.TimeSpan.FromSeconds(1) };
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            Time.Text = System.DateTime.Now.ToShortTimeString();
            Date.Text = System.DateTime.Now.DayOfWeek + ", " + System.DateTime.Now.ToString("dd MMMM");
        }
    }
}
