using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace KIAS_PROJECT_socket_api_win
{
    public partial class Form1 : Form
    {
        private MiniServer socketServer = null;
        TextWriter _writer = null;
        public Form1()
        {
            InitializeComponent();
            // Instantiate the writer
            _writer = new TextBoxStreamWriter(this.textBox1);
            // Redirect the out Console stream
            Console.SetOut(_writer);

            socketServer = new MiniServer(this);
            socketServer.StartServer();
        }
        public int Connect()
        {
            return axKHOpenAPI1.CommConnect();
        }
        public void GetLoginState()
        {

        }
    }
}
