using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

/// <summary>Helper to easy implement a Settings Charm Style Window</summary>
public class SettingsWindowHelper
{
    /// <summary>Default Window Size</summary>
    public const double DEFAULT_WIDTH = 346;
    /// <summary>Allow to show any control floating over the main Window</summary>
    private Popup _popup;
    /// <summary>Delegate to execute when Popup is closed</summary>
    Action CloseAction;


    public SettingsWindowHelper()
    {
        _popup = new Popup();
        _popup.IsLightDismissEnabled = true;
        _popup.Closed += OnPopupClosed;
    }

    /// <summary>
    /// Raise when internal Popup object is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnPopupClosed(object sender, object e)
    {
        if (CloseAction != null)
            CloseAction.Invoke();
    }

    /// <summary>
    /// Shows an user control over the current Window
    /// </summary>
    /// <param name="control">User control to show as settings window</param>
    /// <param name="closeAction">Method to execute when Popup is closed</param>
    /// <param name="width">Window Width</param>
    public void ShowFlyout(UserControl control, Action closeAction = null,
                            double width = DEFAULT_WIDTH)
    {
        //Asignar acción a ejecutar al cerrar el Popup
        CloseAction = closeAction;

        //Asignar ancho y alto del Popup
        _popup.Width = width;
        _popup.Height = Window.Current.Bounds.Height;

        /* Asignar el ancho y alto del control
            * Aunque este puede ya traerlos definidos
            * en este caso es conveniente adecuarlo a 
            * la estructura que hemos planteado*/
        control.Width = width;
        control.Height = Window.Current.Bounds.Height;

        //Asignar el control del parámetro al Popup
        _popup.Child = control;

        //Establecer en que parte de la ventana se comienza a dibujar el Popup
        _popup.VerticalOffset = 0;
        _popup.HorizontalOffset = Window.Current.Bounds.Width - width;

        //Mostrar el Popup, sus contenidos
        _popup.IsOpen = true;
    }
}
