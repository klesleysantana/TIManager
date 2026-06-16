# Contexto do Projeto: TI Manager (Gerenciador de TI)

Este documento fornece um panorama detalhado sobre o projeto **TI Manager**, uma aplicação desktop moderna desenvolvida em Windows Forms com o objetivo de centralizar a gestão de chamados de TI, base de conhecimento (Wiki), relatórios e controle de ativos/credenciais.

Ele serve como o contexto ideal para ser fornecido ao **Antigravity IDE** para guiar futuros desenvolvimentos e manutenções.

---

## 💻 Stack Tecnológica e Dependências

A aplicação foi construída utilizando tecnologias modernas da plataforma Windows:

1. **Framework Principal:** `.NET 11.0-windows` (Windows Forms moderno, empacotado como `WinExe`).
2. **Banco de Dados:** SQLite local através do pacote `Microsoft.Data.Sqlite` (v10.0.7).
3. **Componente Web e PDF:** `Microsoft.Web.WebView2` (v1.0.3912.50) para renderização nativa de páginas HTML, visualização de PDFs locais e exportação/impressão em PDF.
4. **Manipulação de PDF:** `bblanchon.PDFium.Win32` (v149.0.7825) e `PdfiumViewer.Updated` (v2.14.5) para carregamento de documentos PDF e geração dinâmica de miniaturas de imagem.
5. **Desenho e UI (GDI+):** Customizações avançadas de controles WinForms utilizando double buffering (evitando flicker/piscadas) e chamadas nativas do Windows (`Gdi32.dll`) para arredondamento de cantos.

---

## 📁 Estrutura de Diretórios do Código

A solução `TIManager.slnx` referencia o projeto único `TIManager` organizado da seguinte forma:

```text
TIManager/
│
├── TIManager.csproj        # Definições de projeto, SDK e pacotes NuGet
├── Program.cs              # Inicialização do banco e ponto de entrada da aplicação
├── Form1.cs                # Form principal (Shell da aplicação com Sidebar animada)
│
├── Models/                 # Entidades de Dados
│   ├── Chamado.cs          # Modelo dos chamados/chamados abertos
│   └── Documento.cs        # Modelo para arquivos da Wiki e Relatórios
│
├── Data/                   # Camada de Persistência
│   └── DatabaseService.cs  # Inicialização, Migrações e CRUD do SQLite
│
├── Utils/                  # Utilitários de UI e Helpers de Mídia
│   ├── UIHelper.cs         # Métodos para cantos arredondados, double buffer e caminhos GDI+
│   ├── RoundedButton.cs    # Botão customizado com cantos arredondados e borda
│   └── PdfHelper.cs        # Wrapper do PdfiumViewer para gerar miniaturas de PDFs
│
└── Views/                  # Controles de Usuário (UserControls) e Modais
    ├── HomeControl.cs            # Dashboard com estatísticas e gráficos GDI+
    ├── ChamadosControl.cs        # Listagem agrupada por mês, filtros e impressão
    ├── ChamadoDetalhesControl.cs # Visualização detalhada de um chamado e anexo
    ├── NovoChamadoForm.cs        # Formulário para abertura/edição de chamado e anexo de imagens
    ├── MonthYearPickerForm.cs    # Seletor personalizado (Pop-up) para escolha de Mês/Ano de relatórios
    ├── WikiControl.cs            # Biblioteca/Estante de PDFs com miniaturas dinâmicas
    ├── RelatoriosControl.cs      # Similar à Wiki, focado no gerenciamento de relatórios PDFs
    ├── PdfViewControl.cs         # Viewer embutido com WebView2 para PDF e HTML to PDF
    ├── InventarioControl.cs      # [Placeholder] Gestão futura de Ativos de TI
    └── CredenciaisControl.cs     # [Placeholder] Gestor futuro de Credenciais
```

---

## 🗄️ Estrutura do Banco de Dados e Arquivos

