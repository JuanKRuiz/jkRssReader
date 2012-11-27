using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace RSSJuanK4Blog.Util
{
    /// <summary>Provide an easy way to Show a common MessageDialog</summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Show a MessageDialog with one button
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task ShowMessageAsync(string content, string title)
        {
            MessageDialog msg;
            if (!string.IsNullOrWhiteSpace(title))
                msg = new MessageDialog(content, title);
            else
                msg = new MessageDialog(content);

            msg.Options = MessageDialogOptions.AcceptUserInputAfterDelay;

            await msg.ShowAsync();
        }
    }
}
