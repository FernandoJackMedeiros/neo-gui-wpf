﻿using System;
using System.Windows;
using System.Windows.Input;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Wpf.MVVM;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Base.Dialogs;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public class SigningViewModel : ViewModelBase, IDialogViewModel<SigningDialogResult>
    {
        private readonly IWalletController walletController;
        private readonly IClipboardManager clipboardManager;

        private string input;
        private ContractParametersContext output;
        private bool broadcastVisible;

        public SigningViewModel(
            IWalletController walletController,
            IClipboardManager clipboardManager)
        {
            this.walletController = walletController;
            this.clipboardManager = clipboardManager;
        }

        public string Input
        {
            get => this.input;
            set
            {
                if (this.input == value) return;

                this.input = value;

                NotifyPropertyChanged();
            }
        }

        public string Output => this.output?.ToString();

        public bool BroadcastVisible
        {
            get => this.broadcastVisible;
            set
            {
                if (this.broadcastVisible == value) return;

                this.broadcastVisible = value;

                NotifyPropertyChanged();
            }
        }

        public ICommand SignatureCommand => new RelayCommand(this.Sign);

        public ICommand BroadcastCommand => new RelayCommand(this.Broadcast);

        public ICommand CopyCommand => new RelayCommand(this.Copy);

        public ICommand CloseCommand => new RelayCommand(this.TryClose);
        
        #region IDialogViewModel implementation 
        public event EventHandler<SigningDialogResult> SetDialogResult;

        public SigningDialogResult DialogResult { get; private set; }
        #endregion

        private void Sign()
        {
            if (string.IsNullOrEmpty(this.Input))
            {
                MessageBox.Show(Strings.SigningFailedNoDataMessage);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = ContractParametersContext.Parse(this.Input);
            }
            catch
            {
                MessageBox.Show(Strings.SigningFailedNoDataMessage);
                return;
            }

            if (!this.walletController.Sign(context))
            {
                MessageBox.Show(Strings.SigningFailedKeyNotFoundMessage);
                return;
            }

            this.output = context;
            NotifyPropertyChanged(nameof(this.Output));

            if (context.Completed) this.BroadcastVisible = true;
        }

        private void Copy()
        {
            if (this.output == null) return;

            // TODO Highlight output textbox text

            this.clipboardManager.SetText(this.output.ToString());
        }

        private void Broadcast()
        {
            if (this.output == null) return;

            this.output.Verifiable.Scripts = this.output.GetScripts();

            var inventory = (IInventory) this.output.Verifiable;

            this.walletController.Relay(inventory);

            InformationBox.Show(inventory.Hash.ToString(), Strings.RelaySuccessText, Strings.RelaySuccessTitle);

            this.BroadcastVisible = false;
        }
    }
}