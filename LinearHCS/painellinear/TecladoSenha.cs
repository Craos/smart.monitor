using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace LinearHCS
{
    public partial class TecladoSenha : Form
    {

        public delegate void Evento_TecladoSenhaClose( String val );
        public event Evento_TecladoSenhaClose evento_close;

        public byte[] unidade = new byte[7];
        public byte[] senha = new byte[6];
        public byte indiceUnidade = 0;
        public Boolean tratandoSenha = false;


        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public TecladoSenha()
        {
            InitializeComponent();
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            trataDigito(1);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            trataDigito(2);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            trataDigito(3);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            trataDigito(4);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            trataDigito(5);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            trataDigito(6);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            trataDigito(7);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox8_Click(object sender, EventArgs e)
        {
            trataDigito(8);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            trataDigito(9);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBoxEsc_Click(object sender, EventArgs e)
        {
            trataDigito(TECLA_ESC);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBoxZero_Click(object sender, EventArgs e)
        {
            trataDigito(0);
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void pictureBoxEnter_Click(object sender, EventArgs e)
        {
            trataDigito(TECLA_ENTER);
        }
        public bool fechar = false;
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public const byte TECLA_ESC = 10;
        public const byte TECLA_ENTER = 11;
        public const byte TOUT_DIGITACAO = 10; //segundos
        public ulong val;
        public void trataDigito(byte digito)
        {
            int i;

	        if( toutDigitacao_1s > 0 ) toutDigitacao_1s = TOUT_DIGITACAO;
	
	        bip(); // teclado

	        switch( digito )
	        {
		        case TECLA_ENTER:
			        if(( tratandoSenha == false )&&( checkBox_setup_setupGeral_senha13.Checked ))
			        {
                        textBox_senha.Focus();
				        tratandoSenha = true;
				        bip();
			        }
			        else
			        {
				        tratandoSenha = false;
                        byte[] buf = new byte[6];

                        if (checkBox_setup_setupGeral_senha13.Checked)
                        {
                            val = 0;
                            for (i = 0; i < 7; i++)
                            {
                                val *= 10;
                                val += unidade[6 - i];
                            }

                            for (i = 0; i < 6; i++)
                            {
                                val *= 10;
                                val += senha[5 - i];
                            }

                            for (i = 0; i < 6; i++)
                            {
                                buf[5 - i] = (byte)val;
                                val >>= 8;
                            }

                            String s = BytesToHexString(buf);
                            s = s.Substring(0, s.Length - 1);
                        }
                        else
                        {
                            buf[5] = (byte)(senha[0]);
                            buf[5] += (byte)(senha[1]<<4);
                            buf[4] = (byte)(senha[2]);
                            buf[4] += (byte)(senha[3] << 4);
                            buf[3] = (byte)(senha[4]);
                            buf[3] += (byte)(senha[5] << 4);
                        }

                        evento_close(BytesToHexString(buf));
                        
                        val = 0;
				        apagaBufferSenha();
                        atualizaTextBox();                        
                    }
		        break;
		
		        case TECLA_ESC:
			        apagaBufferSenha();
                    tratandoSenha = false;
                    textBox_unidade.Focus();
                    atualizaTextBox();
			        bip(); 
		        break;
		
		        default:		
			        if(( tratandoSenha == false )&&( checkBox_setup_setupGeral_senha13.Checked ))
			        {
				        unidade[6] = unidade[5];
				        unidade[5] = unidade[4];
				        unidade[4] = unidade[3];
				        unidade[3] = unidade[2];
				        unidade[2] = unidade[1];
				        unidade[1] = unidade[0];
				        unidade[0] = digito;
                    }
			        else
			        {
				        senha[5] = senha[4];
				        senha[4] = senha[3];
				        senha[3] = senha[2];
				        senha[2] = senha[1];
				        senha[1] = senha[0];
				        senha[0] = digito;
                    }
                    atualizaTextBox();
		        break;
	        }
            
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void TecladoSenha_Load(object sender, EventArgs e)
        {
            if (checkBox_setup_setupGeral_senha13.Checked == false) 
                textBox_unidade.Enabled = false;
            else
                textBox_unidade.Enabled = true;

            apagaBufferSenha();
            timer1.Start();
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public void apagaBufferSenha()
        {
            for (int i = 0; i < 7; i++) unidade[i] = 0;
            for (int i = 0; i < 6; i++) senha[i] = 0;
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public int toutDigitacao_1s = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (toutDigitacao_1s > 0)
            {
                toutDigitacao_1s--;
                if (toutDigitacao_1s == 0)
                {
                    apagaBufferSenha();
                }
            }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public void bip()
        {
            SystemSounds.Beep.Play();
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void TecladoSenha_FormClosed(object sender, FormClosedEventArgs e)
        {            
            timer1.Stop();
        }        
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public void atualizaTextBox()
        {
            textBox_unidade.Text = null;
            textBox_unidade.Text += unidade[6].ToString();
            textBox_unidade.Text += unidade[5].ToString();
            textBox_unidade.Text += unidade[4].ToString();
            textBox_unidade.Text += unidade[3].ToString();
            textBox_unidade.Text += unidade[2].ToString();
            textBox_unidade.Text += unidade[1].ToString();
            textBox_unidade.Text += unidade[0].ToString();

            textBox_senha.Text = null;
            textBox_senha.Text += senha[5].ToString();
            textBox_senha.Text += senha[4].ToString();
            textBox_senha.Text += senha[3].ToString();
            textBox_senha.Text += senha[2].ToString();
            textBox_senha.Text += senha[1].ToString();
            textBox_senha.Text += senha[0].ToString();
            
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                try
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                catch { MessageBox.Show("Valor inválido!!"); }
            }
            return sb.ToString();
        }
    }
}
