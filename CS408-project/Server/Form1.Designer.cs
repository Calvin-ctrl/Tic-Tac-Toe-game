namespace TicTacToe_Server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            port_number=new Label();
            port_text_box=new TextBox();
            listen_port_button=new Button();
            logs=new RichTextBox();
            SuspendLayout();
            // 
            // port_number
            // 
            port_number.AutoSize=true;
            port_number.Location=new Point(12, 29);
            port_number.Name="port_number";
            port_number.Size=new Size(96, 20);
            port_number.TabIndex=0;
            port_number.Text="Port Number:";
            // 
            // port_text_box
            // 
            port_text_box.Location=new Point(114, 26);
            port_text_box.Name="port_text_box";
            port_text_box.Size=new Size(188, 27);
            port_text_box.TabIndex=1;
            // 
            // listen_port_button
            // 
            listen_port_button.AutoEllipsis=true;
            listen_port_button.Location=new Point(329, 26);
            listen_port_button.Name="listen_port_button";
            listen_port_button.Size=new Size(94, 29);
            listen_port_button.TabIndex=2;
            listen_port_button.Text="listen";
            listen_port_button.UseVisualStyleBackColor=true;
            listen_port_button.Click+=listen_port_button_Click;
            // 
            // logs
            // 
            logs.Location=new Point(12, 78);
            logs.Name="logs";
            logs.Size=new Size(411, 291);
            logs.TabIndex=3;
            logs.Text="";
            // 
            // Form1
            // 
            AutoScaleDimensions=new SizeF(8F, 20F);
            AutoScaleMode=AutoScaleMode.Font;
            ClientSize=new Size(800, 450);
            Controls.Add(logs);
            Controls.Add(listen_port_button);
            Controls.Add(port_text_box);
            Controls.Add(port_number);
            Name="Form1";
            Text="Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label port_number;
        private TextBox port_text_box;
        private Button listen_port_button;
        private RichTextBox logs;
    }
}