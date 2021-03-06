﻿using Neo.SmartContract;

namespace Neo.UI.Contracts
{
    public class DisplayContractParameter
    {
        public DisplayContractParameter(int index, ContractParameter parameter)
        {
            this.Index = index;
            this.Parameter = parameter;
        }

        public string IndexStr => "[" + this.Index + "]";

        public string Type => this.Parameter?.Type.ToString();

        public string Value => this.Parameter?.ToString();

        public ContractParameter Parameter { get; }

        public int Index { get; }
    }
}