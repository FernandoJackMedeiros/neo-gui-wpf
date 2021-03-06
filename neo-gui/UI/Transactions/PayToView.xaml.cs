﻿using Neo.Wallets;

namespace Neo.UI.Transactions
{
    public partial class PayToView
    {
        private readonly PayToViewModel viewModel;

        internal PayToView(AssetDescriptor asset = null, UInt160 scriptHash = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as PayToViewModel;

            this.viewModel?.Load(asset, scriptHash);
        }

        internal TxOutListBoxItem GetOutput()
        {
            return this.viewModel?.GetOutput();
        }
    }
}