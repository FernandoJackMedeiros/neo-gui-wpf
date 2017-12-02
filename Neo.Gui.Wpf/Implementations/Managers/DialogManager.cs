using Autofac;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class DialogManager : IDialogManager
    {
        #region Private Fields 
        private static ILifetimeScope containerLifetimeScope;
        #endregion

        #region IDialogManager implementation 
        public T ShowDialog<T>(params string[] parameters)
        {
            var dialogResult = default(T);

            var view = containerLifetimeScope?.Resolve<IDialog<T>>();

            if (view == null) return dialogResult;

            var viewModel = view.DataContext as IDialogViewModel<T>;

            if (viewModel == null) return dialogResult;

            viewModel.SetDialogResult += (sender, e) =>
            {
                dialogResult = e;
            };

            view.ShowDialog();

            return dialogResult;
        }
        #endregion

        #region Static methods
        public static void SetLifetimeScope(ILifetimeScope lifetimeScope)
        {
            containerLifetimeScope = lifetimeScope;
        }
        #endregion
    }
}
