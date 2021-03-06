﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.UI.Base.Extensions;
using Neo.SmartContract;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.VM;

namespace Neo.UI.Voting
{
    public class ElectionViewModel : ViewModelBase
    {
        private readonly IMessagePublisher messagePublisher;

        private ECPoint selectedBookKeeper;

        public ElectionViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            if (ApplicationContext.Instance.CurrentWallet == null) return;

            // Load book keepers
            var bookKeepers = ApplicationContext.Instance.CurrentWallet.GetContracts().Where(p => p.IsStandard).Select(p =>
                ApplicationContext.Instance.CurrentWallet.GetKey(p.PublicKeyHash).PublicKey);

            this.BookKeepers = new ObservableCollection<ECPoint>(bookKeepers);
        }

        public ObservableCollection<ECPoint> BookKeepers { get; }

        public ECPoint SelectedBookKeeper
        {
            get => this.selectedBookKeeper;
            set
            {
                if (this.selectedBookKeeper != null && this.selectedBookKeeper.Equals(value)) return;

                this.selectedBookKeeper = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool OkEnabled => this.SelectedBookKeeper != null;
        
        public ICommand OkCommand => new RelayCommand(this.Ok);

        private void Ok()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.TryClose();
        }

        private InvocationTransaction GenerateTransaction()
        {
            if (this.SelectedBookKeeper == null) return null;

            var publicKey = this.SelectedBookKeeper;

            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Validator.Register", publicKey);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }
    }
}