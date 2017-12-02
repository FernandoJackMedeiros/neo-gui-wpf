namespace Neo.Gui.Base.Managers
{
    /// <summary>
    /// Interface of the DialogManager that abstracts the usage of Dialog windows in the application.
    /// </summary>
    public interface IDialogManager
    {
        T ShowDialog<T>(params string[] parameters);
    }
}