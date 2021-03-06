﻿using System.Windows;
using System.Windows.Controls;

namespace Neo.UI.Wallets
{
    public partial class CreateWalletView
    {
        private readonly CreateWalletViewModel viewModel;

        public CreateWalletView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as CreateWalletViewModel;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null) return;

            this.viewModel?.UpdatePassword(passwordBox.Password);
        }

        private void ReEnteredPasswordChanged(object sender, RoutedEventArgs e)
        {
            var reEnteredPasswordBox = sender as PasswordBox;

            if (reEnteredPasswordBox == null) return;

            this.viewModel?.UpdateReEnteredPassword(reEnteredPasswordBox.Password);
        }

        public bool GetWalletOpenInfo(out string walletPath, out string password)
        {
            walletPath = null;
            password = null;

            if (this.viewModel == null) return false;

            return this.viewModel.GetWalletOpenInfo(out walletPath, out password);
        }
    }
}