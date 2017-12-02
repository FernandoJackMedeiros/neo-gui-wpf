using System;
using System.Reflection;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;
using Neo.Gui.Wpf.MVVM;
using Neo.Gui.Wpf.Views.Contracts;
using Neo.Gui.Wpf.Views.Development;
using Neo.Gui.Wpf.Views.Updater;
using Neo.Gui.Wpf.Views.Wallets;
using GuiSettings = Neo.Gui.Wpf.Properties.Settings;

namespace Neo.Gui.Wpf.Views.Home
{
    public class HomeViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<UpdateApplicationMessage>,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<InvokeContractMessage>,
        IMessageHandler<WalletStatusMessage>
    {
        #region Private Fields
        private const string OfficialWebsiteUrl = "https://neo.org/";

        private readonly IWalletController walletController;
        private readonly IDialogManager dialogManager;
        private readonly IProcessHelper processHelper;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IDispatchService dispatchService;

        private bool nextBlockProgressIsIndeterminate;
        private double nextBlockProgressFraction;

        private string newVersionLabel;
        private bool newVersionVisible;

        private string heightStatus;
        private uint nodeCount;
        private string blockStatus;
        #endregion

        #region Constructor
        public HomeViewModel(
            IWalletController walletController,
            IDialogManager dialogManager, 
            IProcessHelper processHelper,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber, 
            IDispatchService dispatchService)
        {
            this.walletController = walletController;
            this.dialogManager = dialogManager;
            this.processHelper = processHelper;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
            this.dispatchService = dispatchService;
        }
        #endregion

        #region Public Properties
        public bool WalletIsOpen => this.walletController.WalletIsOpen;

        public string HeightStatus
        {
            get => this.heightStatus;
            set
            {
                if (this.heightStatus == value) return;

                this.heightStatus = value;

                NotifyPropertyChanged();
            }
        }

        public uint NodeCount
        {
            get => this.nodeCount;
            set
            {
                if (this.nodeCount == value) return;

                this.nodeCount = value;

                NotifyPropertyChanged();
            }
        }

        public bool NextBlockProgressIsIndeterminate
        {
            get => this.nextBlockProgressIsIndeterminate;
            set
            {
                if (this.nextBlockProgressIsIndeterminate == value) return;

                this.nextBlockProgressIsIndeterminate = value;

                NotifyPropertyChanged();
            }
        }

        public double NextBlockProgressFraction
        {
            get => this.nextBlockProgressFraction;
            set
            {
                if (this.nextBlockProgressFraction.Equals(value)) return;

                this.nextBlockProgressFraction = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.NextBlockProgressPercentage));
            }
        }

        public int NextBlockProgressPercentage => (int) Math.Round(this.NextBlockProgressFraction * 100.0);

        #endregion Public Properies

        #region Commands

        public ICommand CreateWalletCommand => new RelayCommand(this.CreateWallet);

        public ICommand OpenWalletCommand => new RelayCommand(this.OpenWallet);

        public ICommand CloseWalletCommand => new RelayCommand(this.CloseWallet);

        public ICommand ChangePasswordCommand => new RelayCommand(ChangePassword);

        public ICommand RebuildIndexCommand => new RelayCommand(this.RebuildIndex);

        public ICommand RestoreAccountsCommand => new RelayCommand(RestoreAccounts);

        public ICommand ExitCommand => new RelayCommand(this.Exit);

        public ICommand TransferCommand => new RelayCommand(Transfer);

        public ICommand ShowTransactionDialogCommand => new RelayCommand(ShowTransactionDialog);

        public ICommand ShowSigningDialogCommand => new RelayCommand(ShowSigningDialog);

        public ICommand ClaimCommand => new RelayCommand(Claim);

        public ICommand RequestCertificateCommand => new RelayCommand(RequestCertificate);

        public ICommand AssetRegistrationCommand => new RelayCommand(AssetRegistration);

        public ICommand DistributeAssetCommand => new RelayCommand(DistributeAsset);

        public ICommand DeployContractCommand => new RelayCommand(DeployContract);

        public ICommand InvokeContractCommand => new RelayCommand(InvokeContract);

        public ICommand ShowElectionDialogCommand => new RelayCommand(ShowElectionDialog);

        public ICommand ShowSettingsCommand => new RelayCommand(ShowSettings);

        public ICommand CheckForHelpCommand => new RelayCommand(CheckForHelp);

        public ICommand ShowOfficialWebsiteCommand => new RelayCommand(ShowOfficialWebsite);

        public ICommand ShowDeveloperToolsCommand => new RelayCommand(ShowDeveloperTools);

        public ICommand AboutNeoCommand => new RelayCommand(this.ShowAboutNeoDialog);

        public ICommand ShowUpdateDialogCommand => new RelayCommand(ShowUpdateDialog);

        #endregion Commands

        #region New Version Properties

        public string NewVersionLabel
        {
            get => this.newVersionLabel;
            set
            {
                if (this.newVersionLabel == value) return;

                this.newVersionLabel = value;

                NotifyPropertyChanged();
            }
        }

        public bool NewVersionVisible
        {
            get => this.newVersionVisible;
            set
            {
                if (this.newVersionVisible == value) return;

                this.newVersionVisible = value;

                NotifyPropertyChanged();
            }
        }

        public string BlockStatus
        {
            get => this.blockStatus;
            set
            {
                this.blockStatus = value;
                this.NotifyPropertyChanged(nameof(this.BlockStatus));
            }
        }

        #endregion New Version Properties

        #region Main Menu Command Methods

        private void CreateWallet()
        {
            var result = this.dialogManager.ShowDialog<CreateWalletDialogResult>();

            if (result == null) return;

            if (string.IsNullOrEmpty(result.WalletPath) || string.IsNullOrEmpty(result.Password)) return;

            this.walletController.CreateWallet(result.WalletPath, result.Password);

            GuiSettings.Default.LastWalletPath = result.WalletPath;
            GuiSettings.Default.Save();
        }

        private void OpenWallet()
        {
            var result = this.dialogManager.ShowDialog<OpenWalletDialogResult>();

            if (result == null) return;

            if (string.IsNullOrEmpty(result.WalletPath) || string.IsNullOrEmpty(result.Password)) return;

            if (this.walletController.WalletNeedUpgrade(result.WalletPath))
            {
                var migrationApproved = this.dialogManager.ShowDialog<YesOrNoDialogResult>("ApproveWalletMigrationDialog");

                if (!migrationApproved.Yes) return;

                this.walletController.UpgradeWallet(result.WalletPath);
            }
            
            this.walletController.OpenWallet(result.WalletPath, result.Password, result.OpenInRepairMode);
            
            GuiSettings.Default.LastWalletPath = result.WalletPath;
            GuiSettings.Default.Save();
        }

        public void CloseWallet()
        {
            this.walletController.CloseWallet();
        }

        private void ChangePassword()
        {
            this.dialogManager.ShowDialog<ChangePasswordDialogResult>();
        }

        private async void RebuildIndex()
        {
            await this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.messagePublisher.Publish(new ClearAssetsMessage());
                this.messagePublisher.Publish(new ClearTransactionsMessage());
            });

            this.walletController.RebuildCurrentWallet();
        }

        private void RestoreAccounts()
        {
            this.dialogManager.ShowDialog<RestoreAccountsDialogResult>();
        }

        private void Exit()
        {
            this.TryClose();
        }

        private void Transfer()
        {
            this.dialogManager.ShowDialog<TransferDialogResult>();
        }

        private void ShowTransactionDialog()
        {
            this.dialogManager.ShowDialog<TradeDialogResult>();
        }

        private void ShowSigningDialog()
        {
            this.dialogManager.ShowDialog<SigningDialogResult>();
        }

        private static void Claim()
        {
            var view = new ClaimView();
            view.Show();
        }

        private void RequestCertificate()
        {
            this.dialogManager.ShowDialog<CertificateApplicationDialogResult>();
        }

        private void AssetRegistration()
        {
            this.dialogManager.ShowDialog<AssetRegistrationDialogResult>();
        }

        private void DistributeAsset()
        {
            this.dialogManager.ShowDialog<AssetDistributionDialogResult>();
        }

        private void DeployContract()
        {
            this.dialogManager.ShowDialog<DeployContractDialogResult>();
        }

        private void ShowElectionDialog()
        {
            this.dialogManager.ShowDialog<ElectionDialogResult>();
        }

        private void ShowSettings()
        {
            this.dialogManager.ShowDialog<SettingsDialogResult>();
        }

        private static void CheckForHelp()
        {
            // TODO Implement
        }

        private void ShowOfficialWebsite()
        {
            this.processHelper.OpenInExternalBrowser(OfficialWebsiteUrl);
        }

        private static void ShowDeveloperTools()
        {
            var view = new DeveloperToolsView();
            view.Show();
        }

        private void ShowAboutNeoDialog()
        {
            DialogCoordinator.Instance.ShowMessageAsync(this, Strings.About, $"{Strings.AboutMessage} {Strings.AboutVersion} {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        #endregion Main Menu Command Methods
        
        private static void ShowUpdateDialog()
        {
            var dialog = new UpdateView();
            dialog.ShowDialog();
        }
        
        #region ILoadable Implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region IUnloadable Implementation 
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        private void InvokeContract()
        {
            this.messagePublisher.Publish(new InvokeContractMessage(null));
        }

        #region IMessageHandler implementation 
        public void HandleMessage(UpdateApplicationMessage message)
        {
            // Close window
            this.TryClose();

            // Start update
            this.processHelper.Run(message.UpdateScriptPath);
        }

        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.NotifyPropertyChanged(nameof(this.WalletIsOpen));
        }

        public void HandleMessage(InvokeContractMessage message)
        {
            var invokeContractView = new InvokeContractView(message.Transaction);
            invokeContractView.ShowDialog();
        }

        public void HandleMessage(WalletStatusMessage message)
        {
            var status = message.Status;

            // TODO
            this.HeightStatus = status.WalletHeight + "/" + status.BlockChainHeight + "/" + status.BlockChainHeaderHeight;
            this.NextBlockProgressIsIndeterminate = status.NextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = status.NextBlockProgressFraction;

            this.NodeCount = status.NodeCount;
            this.BlockStatus = $"{Strings.WaitingForNextBlock}:"; // TODO Update property to return actual status
        }
        #endregion
    }
}