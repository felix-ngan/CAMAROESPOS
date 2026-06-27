using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmStore : Form, IStoreView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly StorePresenter _presenter;

        private PictureBox picLogo;
        private Button btnBrowse;
        private Button btnRemove;
        private byte[] _logoBytes;

        private TextBox txtMomo;
        private TextBox txtOrange;
        private Label lblMomo;
        private Label lblOrange;

        private TextBox txtStoreId;
        private TextBox txtRegisterId;
        private TextBox txtCentralConnection;
        private Label lblStoreId;
        private Label lblRegisterId;
        private Label lblCentralConnection;

        public frmStore()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            InitializeCustomControls();
            
            var repository = new Data.StoreRepository(dbcon.MyConnection());
            _presenter = new StorePresenter(this, repository);
            this.KeyPreview = true;
        }

        private void InitializeCustomControls()
        {
            // Increase window height and shift buttons
            this.ClientSize = new System.Drawing.Size(832, 345);
            btnSave.Location = new System.Drawing.Point(165, 290);
            button3.Location = new System.Drawing.Point(268, 290);

            // picLogo
            picLogo = new PictureBox();
            picLogo.Name = "picLogo";
            picLogo.Location = new System.Drawing.Point(670, 50);
            picLogo.Size = new System.Drawing.Size(140, 95);
            picLogo.BorderStyle = BorderStyle.FixedSingle;
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;

            // btnBrowse
            btnBrowse = new Button();
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Text = "Logo...";
            btnBrowse.Location = new System.Drawing.Point(670, 150);
            btnBrowse.Size = new System.Drawing.Size(68, 30);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.FlatAppearance.BorderSize = 1;
            btnBrowse.Click += BtnBrowse_Click;

            // btnRemove
            btnRemove = new Button();
            btnRemove.Name = "btnRemove";
            btnRemove.Text = "Effacer";
            btnRemove.Location = new System.Drawing.Point(742, 150);
            btnRemove.Size = new System.Drawing.Size(68, 30);
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.FlatAppearance.BorderSize = 1;
            btnRemove.Click += BtnRemove_Click;

            // MoMo Controls
            lblMomo = new Label();
            lblMomo.Text = "Code MTN MoMo";
            lblMomo.Location = new System.Drawing.Point(62, 150);
            lblMomo.Size = new System.Drawing.Size(100, 20);

            txtMomo = new TextBox();
            txtMomo.Location = new System.Drawing.Point(165, 147);
            txtMomo.Size = new System.Drawing.Size(490, 25);

            // Orange Controls
            lblOrange = new Label();
            lblOrange.Text = "Code Orange Money";
            lblOrange.Location = new System.Drawing.Point(62, 185);
            lblOrange.Size = new System.Drawing.Size(120, 20);

            txtOrange = new TextBox();
            txtOrange.Location = new System.Drawing.Point(165, 182);
            txtOrange.Size = new System.Drawing.Size(490, 25);

            // Store ID Controls
            lblStoreId = new Label();
            lblStoreId.Text = "ID Boutique";
            lblStoreId.Location = new System.Drawing.Point(62, 217);
            lblStoreId.Size = new System.Drawing.Size(100, 20);

            txtStoreId = new TextBox();
            txtStoreId.Location = new System.Drawing.Point(165, 214);
            txtStoreId.Size = new System.Drawing.Size(120, 25);

            // Register ID Controls
            lblRegisterId = new Label();
            lblRegisterId.Text = "ID Caisse";
            lblRegisterId.Location = new System.Drawing.Point(310, 217);
            lblRegisterId.Size = new System.Drawing.Size(100, 20);

            txtRegisterId = new TextBox();
            txtRegisterId.Location = new System.Drawing.Point(420, 214);
            txtRegisterId.Size = new System.Drawing.Size(235, 25);

            // Central Connection String Controls
            lblCentralConnection = new Label();
            lblCentralConnection.Text = "Connexion Base Centrale";
            lblCentralConnection.Location = new System.Drawing.Point(62, 252);
            lblCentralConnection.Size = new System.Drawing.Size(100, 20);

            txtCentralConnection = new TextBox();
            txtCentralConnection.Location = new System.Drawing.Point(165, 249);
            txtCentralConnection.Size = new System.Drawing.Size(490, 25);

            this.Controls.Add(picLogo);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnRemove);
            this.Controls.Add(lblMomo);
            this.Controls.Add(txtMomo);
            this.Controls.Add(lblOrange);
            this.Controls.Add(txtOrange);
            this.Controls.Add(lblStoreId);
            this.Controls.Add(txtStoreId);
            this.Controls.Add(lblRegisterId);
            this.Controls.Add(txtRegisterId);
            this.Controls.Add(lblCentralConnection);
            this.Controls.Add(txtCentralConnection);
        }

        public string StoreName
        {
            get => txtStore.Text;
            set => txtStore.Text = value;
        }

        public string StoreAddress
        {
            get => txtAddress.Text;
            set => txtAddress.Text = value;
        }

        public byte[] StoreLogo
        {
            get => _logoBytes;
            set
            {
                _logoBytes = value;
                if (_logoBytes != null && _logoBytes.Length > 0)
                {
                    try
                    {
                        using (var ms = new System.IO.MemoryStream(_logoBytes))
                        {
                            picLogo.Image = System.Drawing.Image.FromStream(ms);
                        }
                    }
                    catch
                    {
                        picLogo.Image = null;
                    }
                }
                else
                {
                    picLogo.Image = null;
                }
            }
        }

        public string MomoCode
        {
            get => txtMomo.Text;
            set => txtMomo.Text = value;
        }

        public string OrangeCode
        {
            get => txtOrange.Text;
            set => txtOrange.Text = value;
        }

        public string StoreId
        {
            get => txtStoreId.Text;
            set => txtStoreId.Text = value;
        }

        public string RegisterId
        {
            get => txtRegisterId.Text;
            set => txtRegisterId.Text = value;
        }

        public string CentralConnection
        {
            get => txtCentralConnection.Text;
            set => txtCentralConnection.Text = value;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fichiers Image (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
                ofd.Title = "Sélectionner le Logo de la Boutique";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(ofd.FileName);
                        if (bytes.Length > 2 * 1024 * 1024)
                        {
                            MessageBox.Show("L'image est trop volumineuse (max 2 Mo).", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        StoreLogo = bytes;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la lecture du fichier : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            StoreLogo = null;
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void LoadRecords()
        {
            _presenter.LoadStoreDetails();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.SaveStoreDetails();
        }

        private void frmStore_Load(object sender, EventArgs e)
        {
            txtStore.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txtStore.Clear();
            txtAddress.Clear();
            CloseView();
        }

        private void frmStore_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseView();
            }
        }
    }
}