O banco de dados SQLite é inicializado em tempo de execução no caminho fixo `C:\TIManagerData\timanager.db`. A estrutura de diretórios e tabelas consiste em:

### Diretórios de Armazenamento Local:
* Banco de Dados: `C:\TIManagerData`
* Arquivos PDF da Wiki e Relatórios: `C:\TIManagerData\Documentos`
* Imagens de Miniatura (Thumbnails): `C:\TIManagerData\Thumbnails`
* Imagens Anexadas a Chamados: `C:\TIManagerData\Imagens`

### Tabelas:

#### 1. `Chamados`
Armazena os registros de suporte de TI:
* `Id` (`INTEGER` PRIMARY KEY AUTOINCREMENT)
* `Titulo` (`TEXT` NOT NULL)
* `Solicitante` (`TEXT` NOT NULL)
* `Setor` (`TEXT`) - Setores disponíveis: *Coordenação de Projetos, Logística, TI, Contabilidade, Financeiro, Prestação de Contas, RH/Departamento Pessoal, Diretoria, CERII, TEA, Oficinas, Portaria, Call Center, Loja, Infantojuvenil, CAPS*
* `Prioridade` (`TEXT`) - *Baixa, Média, Alta, Crítica*
* `Descricao` (`TEXT`)
* `Status` (`TEXT`) - *Aberto, Em Atendimento, Finalizado*
* `CaminhoImagem` (`TEXT`) - Caminho local do anexo
* `DataAbertura` (`TEXT` NOT NULL) - Data gravada em string ISO 8601
* `DataFinalizacao` (`TEXT`) - Data de conclusão (calculada automaticamente ao marcar como "Finalizado")

#### 2. `Documentos`
Armazena metadados de PDFs carregados no sistema (Wiki ou Relatórios):
* `Id` (`INTEGER` PRIMARY KEY AUTOINCREMENT)
* `Nome` (`TEXT` NOT NULL)
* `Categoria` (`TEXT` NOT NULL) - *Wiki* ou *Relatorio*
* `CaminhoArquivo` (`TEXT` NOT NULL) - Caminho do PDF salvo localmente
* `CaminhoMiniatura` (`TEXT`) - Caminho da imagem gerada como miniatura
* `DataUpload` (`TEXT` NOT NULL) - Gravada em string ISO 8601

---

## 🏛️ Arquitetura de UI e Componentes

A interface adota um design moderno com uma paleta de cores institucional em tons de **Teal escuro (`#1B676B`)** e **Verde Limão (`#88C425`)**.

### 1. Navegação Principal (`Form1`)
* Possui uma **Sidebar animada** acionada por um Timer que expande (largura 250px) e colapsa (largura 60px), com efeito de opacidade suave no texto dos botões.
* A troca de telas é feita chamando `ShowControl(UserControl)`, que remove o controle ativo, adiciona o novo e aplica uma microanimação de transição lateral (efeito slide rápido).
* O menu "Chamados" expande-se verticalmente em formato cascata para mostrar filtros rápidos por status (*Todos, Aberto, Em Atendimento, Finalizado*).

### 2. Dashboard (`HomeControl`)
* **Gráfico de Barras (`BarChartPanel`):** Desenho customizado em GDI+ que plota o volume de chamados por mês (últimos 6 meses). Inclui degradês lineares de verde para azul nos cards e textos informativos de quantidade.
* **Gráficos de Pizza (`PieChartPanel`):** Componente genérico que recebe uma função lambda seletora para agrupar chamados por `Status` ou por `Setor` e plota os dados de forma circular com uma legenda dinâmica colorida.
* **Lista de Chamados Recentes:** Painel responsivo que exibe os últimos 6 chamados cadastrados, com efeito visual de hover e navegação direta ao clicar no card do chamado.
* **Barra de Pesquisa:** Localizada no painel direito para filtros rápidos de chamados recentes.

