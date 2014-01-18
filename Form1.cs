using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MyTetris
{
    public partial class Form1 : Form
    {
        public string getPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public Form1()
        {
            InitializeComponent();
            this.SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            
            InitGame();
        }

        private void InitGame()
        {
            PlayBox.SetOutput(pictureBox1, pictureBox2);
            PlayBox.DrawBox();
            PlayBox.ScoreDlg = MyScore;
            PlayBox.GameOverDlg = MyGameOver;
            PlayBox.StartGame();
            timer1.Enabled = true;
            toolStripStatusLabel1.Text = "0";
        }

        public void MyScore(int sc)
        {
            toolStripStatusLabel1.Text = (Int32.Parse(toolStripStatusLabel1.Text) + sc).ToString();
        }

        public void MyGameOver()
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                toolStripStatusLabel1.Text = toolStripStatusLabel1.Text + " ( Game over )";
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            PlayBox.DoTick();
            PlayBox.DrawBox();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space: PlayBox.RotateShape();
                    break;
                case Keys.Right: PlayBox.MoveRight();
                    break;
                case Keys.Left: PlayBox.MoveLeft();
                    break;
                case Keys.Down: PlayBox.MoveDown();
                    break;
                case Keys.S: PlayBox.SaveGame(getPath + @"\savegame.bin");
                    break;
                case Keys.L:
                    if (System.IO.File.Exists(getPath + @"\savegame.bin")) 
                        PlayBox.LoadGame(getPath + @"\savegame.bin");
                    break;
                case Keys.P:
                    if (timer1.Enabled == true)
                        timer1.Enabled = false;
                    else
                        timer1.Enabled = true;
                    break;
                case Keys.Escape:
                    InitGame();
                    break;
            }

            PlayBox.DrawBox();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("MyTetris game by petr.rasin@gmail.com, public domain licence" + Environment.NewLine + "2014", "About game", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
