﻿using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace KIAS_PROJECT_socket_api_win
{
    class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;
        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString()); // When character data is written, append it to the text box.
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
