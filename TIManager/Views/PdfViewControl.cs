using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using TIManager.Utils;

namespace TIManager.Views
{
    public class PdfViewControl : UserControl
    {
        private WebView2 webView;
        private Panel pnlHeader;
        private RoundedButton btnVoltar;
        private Label lblTitle;
        
        public event EventHandler OnVoltar;

        public PdfViewControl()
        {
            InitializeComponent();
        }

        private async void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 248, 248),
                Padding = new Padding(20, 10, 20, 10)
            };

            btnVoltar = new RoundedButton
            {
                Text = "← Voltar para Estante",
                Size = new Size(180, 40),
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                BorderColor = Color.LightGray,
                BorderSize = 1,
                BorderRadius = 20,
                Dock = DockStyle.Left
            };
            btnVoltar.Click += (s, e) => OnVoltar?.Invoke(this, EventArgs.Empty);

            lblTitle = new Label
            {
                Text = "Visualizador de Documento",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnVoltar);

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(webView);
            this.Controls.Add(pnlHeader);

            try
            {
                await webView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inicializar o visualizador: " + ex.Message);
            }
        }

        public void CarregarPdf(string path, string titulo)
        {
            lblTitle.Text = titulo;
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
            }
            else
            {
                webView.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    webView.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
                };
            }
        }

        public async void CarregarHtmlComoPdf(string htmlPath, string titulo)
        {
            lblTitle.Text = titulo + " (Gerando PDF...)";
            
            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }

            // Define o tratador de evento uma única vez
            EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
            handler = async (s, e) =>
            {
                webView.NavigationCompleted -= handler; // Desinscreve para evitar loop infinito

                if (e.IsSuccess)
                {
                    try
                    {
                        string pdfPath = Path.Combine(Path.GetTempPath(), $"relatorio_{Guid.NewGuid()}.pdf");
                        
                        var printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
                        printSettings.Orientation = CoreWebView2PrintOrientation.Landscape;
                        printSettings.ShouldPrintBackgrounds = true;

                        await webView.CoreWebView2.PrintToPdfAsync(pdfPath, printSettings);
                        
                        lblTitle.Text = titulo;
                        webView.CoreWebView2.Navigate(new Uri(pdfPath).AbsoluteUri);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao converter relatório: " + ex.Message);
                    }
                }
            };

            webView.NavigationCompleted += handler;
            webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
        }
    }
}