### 3. Gestão de Chamados (`ChamadosControl` e `NovoChamadoForm`)
* **Visualização Agrupada por Mês:** Ao invés de uma lista única, os chamados são divididos em várias tabelas (`DataGridView`), uma para cada mês do ano, permitindo a redução de scroll infinito e melhor leitura.
* **Comportamentos Especiais da Tabela:**
  * O botão 👁 no final de cada linha abre os detalhes do chamado.
  * O clique duplo na linha executa a mesma ação de visualização.
  * O botão direito do mouse exibe um menu de contexto com a opção "Editar Chamado".
  * Formatação de cores no texto das colunas `Prioridade` e `Status` (Ex: Vermelho para Alta/Crítica, Azul para Aberto, Cinza para Finalizado).
* **Impressão de Relatórios de Fechamento:**
  * Utiliza o `MonthYearPickerForm` (uma janela flutuante popup que some ao perder o foco) para escolher o período (Mês/Ano).
  * Constrói dinamicamente uma página HTML estruturada com os dados filtrados.
  * Carrega a página HTML no WebView2 e executa de forma oculta `PrintToPdfAsync` no formato Paisagem (Landscape).
  * Exibe o PDF resultante no visualizador embutido da aplicação.

### 4. Base de Wiki e Relatórios (`WikiControl` e `RelatoriosControl`)
* Funcionam como uma estante virtual de documentos PDF.
* Ao clicar no botão para adicionar um documento, o sistema:
  1. Copia o arquivo original do usuário para `C:\TIManagerData\Documentos` renomeando-o com um `Guid` único para evitar colisões.
  2. Executa o helper `PdfHelper.GerarMiniatura` que carrega a página `0` do PDF com o **Pdfium**, renderiza uma imagem JPEG e a salva na pasta `Thumbnails`.
  3. Insere o registro na tabela `Documentos` do banco de dados.
* A listagem de documentos exibe cards contendo a imagem gerada (ou um ícone genérico se falhar), o nome do arquivo e a data de adição.
* Ao clicar em um card, o controle abre o arquivo local no `PdfViewControl`, que faz uso da API `Navigate` do WebView2 com as barras de ferramenta integradas de visualização de PDFs da Microsoft.

---

## 🛠️ Detalhes dos Utilitários Customizados

### `RoundedButton.cs`
Estende o controle padrão `System.Windows.Forms.Button` e redefine o método `OnPaint` para desenhar bordas e preenchimentos suaves utilizando `GraphicsPath.AddArc` com anti-aliasing ativo, proporcionando cantos arredondados de raio personalizável nas propriedades do editor do Visual Studio.

### `UIHelper.cs`
* `ArredondarCantos(Control, int)`: Aplica um recorte geométrico na janela do controle (`Region.FromHrgn`) importando nativamente do Windows a função `CreateRoundRectRgn`.
* `SetDoubleBuffered(Control)`: Acessa via Reflection a propriedade interna `DoubleBuffered` de qualquer controle para mitigar piscadas de renderização ao redimensionar elementos pesados, como tabelas.

### `PdfHelper.cs`
Carrega o PDF de forma estática:
```csharp
using (var document = PdfDocument.Load(pdfPath))
{
    if (document.PageCount > 0)
    {
        using (var image = document.Render(0, 300, 400, 96, 96, false))
        {
            image.Save(thumbPath, ImageFormat.Jpeg);
        }
        return thumbPath;
    }
}
```

---

## 🎯 Próximos Passos (Pontos Pendentes)

Ao carregar o contexto no Antigravity IDE, o desenvolvedor notará que as seguintes telas possuem apenas componentes estruturais vazios (Placeholders):
1. **Inventário (`InventarioControl.cs`):** Apenas uma label indicativa de título. Necessita da modelagem de ativos de TI (computadores, monitores, switches, termos de responsabilidade), tabelas no SQLite correspondentes e interface para CRUD.
2. **Credenciais (`CredenciaisControl.cs`):** Apenas uma label indicativa de título. Necessita de desenvolvimento de cofre de senhas de sistemas e redes, preferencialmente com criptografia básica para segurança dos registros locais.
