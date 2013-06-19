#region Copyright (C)
/* 
 *	Copyright (C) 2006-2012 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace ShowLicense
{
    public partial class License : Form
    {
        System.Windows.Forms.Timer m_timeOutTimer = new System.Windows.Forms.Timer();


        public License(string licensefile)
        {
            
            InitializeComponent();
            

            string[] all_lines = null;

            if (File.Exists(licensefile) == true)
            {
                all_lines = File.ReadAllLines(licensefile);
            }
            

            foreach (string s in all_lines)
            {
                listBox1.Items.Add(s);
            }
            return ;
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonDisagree_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
    }
}
