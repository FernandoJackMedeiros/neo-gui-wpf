﻿using Neo.Implementations.Wallets.EntityFramework;
using Neo.UI;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static UserWallet CurrentWallet;

        internal App()
        {
            this.InitializeComponent();

            this.MainWindow = new MainView();

            this.MainWindow.Show();
        }
    }
}