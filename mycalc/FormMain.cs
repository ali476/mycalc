using mycalc.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mycalc
{
    /// <summary>
    /// Application main form
    /// </summary>
    public partial class FormMain : Form
    {
        private Calculator m_calculator = new Calculator();

        /// <summary>
        /// Constructor for the application's main form
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            m_calculator.OnCalculatorAction += new Calculator.CalculatorAction(CalculatorActionHandler);
        }

        private void CalculatorActionHandler(object sender, CalculatorEventArgs e)
        {
            textOut.Text = e.DisplayValue;
            labelOut.Text = e.History;
            labelMem.Text = String.IsNullOrEmpty(e.Memory) ? "" : "Mem: " + e.Memory;
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            m_calculator.DataEntry(e.KeyChar.ToString());
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            m_calculator.DataEntry(((Button)sender).Text);
            btnEqual.Focus();
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
                Clipboard.SetText(textOut.Text);
            else if (e.Control && e.KeyCode == Keys.V)
            {
                m_calculator.Value = Clipboard.GetText();
            }
        }
    }
}
