using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using TIManager.Models;
using TIManager.Utils;

namespace TIManager.Views
{
    public class NovoChamadoForm : Form
    {
        public Chamado ChamadoAtual { get; private set; }
        private TextBox txtTitulo, txtSolicitante, txtDescricao, txtCaminhoImagem;
        private ComboBox cmbSetor, cmbPrioridade, cmbStatus;
        private RoundedButton btnSalvar, btnCancelar, btnAnexar, btnRemover;
        private bool ehEdicao;

        public NovoChamadoForm(Chamado chamado = null)
        {
            this.ChamadoAtual = chamado ?? new Chamado();
            this.ehEdicao = chamado != null;
            InitializeComponent();
            if (ehEdicao) PreencherCampos();
        }

        private void InitializeComponent()
        {
            this.Text = ehEdicao ? "Editar Chamado #" + ChamadoAtual.Id : "Abrir Novo Chamado";
            this.Size = new Size(600, 750);
            this.MinimumSize = new Size(500, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            var pnlFooter = new Panel { 
                Dock = DockStyle.Bottom, 
                Height = 85, 
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(0, 0, 20, 0)
            };
            
            var footerLine = new Label { 
                Dock = DockStyle.Top, 
                Height = 1, 
                BackColor = Color.FromArgb(230, 230, 230) 
            };
            pnlFooter.Controls.Add(footerLine);

            var pnlBotoesContainer = new Panel { Dock = DockStyle.Right, Width = 340 };
            
            btnCancelar = new RoundedButton { 
                Text = "Cancelar", 
                DialogResult = DialogResult.Cancel, 
                Size = new Size(150, 40), 
                Location = new Point(175, 25),
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                BorderColor = Color.LightGray,
                BorderSize = 1,
                BorderRadius = 20
            };

            btnSalvar = new RoundedButton { 
                Text = "Salvar", 
                DialogResult = DialogResult.OK, 
                Size = new Size(150, 40), 
                Location = new Point(10, 25),
                BackColor = ColorTranslator.FromHtml("#88C425"), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 20
            };
            
            pnlBotoesContainer.Controls.Add(btnSalvar);
            pnlBotoesContainer.Controls.Add(btnCancelar);
            pnlFooter.Controls.Add(pnlBotoesContainer);
            this.Controls.Add(pnlFooter);

            var tblLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.White };
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 500F));
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var container = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20, 30, 20, 30), AutoScroll = true, BackColor = Color.White };

            var lblFormTitle = new Label {
                Text = ehEdicao ? "DETALHES DA EDIÇÃO" : "NOVA SOLICITAÇÃO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 25)
            };
            container.Controls.Add(lblFormTitle);

            txtTitulo = AddInputField(container, "Título do Chamado:");
            txtSolicitante = AddInputField(container, "Nome do Solicitante:");
            cmbSetor = AddComboField(container, "Setor Solicitante:", new string[] { 
                "Coordenação de Projetos", "Logística", "TI", "Contabilidade", 
                "Financeiro", "Prestação de Contas", "RH/Departamento Pessoal", 
                "Diretoria", "CERII", "TEA", "Oficinas", "Portaria", 
                "Call Center", "Loja", "Infantojuvenil", "CAPS" 
            });
            cmbPrioridade = AddComboField(container, "Prioridade:", new string[] { "Baixa", "Média", "Alta", "Crítica" });

            if (ehEdicao) 
                cmbStatus = AddComboField(container, "Status do Chamado:", new string[] { "Aberto", "Em Atendimento", "Finalizado" });
            
            txtDescricao = AddInputField(container, "Descrição do Problema:", true);

            var lblImg = new Label { Text = "Anexo de Imagem:", AutoSize = true, Margin = new Padding(0, 20, 0, 5), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1B676B") };
            container.Controls.Add(lblImg);

            var pnlAnexo = new FlowLayoutPanel { Width = 450, Height = 55, Margin = new Padding(0) };
            txtCaminhoImagem = new TextBox { Width = 150, ReadOnly = true, Margin = new Padding(0, 8, 5, 0), Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(245, 245, 245) };
            
            btnAnexar = new RoundedButton { 
                Text = "Anexar...", 
                Size = new Size(130, 40), 
                BackColor = Color.White,
                ForeColor = Color.FromArgb(64,64,64),
                BorderColor = Color.LightGray,
                BorderSize = 1,
                BorderRadius = 20,
                Margin = new Padding(0, 0, 5, 0)
            };

            btnRemover = new RoundedButton { 
                Text = "Remover", 
                Size = new Size(130, 40), 
                BackColor = Color.MistyRose,
                ForeColor = Color.DarkRed,
                BorderColor = Color.LightPink,
                BorderSize = 1,
                BorderRadius = 20,
                Margin = new Padding(0)
            };
            
            btnAnexar.Click += BtnAnexar_Click;
            btnRemover.Click += (s, e) => { txtCaminhoImagem.Clear(); caminhoImagemTemporario = ""; ChamadoAtual.CaminhoImagem = ""; };

            pnlAnexo.Controls.Add(txtCaminhoImagem);
            pnlAnexo.Controls.Add(btnAnexar);
            pnlAnexo.Controls.Add(btnRemover);
            container.Controls.Add(pnlAnexo);

            tblLayout.Controls.Add(container, 1, 0);
            this.Controls.Add(tblLayout);
            btnSalvar.Click += BtnSalvar_Click;
        }

        private void PreencherCampos()
        {
            txtTitulo.Text = ChamadoAtual.Titulo;
            txtSolicitante.Text = ChamadoAtual.Solicitante;
            cmbSetor.Text = ChamadoAtual.Setor;
            cmbPrioridade.Text = ChamadoAtual.Prioridade;
            txtDescricao.Text = ChamadoAtual.Descricao;
            txtCaminhoImagem.Text = Path.GetFileName(ChamadoAtual.CaminhoImagem);
            if (cmbStatus != null) cmbStatus.Text = ChamadoAtual.Status;
        }

        private TextBox AddInputField(FlowLayoutPanel parent, string labelText, bool isMultiline = false)
        {
            var lbl = new Label { Text = labelText, AutoSize = true, Margin = new Padding(0, 10, 0, 5), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var txt = new TextBox { Width = 450, Font = new Font("Segoe UI", 11) };
            if (isMultiline) 
            { 
                txt.Multiline = true; 
                txt.Height = 150; 
                txt.ScrollBars = ScrollBars.Vertical;
            }
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private ComboBox AddComboField(FlowLayoutPanel parent, string labelText, string[] items)
        {
            var lbl = new Label { Text = labelText, AutoSize = true, Margin = new Padding(0, 10, 0, 5), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var cmb = new ComboBox { Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cmb.Items.AddRange(items);
            parent.Controls.Add(lbl);
            parent.Controls.Add(cmb);
            return cmb;
        }

        private string caminhoImagemTemporario = "";
        private void BtnAnexar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp" }) {
                if (ofd.ShowDialog() == DialogResult.OK) {
                    txtCaminhoImagem.Text = Path.GetFileName(ofd.FileName);
                    caminhoImagemTemporario = ofd.FileName;
                }
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            ChamadoAtual.Titulo = txtTitulo.Text;
            ChamadoAtual.Solicitante = txtSolicitante.Text;
            ChamadoAtual.Setor = cmbSetor.Text;
            ChamadoAtual.Prioridade = cmbPrioridade.Text;
            ChamadoAtual.Descricao = txtDescricao.Text;
            
            string statusAnterior = ChamadoAtual.Status;
            if (cmbStatus != null) ChamadoAtual.Status = cmbStatus.Text;

            // Gerencia Data de Finalização
            if (ChamadoAtual.Status == "Finalizado")
            {
                if (statusAnterior != "Finalizado" || ChamadoAtual.DataFinalizacao == null)
                {
                    ChamadoAtual.DataFinalizacao = DateTime.Now;
                }
            }
            else
            {
                ChamadoAtual.DataFinalizacao = null;
            }

            // Gerencia Persistência de Imagem
            if (!string.IsNullOrEmpty(caminhoImagemTemporario))
            {
                try
                {
                    string pastaDestino = @"\\192.168.10.152\ciras\TI\.TIManagerData\Imagens";
                    if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                    string extensao = Path.GetExtension(caminhoImagemTemporario);
                    string novoNome = $"img_{Guid.NewGuid()}{extensao}";
                    string caminhoDestino = Path.Combine(pastaDestino, novoNome);

                    File.Copy(caminhoImagemTemporario, caminhoDestino, true);
                    ChamadoAtual.CaminhoImagem = caminhoDestino;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao salvar imagem: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
