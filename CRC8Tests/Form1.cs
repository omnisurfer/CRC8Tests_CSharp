using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        ///
        /// This enum is used to indicate what kind of checksum you will be calculating.
        /// 
        public enum CRC8_POLY
        {
            CRC8 = 0xD8,
            CRC8_CCITT = 0x07,
            CRC8_DALLAS_MAXIM = 0x31,
            CRC8_SAE_J1850 = 0x1D,
            CRC_8_WCDMA = 0x9b,
        };

        /// 
        /// Class for calculating CRC8 checksums...
        /// 
        public class CRC8Calc
        {
            private byte[] table = new byte[256];

            public byte Checksum(params byte[] val)
            {
                if (val == null)
                    throw new ArgumentNullException("val");

                byte c = 0;

                foreach (byte b in val)
                {
                    c = table[c ^ b];
                }

                return c;
            }

            public byte[] Table
            {
                get
                {
                    return this.table;
                }
                set
                {
                    this.table = value;
                }
            }

            public byte[] GenerateTable(CRC8_POLY polynomial)
            {
                byte[] csTable = new byte[256];

                for (int i = 0; i < 256; ++i)
                {
                    int curr = i;

                    for (int j = 0; j < 8; ++j)
                    {
                        if ((curr & 0x80) != 0)
                        {
                            curr = (curr << 1) ^ (int)polynomial;
                        }
                        else
                        {
                            curr <<= 1;
                        }
                    }

                    csTable[i] = (byte)curr;
                }

                return csTable;
            }

            public CRC8Calc(CRC8_POLY polynomial)
            {
                this.table = this.GenerateTable(polynomial);
            }
        }

        private static void WL(object text, params object[] args)
        {
            Console.WriteLine(text.ToString()); 
            int temp = Convert.ToInt16(text.ToString());
            Console.WriteLine("CRC " + Convert.ToString(temp, 16).PadLeft(2, '0'));            
        }

        public static void RunSnippet()
        {
            byte checksum;
            byte[] testVal = new byte[] { 0x01 };
            //CRC8Calc crc_dallas = new CRC8Calc(CRC8_POLY.CRC8_DALLAS_MAXIM);
            //checksum = crc_dallas.Checksum(testVal);
            //WL(checksum);
            CRC8Calc crc = new CRC8Calc(CRC8_POLY.CRC8);
            checksum = crc.Checksum(testVal);
            WL(checksum);
        }

        byte POLYNOMIAL = 0xD8;

        public byte crcNaive(byte message)
        {
            byte remainder;
            /*
             * Initially, the dividend is the remainder.
             */
            remainder = message;

            /*
             * For each bit position in the message....
             */
            for (uint bit = 8; bit > 0; --bit)
            {
                /*
                 * If the uppermost bit is a 1...
                 */
                if ((byte)(remainder & 0x80) == 0x80)
                {
                    /*
                     * XOR the previous remainder with the divisor.
                     */
                    remainder ^= POLYNOMIAL;
                }

                /*
                 * Shift the next bit of the message into the remainder.
                 */
                remainder = (byte)(remainder << 1);
            }

            /*
             * Return only the relevant bits of the remainder as CRC.
             */
            return (byte)(remainder >> 4);
        }

        public Form1()
        {
            InitializeComponent();
        }

        int shiftValue = 0;

        public void DrawLinesPointF(PaintEventArgs e, int shift)
        {
            Random random = new Random();

            // Create pen.
            Pen penSin = new Pen(Color.Red, 2), penCos = new Pen(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)), 2);

            float yScale = 0.1F, yOffset = (pictureBox1.Bottom / 2), xScale = 2.0F, sinFreq = 1.0F, cosFreq = sinFreq * 1.0F, sinAmp = 2.5F, cosAmp = sinAmp * 1.0F;

            PointF[] pointsSin = new PointF[pictureBox1.Right], pointsCos = new PointF[pictureBox1.Right];

            for (int i = 0; i < pictureBox1.Right; i++)
            {
                pointsSin[i] = new PointF((i), (sinAmp * (yScale * pictureBox1.Bottom) * (float)Math.Sin((i * (sinFreq/(pictureBox1.Right/xScale)) * Math.PI) + shift)) + yOffset);                
            }

            for (int i = 0; i < pictureBox1.Right; i++)
            {
                pointsCos[i] = new PointF((i), (cosAmp * (yScale * pictureBox1.Bottom) * (float)Math.Cos((i * (cosFreq/(pictureBox1.Right/xScale)) * Math.PI) - (shift))) + yOffset);                              
            }

            //Draw lines to screen.
            e.Graphics.DrawLines(penSin, pointsSin);

            e.Graphics.DrawLines(penCos, pointsCos);
        }

        //private PictureBox pictureBox1 = new PictureBox();        

        private void Form1_Load(object sender, EventArgs e)
        {            
            int text = 0;
            for (int i = 0; i < 256; i++)
            {
                text = crcNaive((byte)i);                              
                Console.WriteLine("CRC Naive " + Convert.ToString(text, 16).PadLeft(2, '0'));                 
            }

            createData();

            // Dock the PictureBox to the form and set its background to white.
            //pictureBox1.Dock = DockStyle.Fill;
            //pictureBox1.BackColor = Color.White;
            
            // Connect the Paint event of the PictureBox to the event handler method.
            pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);

            // Add the PictureBox control to the Form. 
            //this.Controls.Add(pictureBox1);
            
            /*
            try
            {
                RunSnippet();
            }
            catch (Exception x)
            {
                string error = string.Format
                ("---\nThe following error occurred while executing the snippet:\n{0}\n---", e.ToString());
                Console.WriteLine(error);
            }
             */ 
        }

        private void createData()
        {
            float[] samples = new float[1024], sinBasis = new float[1024], cosBasis = new float[1024];

            float yScale = 0.1F, yOffset = (pictureBox1.Bottom / 2), xScale = 2.0F, sinFreq = 1.0F, cosFreq = sinFreq * 1.0F, sinAmp = 2.5F, cosAmp = sinAmp * 1.0F;

            Random randomSample = new Random();

            //simulated samples
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)randomSample.NextDouble() * yScale * pictureBox1.Bottom * sinAmp;
                sampleTextBox.AppendText(samples[i].ToString() + ",");
            }

            //sin & cos basis
            for (int i = 0; i < sinBasis.Length; i++)
            {
                sinBasis[i] = (sinAmp * (yScale * pictureBox1.Bottom) * (float)Math.Sin((i * (sinFreq / (pictureBox1.Right / xScale)) * Math.PI)) + yOffset);
                sinBasisTextBox.AppendText(sinBasis[i].ToString() + ",");

                cosBasis[i] = (sinAmp * (yScale * pictureBox1.Bottom) * (float)Math.Cos((i * (sinFreq*100 / (pictureBox1.Right / xScale)) * Math.PI)) + yOffset);
                cosBasisTextBox.AppendText(cosBasis[i].ToString() + ",");
            }                        
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            //pictureBox1.Invalidate(new Rectangle(100, 100, 100, 100));

            pictureBox1.Invalidate();

            pictureBox3.Hide();
            
            // Draw a line in the PictureBox.
            //g.DrawLine(System.Drawing.Pens.Blue, pictureBox1.Left, pictureBox1.Top,
                //pictureBox1.Right, pictureBox1.Bottom);
        }

        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {            
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;

            Random random = new Random();             
            
            //System.Diagnostics.Debug.WriteLine("Box1 Event Triggered!");

            int randomNumber = random.Next(0, pictureBox1.Bottom);

            // Draw a line in the PictureBox.
            g.DrawLine(System.Drawing.Pens.Purple, 0, (pictureBox1.Bottom / 2),
                pictureBox1.Right, (pictureBox1.Bottom / 2));
            
            DrawLinesPointF(e, shiftValue);
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {            
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;
            
            Random random = new Random();

            int randomNumber = random.Next(0, pictureBox1.Bottom);

            // Draw a line in the PictureBox.
            g.DrawLine(System.Drawing.Pens.Blue, pictureBox1.Left, pictureBox1.Top,
                pictureBox1.Right, randomNumber);                         
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox3.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox3.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (shiftValue > 255)
                shiftValue = 0;

            shiftValue++;

            pictureBox1.Refresh();
            pictureBox3.Refresh();

            //System.Diagnostics.Debug.WriteLine("TimerTicked!");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MouseDown!");
        }
    }
}
