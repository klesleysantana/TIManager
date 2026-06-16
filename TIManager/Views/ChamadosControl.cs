using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using TIManager.Models;
using TIManager.Utils;

namespace TIManager.Views
{
    public class ChamadosControl : UserControl
    {
        private Panel pnlHeader, pnlContainerGeral, pnlDetalhes;
        private Panel pnlListaScroll; // Mudamos de FlowLayoutPanel para Panel comum
        private RoundedButton btnNovoChamado;
        private List<Chamado> listaChamados = new List<Chamado>();
        private string filtroAtual = "Todos";

        public ChamadosControl()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            pnlContainerGeral = new Panel { Dock = DockStyle.Fill };
            pnlDetalhes = new Panel { Dock = DockStyle.Fill, Visible = false };

            this.Controls.Add(pnlDetalhes);
            this.Controls.Add(pnlContainerGeral);

            ConfigurarHeader();

            pnlListaScroll = new Panel { 
                Dock = DockStyle.Fill, 
                AutoScroll = true,
                Padding = new Padding(20, 0, 20, 20)
            };
            UIHelper.SetDoubleBuffered(pnlListaScroll);
            
            pnlContainerGeral.Controls.Add(pnlListaScroll);
            pnlContainerGeral.Controls.Add(pnlHeader);
        }

