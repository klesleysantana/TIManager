using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TIManager.Models;
using TIManager.Data;
using TIManager.Utils;

namespace TIManager.Views
{
    public class WikiControl : UserControl
    {
        private FlowLayoutPanel pnlLibrary;
        private Panel pnlContainer;
        private PdfViewControl pdfViewer;
        private RoundedButton btnAdicionar;
        private string _categoria = "Wiki";

        public WikiControl()
        {
            InitializeComponent();
            CarregarDocumentos();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 248, 248);

            pnlContainer = new Panel { Dock = DockStyle.Fill };
            
            pdfViewer = new PdfViewControl { Visible = false };
            pdfViewer.OnVoltar += (s, e) => {
                pdfViewer.Visible = false;
                pnlLibrary.Visible = true;
                pnlHeader().Visible = true;
            };

            var pnlHeaderControl = new Panel { Name = "pnlHeader", Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };

            var label = new Label
            {
                Text = "Base de Conhecimento (Wiki)",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                AutoSize = true,
                Dock = DockStyle.Left
            };

            btnAdicionar = new RoundedButton
            {
                Text = "+ Adicionar Documento",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(200, 40),
                BackColor = ColorTranslator.FromHtml("#88C425"),
                ForeColor = Color.White,
                BorderRadius = 20,
                Dock = DockStyle.Right
            };
            btnAdicionar.Click += BtnAdicionar_Click;

            pnlHeaderControl.Controls.Add(label);
            pnlHeaderControl.Controls.Add(btnAdicionar);

            pnlLibrary = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 0, 20, 20),
                BackColor = Color.Transparent
            };

            pnlContainer.Controls.Add(pnlLibrary);
            pnlContainer.Controls.Add(pnlHeaderControl);
            pnlContainer.Controls.Add(pdfViewer);

            this.Controls.Add(pnlContainer);
        }

        private Panel pnlHeader() => (Panel)pnlContainer.Controls["pnlHeader"];

        private void CarregarDocumentos()
        {
            pnlLibrary.Controls.Clear();
            var docs = DatabaseService.Instance.GetAllDocumentos(_categoria);

            foreach (var doc in docs)
            {
                var card = CriarCardDocumento(doc);
                pnlLibrary.Controls.Add(card);
            }

            if (docs.Count == 0)
            {
                var lblVazio = new Label
                {
                    Text = "Nenhum documento encontrado. Clique em '+ Adicionar Documento' para começar.",
                    Font = new Font("Segoe UI", 12, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(10, 20, 0, 0)
                };
                pnlLibrary.Controls.Add(lblVazio);
            }
        }

        private Panel CriarCardDocumento(Documento doc)
        {
            var card = new Panel
            {
                Width = 180,
                Height = 260,
                Margin = new Padding(15),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    g.DrawRectangle(pen, rect);
            };

            Control pic;
            if (!string.IsNullOrEmpty(doc.CaminhoMiniatura) && File.Exists(doc.CaminhoMiniatura))
            {
                pic = new PictureBox
                {
                    Image = Image.FromFile(doc.CaminhoMiniatura),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Top,
                    Height = 160,
                    BackColor = Color.FromArgb(240, 240, 240)
                };
            }
            else
            {
                pic = new Label
                {
                    Text = "📄",
                    Font = new Font("Segoe UI", 48),
                    ForeColor = Color.IndianRed,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 160,
                    BackColor = Color.FromArgb(250, 250, 250)
                };
            }

            var lblNome = new Label
            {
                Text = doc.Nome,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Padding = new Padding(5, 10, 5, 0),
                AutoEllipsis = true
            };

            var lblData = new Label
            {
                Text = doc.DataUpload.ToString("dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblNome);
            card.Controls.Add(pic);
            card.Controls.Add(lblData);

            EventHandler clickHandler = (s, e) => AbrirDocumento(doc);
            card.Click += clickHandler;
            pic.Click += clickHandler;
            lblNome.Click += clickHandler;
            lblData.Click += clickHandler;

            return card;
        }

        private void BtnAdicionar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Arquivos PDF (*.pdf)|*.pdf";
                ofd.Title = "Selecione um documento para a Wiki";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string originalPath = ofd.FileName;
                        string originalName = Path.GetFileName(originalPath);
                        
                        string folderPath = @"C:\TIManagerData\Documentos";
                        string newFileName = $"{Guid.NewGuid()}_{originalName}";
                        string destPath = Path.Combine(folderPath, newFileName);

                        File.Copy(originalPath, destPath);

                        // Gera miniatura
                        string thumbPath = PdfHelper.GerarMiniatura(destPath);

                        var doc = new Documento
                        {
                            Nome = Path.GetFileNameWithoutExtension(originalName),
                            Categoria = _categoria,
                            CaminhoArquivo = destPath,
                            CaminhoMiniatura = thumbPath
                        };

                        DatabaseService.Instance.InsertDocumento(doc);
                        CarregarDocumentos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao salvar o documento: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AbrirDocumento(Documento doc)
        {
            if (File.Exists(doc.CaminhoArquivo))
            {
                pnlLibrary.Visible = false;
                pnlHeader().Visible = false;
                pdfViewer.Visible = true;
                pdfViewer.BringToFront();
                pdfViewer.CarregarPdf(doc.CaminhoArquivo, doc.Nome);
            }
            else
            {
                MessageBox.Show("Arquivo não encontrado.");
            }
        }
    }
}
