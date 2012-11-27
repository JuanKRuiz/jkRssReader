using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RSSJuanK4Blog.User_Controls
{
    public sealed partial class PrivacyPolicyUC : UserControl
    {
        public PrivacyPolicyUC()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handler for Clos button actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //Referenciar el Popup que es el control padre de este user control
            var pop = this.Parent as Popup;

            //Si el padre es en efecto un Popup cerrarlo
            if (pop != null)
                pop.IsOpen = false;

            //Mostrar el SettingsPane
            SettingsPane.Show();
        }
    }
}
