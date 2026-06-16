namespace TIManager;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        try
        {
            TIManager.Data.DatabaseService.Instance.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao inicializar o banco de dados: {ex.Message}", "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Application.Run(new Form1());
    }    
}