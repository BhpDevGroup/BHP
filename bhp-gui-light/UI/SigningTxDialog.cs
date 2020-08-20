﻿using Akka.Actor;
using Bhp.Network.P2P;
using Bhp.Network.P2P.Payloads;
using Bhp.Properties;
using Bhp.SmartContract;
using System;
using System.Windows.Forms;

namespace Bhp.UI
{
    internal partial class SigningTxDialog : Form
    {
        public SigningTxDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show(Strings.SigningFailedNoDataMessage);
                return;
            }
            try
            {
                ContractParametersContext context = ContractParametersContext.Parse(textBox1.Text);
                if (!Program.CurrentWallet.Sign(context))
                {
                    MessageBox.Show(Strings.SigningFailedKeyNotFoundMessage);
                    return;
                }

                textBox2.Text = context.ToString();
                if (context.Completed) button4.Visible = true;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("交易格式不正确！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.SelectAll();
            textBox2.Copy();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ContractParametersContext context = ContractParametersContext.Parse(textBox2.Text);
            if (!(context.Verifiable is Transaction tx))
            {
                MessageBox.Show("Only support to broadcast transaction.");
                return;
            }
            tx.Witnesses = context.GetWitnesses();
            //Program.System.LocalNode.Tell(new LocalNode.Relay { Inventory = tx });
            InformationBox.Show(tx.Hash.ToString(), Strings.RelaySuccessText, Strings.RelaySuccessTitle);
            button4.Visible = false;
        }
    }
}
