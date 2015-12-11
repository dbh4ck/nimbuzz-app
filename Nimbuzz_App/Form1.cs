using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using agsXMPP;
using System.Diagnostics;
using agsXMPP.Exceptions;
using agsXMPP.Collections;
using agsXMPP.Util;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Threading;
using MehdiComponent;
using MehdiComponent.MehdiXmpp.Create;
using MehdiComponent.Xml;

namespace DBuzz
{
    
    public partial class Form1 : Form
     {
        
        XmppClientConnection bcon = new XmppClientConnection();
        public Form1()
        {
            InitializeComponent();
            
            bcon.OnLogin += new ObjectHandler(bcon_onlogin);
            bcon.OnAuthError += new XmppElementHandler(bcon_onerror);
            bcon.OnPresence += new agsXMPP.protocol.client.PresenceHandler(connection_onpresence);
            bcon.OnMessage += new agsXMPP.protocol.client.MessageHandler(connection_onmsg);
            bcon.OnReadXml += new XmlHandler(XmppCon_OnReadXml);
         // bcon.OnWriteXml += new XmlHandler(XmppCon_OnWriteXml);
            bcon.OnIq += new IqHandler(con_oniq);


            {
                idMaker.Created += idMaker_Created;
                idMaker.ErrorCreate += idMaker_ErrorCreate;
                idMaker.Available += idMaker_Available;
                idMaker.WrongCaptcha += idMaker_WrongCaptcha;
                idMaker.InvalidPassword += idMaker_InvalidPassword;
             // idMaker.LoadChaptcha(pictureBox3);
            }
        }

        
        private IdMaker idMaker = new IdMaker(); 
        
        
        private void XmppCon_OnReadXml(object sender, string xml)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new agsXMPP.XmlHandler(XmppCon_OnReadXml), new object[] { sender, xml });
                return;
            }
       
        }


        void bcon_onerror(object sender, agsXMPP.Xml.Dom.Element e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new XmppElementHandler(bcon_onerror), new object[] { sender, e });
                return;
            }

            MessageBox.Show("Yu have entered a wrong username or password", "Attention!");

            listBox1.Items.Add("Wrong username or password");
            textBox1.BackColor = Color.Red;
            textBox2.BackColor = Color.Red;

        }

        void bcon_onlogin(object sender)
        {
            
            if (InvokeRequired)
            {
                BeginInvoke(new ObjectHandler(bcon_onlogin), new object[] { sender });
                return;
            }
            listBox1.Items.Remove("Please wait...");
            listBox1.Items.Add("Yu are logged in successfully");
            textBox1.BackColor = Color.Green;
            textBox2.BackColor = Color.Green;
        }

        private string Username;
        private void button1_Click(object sender, EventArgs e)
        
                {
                    listBox1.Items.Add("Please wait...");
                    bcon.Server = "nimbuzz.com";
                    bcon.ConnectServer = "o.nimbuzz.com";
                    bcon.Open(textBox1.Text, textBox2.Text, textBox3.Text, 50);
                    bcon.Username = textBox1.Text;
                    bcon.Password = textBox2.Text;
                    bcon.Resource = textBox3.Text;
                    bcon.Priority = 10;
                    bcon.Status = "Online via DBuzz :)";
                    bcon.Port = 5222;
                    bcon.AutoAgents = false;
                    bcon.AutoResolveConnectServer = true;
                    bcon.UseCompression = false;
                    Username = textBox1.Text;

                }        

        
        private void button2_Click(object sender, EventArgs e)
        {
            bcon.SocketDisconnect();
            textBox1.BackColor = Color.Yellow;
            textBox2.BackColor = Color.Yellow;
            richTextBox1.Clear();
            listBox1.Items.Clear();
            listBox1.Items.Add("Yu are logged out");

        }

        private Jid Roomjid;
        agsXMPP.protocol.x.muc.MucManager manager;
        private void button3_Click(object sender, EventArgs e)
        {
            manager = new agsXMPP.protocol.x.muc.MucManager(bcon);
            Jid room = new Jid(textBox4.Text + "@conference.nimbuzz.com");
            //manager.JoinRoom(room, Username);
            //manager.AcceptDefaultConfiguration(room);
            Roomjid = room;

            //you can also use xml to join room
            try
           {
               bcon.Send("<presence to='" + textBox4.Text + "@conference.nimbuzz.com/" + textBox1.Text + "' xml:lang='en'><x xmlns='http://jabber.org/protocol/muc' /></presence>");
           }

          catch { }

           
        }

   
        private void button4_Click(object sender, EventArgs e)
        {
           manager.LeaveRoom(Roomjid, Username);
        }

        
        
        void connection_onmsg(object sender, agsXMPP.protocol.client.Message msg)
        {
            if (msg.Type == MessageType.error || msg.Body == null)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(new agsXMPP.protocol.client.MessageHandler(connection_onmsg), new object[] { sender, msg });
                return;

            }

            

            //if message is a group chat message
            if (msg.From.User != string.Empty)
            {
                if (msg.From.User + "@conference.nimbuzz.com" == Roomjid.ToString())
                {
                    richTextBox1.SelectionColor = Color.Green;
                    richTextBox1.AppendText(msg.From.Resource + ": ");
                    richTextBox1.AppendText(msg.Body);
                    richTextBox1.AppendText("\r\n");

                }
    
            }


            if (msg.Type == MessageType.groupchat)
            {
                if (msg.From.Resource == "admin")
                {
                    pictureBox2.Load(msg.Body.Replace("Enter the right answer to start chatting.", ""));
                   // textBox5.Text = msg.Body.Replace("Enter the right answer to start chatting.", "");
                }
            }

        }


        void connection_onpresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new agsXMPP.protocol.client.PresenceHandler(connection_onpresence), new object[] { sender, pres });
                return;
            }



            if (pres.Type == PresenceType.available)
            {
                listBox1.Items.Add(pres.From.Resource + " is online");
            }
            if (pres.Type == PresenceType.unavailable)
            {
                listBox1.Items.Add(pres.From.Resource + " went offline");
            }


        }

        //   private void XmppCon_OnWriteXml(object sender, string xml)
        //    {
        //          if (InvokeRequired)
        //        {

        //             BeginInvoke(new XmlHandler(XmppCon_OnWriteXml), new object[] { sender, xml });
        //             return;
        //        }
        //   }


        private void button7_Click(object sender, EventArgs e)
        {
            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
            msg.Type = MessageType.groupchat;
            msg.To = Roomjid;
            msg.Body = textBox5.Text;
            bcon.Send(msg);
            textBox5.Clear();
        }


        private void con_oniq(object sender, agsXMPP.protocol.client.IQ iq)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new IqHandler(con_oniq), new object[] { sender, iq });
                return;
            }
        }


        private void richTextBox2_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                button5.PerformClick();
                this.richTextBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(CheckKeys);
            }
        }


        private void CheckKeys(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                // Then Enter key was pressed
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {

            if (richTextBox2.Text != string.Empty)
            {
                agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
                msg.Type = MessageType.groupchat;
                msg.To = Roomjid;
                msg.Body = richTextBox2.Text;
                bcon.Send(msg);
                richTextBox2.Clear();
            }
           // bcon.Send("db");
        }


        private void label5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("IEXplore", "http://spario-wiki.blogspot.in");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }


        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox7.Text))
            {
                lblsts.TextAlign = ContentAlignment.MiddleCenter;
                lblsts.BackColor = Color.Yellow;
                lblsts.Text = "Please Enter Your Account Name";
                return;
            }
            if (string.IsNullOrEmpty(textBox6.Text))
            {
                lblsts.BackColor = Color.Yellow;
                lblsts.Text = "Please Enter Your Password";
                return;
            }
            if (string.IsNullOrEmpty(textBox8.Text))
            {
                lblsts.BackColor = Color.Yellow;
                lblsts.Text = "Please Enter Captcha";
                return;
            }
           
            
            idMaker.Create(textBox7.Text, textBox6.Text, textBox8.Text);
        }


        private void idMaker_InvalidPassword(IdMaker idMaker, string msg)
        {
            MessageBox.Show("Invalid Password", "Error");
            return;
        }

        private void idMaker_WrongCaptcha(IdMaker idMaker, string msg)
        {
            MessageBox.Show("Wrong Captcha", "Error");
            return;
        }

        private void idMaker_Available(IdMaker idMaker, bool available)
        {
            Invoke(new Action(delegate
            {
                if (available == false)
                {
                    var msgBox = new MehdiMessageBox("\nThis Id is Already Created", "DBuzz",
                    MehdiMessageBox.Type.Error);
                    msgBox.MessageColor = Color.Red;
                    msgBox.TitleColor = Color.Red;
                    msgBox.ShowDialog();
                }
            }));
        }

        private void idMaker_ErrorCreate(IdMaker idMaker)
        {
            lblsts.Text = "Error Create";
            MessageBox.Show("Error in Creating Account", "DBuzz");
            return;
        }

        private void idMaker_Created(IdMaker idMaker, string msg, string data)
        {
            MessageBox.Show("Account Created Sucessfully!", "DBuzz");
            return;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            idMaker.LoadChaptcha(pictureBox3);
        }


    }
}