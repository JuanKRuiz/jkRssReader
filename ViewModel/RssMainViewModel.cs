using RSSJuanK4Blog.Common;
using RSSJuanK4Blog.Model;
using RSSJuanK4Blog.User_Controls;
using RSSJuanK4Blog.Util;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace RSSJuanK4Blog.ViewModel
{
    /// <summary>Connect UI with model procuring none or low coupling</summary>
    public class RssMainViewModel : BindableBase
    {
        #region constants
        /// <summary>Minimun lenght for an image dimension</summary>
        private const int MIN_LEN_IMG_SIDE = 15;
        #endregion

        #region private fields
        /// <summary>Soft reference to View's Rectangle for rendering webview contents</summary>
        private Rectangle RgWebViewRenderingSurface;
        /// <summary>Soft reference to View's WebView</summary>
        private WebView WebViewControl;

        /// <summary>Default bitmap used when no valid images are using</summary>
        private readonly BitmapImage VoidBitmap = new BitmapImage(
            new Uri(
                     (string)App.Current.Resources["void-img-uri"]
                   )

                   );
        #endregion private fields

        #region Properties

        /// <summary>First article in arcticle collection</summary>
        private Article _FirstOrDefaultArticle;
        /// <summary>First article in arcticle collection</summary>
        public Article FirstOrDefaultArticle
        {
            get { return _FirstOrDefaultArticle; }
            set
            {
                SetProperty(ref _FirstOrDefaultArticle, value);
            }
        }

        /// <summary>Article list taken from Rss Feed</summary>
        private ArticleList _articles = new ArticleList();
        /// <summary>Article list taken from Rss Feed</summary>
        public ArticleList Articles
        {
            get { return _articles; }
            set
            {
                SetProperty(ref _articles, value);
            }
        }

        /// <summary>Url where the feed is located</summary>
        private string _feedUrlString;
        /// <summary>Url where the feed is located</summary>
        public string FeedUrlString
        {
            get { return _feedUrlString; }
            set
            {
                SetProperty(ref _feedUrlString, value);
            }
        }

        /// <summary>Allow View main list to be displayed</summary>
        private bool _showList;
        /// <summary>Allow View main list to be displayed</summary>
        public bool ShowList
        {
            get { return _showList; }
            set { SetProperty(ref _showList, value); }
        }

        /// <summary>Shows when loading articles from feed proccess still in execution</summary>
        private bool _IsLoading = true;
        /// <summary>Shows when loading articles from feed proccess still in execution</summary>
        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                SetProperty(ref _IsLoading, value);
            }
        }
        #endregion Properties

        #region Attached Properties
        /// <summary>
        /// Get HtmlString property from an object (part of defined attached propoerty mechanism)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetHtmlString(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlStringProperty);
        }

        /// <summary>
        /// Set HtmlString property for an object (part of defined attached propoerty mechanism)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetHtmlString(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlStringProperty, value);
        }

        /// <summary>Allow setup Html content to a control, example: a WebView  (part of defined attached propoerty mechanism)</summary>
        public static readonly DependencyProperty HtmlStringProperty =
            DependencyProperty.RegisterAttached("HtmlString",
                                    typeof(string), typeof(RssMainViewModel),
                                    new PropertyMetadata("", HtmlStringChanged)
                                                );
        #endregion Attached Properties

        #region Methods

        /// <summary>
        /// Initialize ViewModel and View components
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            await UIInitializer();
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
        }

        /// <summary>
        /// Initialize View Model and components
        /// </summary>
        /// <returns></returns>
        private async Task UIInitializer()
        {
            string exMessage = "";
            try
            {
                Articles = await RSSHelper.GetArticleListFromFeedAsync(this.FeedUrlString);
            }
            catch (Exception e)
            {
                exMessage = e.Message;
            }

            if (!string.IsNullOrWhiteSpace(exMessage))
            {
                ShowList = false;
                IsLoading = false;
                await MessageHelper.ShowMessageAsync(exMessage, "Houston, tenemos un problema!");
                App.Current.Exit();
            }
            else
            {
                ShowList = true;
            }

            FirstOrDefaultArticle = Articles.FirstOrDefault();
            IsLoading = false;
        }
        #endregion Methods

        #region Event Handlers
        /// <summary>
        /// Handler to execute when SettingsPane is requiring for commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();
            var jkCommand = new SettingsCommand("ppolicy", "Política de Privacidad",
                                                (handler) =>
                                                {
                                                    var settingsHelper = new SettingsWindowHelper();
                                                    
                                                    //Render WebView contents in brush
                                                    var brush = new WebViewBrush();
                                                    brush.SetSource(WebViewControl);
                                                    brush.Redraw();
                                                    //Fill rectangle with brush texture
                                                    RgWebViewRenderingSurface.Fill = brush;

                                                    WebViewControl.Visibility = Visibility.Collapsed;

                                                    //Show the settings flyout
                                                    //send Action to be executed when Settings window be closed
                                                    settingsHelper.ShowFlyout(new PrivacyPolicyUC(),
                                                            () => WebViewControl.Visibility = Visibility.Visible
                                                        );
                                                });

            args.Request.ApplicationCommands.Add(jkCommand);
        }

        /// <summary>
        /// Handler to executge when View is loaded
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public async void ViewLoadHandler(object o, RoutedEventArgs e)
        {
            var tmpView = o as FrameworkElement;

            if (tmpView != null)
            {
                tmpView.DataContext = this;
                //Look for a 'soft' reference to neccesary model objects
                RgWebViewRenderingSurface = (Rectangle)tmpView.FindName("wvWrapper");
                WebViewControl = (WebView)tmpView.FindName("wvBlogContent");
            }

            if (!string.IsNullOrWhiteSpace(FeedUrlString))
            {
                await Initialize();
            }
        }

        /// <summary>
        /// Handler to execute when HtmlString attached property is changing it's contents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        public static void HtmlStringChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            var wb = sender as WebView;
            if (wb != null)
            {
                wb.NavigateToString((string)arg.NewValue);
                wb.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handle to execute when an image is loaded from internet and then open to show it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ImageOpenedHandler(object sender, RoutedEventArgs e)
        {
            var tmp = (Image)sender;
            var bmp = (BitmapImage)tmp.Source;

            if (bmp.PixelWidth <= MIN_LEN_IMG_SIDE || bmp.PixelHeight <= MIN_LEN_IMG_SIDE)
                tmp.Source = VoidBitmap;
        }
        #endregion Event Handlers
    }
}
