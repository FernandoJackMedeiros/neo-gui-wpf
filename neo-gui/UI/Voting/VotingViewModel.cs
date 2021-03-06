﻿using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.UI.Base.Extensions;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.VM;
using Neo.Wallets;

namespace Neo.UI.Voting
{
    public class VotingViewModel : ViewModelBase
    {
        private readonly IMessagePublisher messagePublisher;

        private UInt160 scriptHash;

        private string votes;

        public VotingViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

        public string Address { get; private set; }

        public string Votes
        {
            get => this.votes;
            set
            {
                if (this.votes == value) return;

                this.votes = value;

                NotifyPropertyChanged();
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public void SetScriptHash(UInt160 hash)
        {
            this.scriptHash = hash;

            var account = Blockchain.Default.GetAccountState(hash);

            // Set address
            this.Address = Wallet.ToAddress(hash);

            // Concatenate votes into multi-line string
            var voteStrings = account.Votes.Select(p => p.ToString()).ToArray();

            // Set votes
            this.Votes = voteStrings.ToMultiLineString();

            // Update bindable properties
            NotifyPropertyChanged(nameof(this.Address));
        }

        private InvocationTransaction GenerateTransaction()
        {
            using (var builder = new ScriptBuilder())
            {
                var voteLineCount = 0;

                if (!string.IsNullOrEmpty(this.Votes))
                {
                    // Split vote lines
                    var voteLines = this.Votes.ToLines();

                    foreach (var line in voteLines.Reverse())
                    {
                        builder.EmitPush(line.HexToBytes());
                    }

                    voteLineCount = voteLines.Length;
                }

                builder.EmitPush(voteLineCount);
                builder.Emit(OpCode.PACK);
                builder.EmitPush(this.scriptHash);
                builder.EmitSysCall("Neo.Blockchain.GetAccount");
                builder.EmitSysCall("Neo.Account.SetVotes");

                return new InvocationTransaction
                {
                    Script = builder.ToArray(),
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = this.scriptHash.ToArray()
                        }
                    }
                };
            }
        }

        private void Ok()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.TryClose();
        }

        private void Cancel()
        {
            this.TryClose();
        }
    }
}