        private void ConfigurarHeader()
        {
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20, 20, 20, 10) };
            
            var lblTitle = new Label { 
                Text = "Chamados", 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                Dock = DockStyle.Left, 
                AutoSize = true 
            };

            var lblDica = new Label {
                Text = "Dica: Clique no ícone 👁 ou clique duas vezes para ver detalhes",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Location = new Point(25, 65),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblDica);

            var pnlBtns = new Panel { Dock = DockStyle.Right, Width = 320 };

            var btnImprimir = new RoundedButton {
                Text = "🖨 Imprimir",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(130, 40),
                Location = new Point(10, 0),
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                BorderColor = ColorTranslator.FromHtml("#1B676B"),
                BorderSize = 1,
                BorderRadius = 20
            };
            btnImprimir.Click += (s, e) => ImprimirRelatorio();
            
            btnNovoChamado = new RoundedButton {
                Text = "+ Novo Chamado",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(150, 0),
                BackColor = ColorTranslator.FromHtml("#88C425"),
                ForeColor = Color.White,
                BorderRadius = 20
            };
            btnNovoChamado.Click += BtnNovoChamado_Click;

            pnlBtns.Controls.Add(btnImprimir);
            pnlBtns.Controls.Add(btnNovoChamado);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(pnlBtns);
        }

        private void CarregarDados()
        {
            listaChamados = TIManager.Data.DatabaseService.Instance.GetAllChamados();
            AtualizarGrid();
        }

        private void BtnNovoChamado_Click(object sender, EventArgs e)
        {
            using (var form = new NovoChamadoForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var novo = form.ChamadoAtual;
                    TIManager.Data.DatabaseService.Instance.InsertChamado(novo);
                    CarregarDados();
                }
            }
        }

        public void FiltrarPorStatus(string status)
        {
            filtroAtual = status;
            AtualizarGrid();
        }

        public void SelecionarEAbrirChamado(int id)
        {
            var chamado = listaChamados.FirstOrDefault(c => c.Id == id);
            if (chamado != null)
            {
                AbrirDetalhesInterno(chamado);
            }
        }

        private void AtualizarGrid()
        {
            pnlListaScroll.Controls.Clear();

            var chamadosFiltrados = filtroAtual == "Todos" 
                ? listaChamados 
                : listaChamados.FindAll(c => c.Status == filtroAtual);

            // Agrupa por Ano e Mês (Ordenado do mais antigo para o mais recente para o Dock.Top funcionar)
            var grupos = chamadosFiltrados
                .OrderBy(c => c.DataAbertura) // Ordem direta pois o Dock.Top inverte o empilhamento
                .GroupBy(c => new { c.DataAbertura.Year, c.DataAbertura.Month });

            foreach (var grupo in grupos)
            {
                string nomeMes = new DateTime(grupo.Key.Year, grupo.Key.Month, 1).ToString("MMMM / yyyy").ToUpper();
                
                // Grid para este mês
                var dgv = CriarGridCustomizada();
                dgv.DataSource = grupo.OrderByDescending(c => c.DataAbertura).ToList();
                
                // Ajustar altura da grid conforme número de linhas
                int altura = (grupo.Count() * 35) + 50; 
                dgv.Height = Math.Max(altura, 100);
                dgv.Dock = DockStyle.Top; // MÁGICA: O Windows redimensiona automaticamente agora!

                // Container para a Grid com margem
                var pnlGridWrapper = new Panel { Dock = DockStyle.Top, Height = dgv.Height + 10, Padding = new Padding(0, 0, 0, 10) };
                pnlGridWrapper.Controls.Add(dgv);

                // Label do Mês
                var lblMes = new Label {
                    Text = nomeMes,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = ColorTranslator.FromHtml("#1B676B"),
                    Dock = DockStyle.Top,
                    Height = 40,
                    TextAlign = ContentAlignment.BottomLeft
                };

                // Adiciona ao painel (como é Dock.Top, o último adicionado fica no topo)
                pnlListaScroll.Controls.Add(pnlGridWrapper);
                pnlListaScroll.Controls.Add(lblMes);
            }
        }

        private DataGridView CriarGridCustomizada()
        {
            var dgv = new DataGridView {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(240, 240, 240),
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.None,
                Cursor = Cursors.Hand
            };

            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 245, 233);
            dgv.DefaultCellStyle.SelectionForeColor = ColorTranslator.FromHtml("#1B676B");
            
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#1B676B");
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 35;
            dgv.RowTemplate.Height = 35;

            UIHelper.SetDoubleBuffered(dgv);

            // Context Menu
            var cms = new ContextMenuStrip();
            var itemEditar = new ToolStripMenuItem("Editar Chamado");
            itemEditar.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    var chamado = (Chamado)dgv.SelectedRows[0].DataBoundItem;
                    AbrirFormEditar(chamado);
                }
            };
            cms.Items.Add(itemEditar);
            dgv.ContextMenuStrip = cms;

            dgv.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Right) {
                    var hit = dgv.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0) {
                        dgv.ClearSelection();
                        dgv.Rows[hit.RowIndex].Selected = true;
                    }
                }
            };

            dgv.CellFormatting += (s, e) => {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0) {
                    string colName = dgv.Columns[e.ColumnIndex].Name;
                    if (colName == "Prioridade" || colName == "Status") {
                        string val = e.Value?.ToString() ?? "";
                        e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                        if (colName == "Prioridade") {
                            if (val == "Alta" || val == "Crítica") e.CellStyle.ForeColor = Color.Red;
                            else if (val == "Média") e.CellStyle.ForeColor = Color.DarkOrange;
                            else e.CellStyle.ForeColor = Color.Green;
                            
                            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor; // Mantém a cor na seleção
                        } else {
                            if (val == "Aberto") e.CellStyle.ForeColor = Color.DodgerBlue;
                            else if (val == "Em Atendimento") e.CellStyle.ForeColor = Color.BlueViolet;
                            else e.CellStyle.ForeColor = Color.Gray;

                            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor; // Mantém a cor na seleção
                        }
                    }
                }
            };

            dgv.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0) {
                    var chamado = (Chamado)dgv.Rows[e.RowIndex].DataBoundItem;
                    AbrirDetalhesInterno(chamado);
                }
            };

            dgv.DataBindingComplete += (s, e) => {
                if (!dgv.Columns.Contains("Ver")) {
                    var btnCol = new DataGridViewButtonColumn {
                        Name = "Ver", HeaderText = "", Text = "👁",
                        UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat
                    };
                    btnCol.DefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                    btnCol.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#1B676B");
                    btnCol.Resizable = DataGridViewTriState.False;
                    btnCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns.Add(btnCol);
                }

                if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
                if (dgv.Columns.Contains("CaminhoImagem")) dgv.Columns["CaminhoImagem"].Visible = false;
                if (dgv.Columns.Contains("Descricao")) dgv.Columns["Descricao"].Visible = false;

                // Configuração de Ordem e Nomes amigáveis
                if (dgv.Columns.Contains("DataAbertura")) {
                    dgv.Columns["DataAbertura"].HeaderText = "Data Abertura";
                    dgv.Columns["DataAbertura"].DisplayIndex = 0;
                }
                if (dgv.Columns.Contains("Titulo")) dgv.Columns["Titulo"].DisplayIndex = 1;
                if (dgv.Columns.Contains("Solicitante")) dgv.Columns["Solicitante"].DisplayIndex = 2;
                if (dgv.Columns.Contains("Setor")) dgv.Columns["Setor"].DisplayIndex = 3;
                if (dgv.Columns.Contains("Prioridade")) dgv.Columns["Prioridade"].DisplayIndex = 4;
                if (dgv.Columns.Contains("Status")) dgv.Columns["Status"].DisplayIndex = 5;
                if (dgv.Columns.Contains("DataFinalizacao")) {
                    dgv.Columns["DataFinalizacao"].HeaderText = "Data Fechamento";
                    dgv.Columns["DataFinalizacao"].DisplayIndex = 6;
                }

                if (dgv.Columns.Contains("Ver")) dgv.Columns["Ver"].DisplayIndex = 7;

                foreach (DataGridViewColumn col in dgv.Columns) {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            };

            dgv.CellContentClick += (s, e) => {
                if (dgv.Columns.Contains("Ver") && e.ColumnIndex == dgv.Columns["Ver"].Index && e.RowIndex >= 0) {
                    var chamado = (Chamado)dgv.Rows[e.RowIndex].DataBoundItem;
                    AbrirDetalhesInterno(chamado);
                }
            };

            return dgv;
        }

        private void AbrirDetalhesInterno(Chamado chamado)
        {
            pnlDetalhes.Controls.Clear();
            var detalhes = new ChamadoDetalhesControl(chamado);
            detalhes.OnVoltar += (s, ev) => { pnlDetalhes.Visible = false; pnlContainerGeral.Visible = true; };
            pnlDetalhes.Controls.Add(detalhes);
            pnlContainerGeral.Visible = false;
            pnlDetalhes.Visible = true;
        }

        private bool AbrirFormEditar(Chamado chamado)
        {
            using (var form = new NovoChamadoForm(chamado)) {
                if (form.ShowDialog() == DialogResult.OK) {
                    TIManager.Data.DatabaseService.Instance.UpdateChamado(form.ChamadoAtual);
                    CarregarDados();
                    return true;
                }
            }
            return false;
        }

        private void ImprimirRelatorio()
        {
            var picker = new MonthYearPickerForm(DateTime.Now.Year);
            
            // Posiciona o picker perto do mouse ou do botão
            Point location = Cursor.Position;
            picker.Location = new Point(location.X - 125, location.Y);

            if (picker.ShowDialog() == DialogResult.Cancel && picker.Confirmed)
            {
                // Note: ShowDialog is used here, but Deactivate closes it. 
                // Using a slightly different approach for popup behavior:
            }

            // Na verdade, como Deactivate fecha o form, ShowDialog retornará.
            if (picker.Confirmed)
            {
                var chamadosDoMes = listaChamados.FindAll(c => 
                    c.DataAbertura.Month == picker.SelectedMonth && 
                    c.DataAbertura.Year == picker.SelectedYear);

                if (chamadosDoMes.Count == 0)
                {
                    MessageBox.Show("Não há chamados para o período selecionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string mesNome = new DateTime(picker.SelectedYear, picker.SelectedMonth, 1).ToString("MMMM / yyyy");
                GerarRelatorioPdf(chamadosDoMes, mesNome);
            }
        }

        private void GerarRelatorioPdf(List<Chamado> chamados, string mesAno)
        {
            try
            {
                string html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', sans-serif; padding: 40px; color: #333; }}
                        h1 {{ color: #1B676B; border-bottom: 2px solid #1B676B; padding-bottom: 10px; margin-bottom: 5px; }}
                        h2 {{ color: #555; margin-top: 0; font-weight: normal; }}
                        table {{ width: 100%; border-collapse: collapse; margin-top: 25px; }}
                        th {{ background-color: #1B676B; color: white; padding: 12px 10px; text-align: left; font-size: 13px; text-transform: uppercase; }}
                        td {{ border-bottom: 1px solid #eee; padding: 12px 10px; font-size: 12px; }}
                        tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .footer {{ margin-top: 40px; font-size: 11px; color: #999; text-align: center; border-top: 1px solid #eee; padding-top: 10px; }}
                        .status-aberto {{ color: #1E90FF; font-weight: bold; }}
                        .status-atendimento {{ color: #8A2BE2; font-weight: bold; }}
                        .status-finalizado {{ color: #808080; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <h1>RELATÓRIO DE CHAMADOS</h1>
                    <h2>Mês de Referência: {mesAno.ToUpper()}</h2>
                    <table>
                        <thead>
                            <tr>
                                <th>DATA ABERTURA</th>
                                <th>TÍTULO</th>
                                <th>SOLICITANTE</th>
                                <th>SETOR</th>
                                <th>STATUS</th>
                                <th>DATA FECHAMENTO</th>
                            </tr>
                        </thead>
                        <tbody>";

                foreach (var c in chamados)
                {
                    string statusClass = c.Status == "Aberto" ? "status-aberto" : (c.Status == "Em Atendimento" ? "status-atendimento" : "status-finalizado");
                    html += $@"
                            <tr>
                                <td>{c.DataAbertura:dd/MM/yyyy HH:mm}</td>
                                <td>{c.Titulo}</td>
                                <td>{c.Solicitante}</td>
                                <td>{c.Setor}</td>
                                <td class='{statusClass}'>{c.Status}</td>
                                <td>{c.DataFinalizacao?.ToString("dd/MM/yyyy HH:mm") ?? "-"}</td>
                            </tr>";
                }

                html += $@"
                        </tbody>
                    </table>
                    <div class='footer'>Gerado por TI Manager em {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                </body>
                </html>";

                string tempHtml = Path.Combine(Path.GetTempPath(), $"relatorio_{Guid.NewGuid()}.html");
                File.WriteAllText(tempHtml, html);

                AbrirVisualizadorPdf(tempHtml, "Relatório de Chamados - " + mesAno);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar relatório: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirVisualizadorPdf(string path, string titulo)
        {
            pnlDetalhes.Controls.Clear();
            var viewer = new PdfViewControl();
            viewer.OnVoltar += (s, ev) => { pnlDetalhes.Visible = false; pnlContainerGeral.Visible = true; };
            
            // Agora usamos a conversão para PDF para habilitar o Toolbar do WebView2
            viewer.CarregarHtmlComoPdf(path, titulo);
            
            pnlDetalhes.Controls.Add(viewer);
            pnlContainerGeral.Visible = false;
            pnlDetalhes.Visible = true;
        }
    }
}
