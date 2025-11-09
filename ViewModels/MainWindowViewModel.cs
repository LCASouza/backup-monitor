using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BackupMonitor.Services;
using BackupMonitor.Views;
using Avalonia.Controls;
using BackupMonitor.Models;
using Avalonia.Controls.ApplicationLifetimes;

namespace BackupMonitor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private MainWindowModel modelo = new MainWindowModel();
        
    private Window ownerWindow;
    private AppConfig appConfig;
    private readonly PostgresService pgService = new();
    private readonly HashService hashService = new();
    private readonly CriptografiaService criptoService = new();
    private readonly AzureBlobService azureService = new();
    
    public bool isOk = false;

    public async Task InitializeAsync(Window owner)
    {
        try
        {
            ownerWindow = owner;

            // Caminho padrão do arquivo de configuração
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.enc");

            if (!File.Exists(configPath))
            {
                Modelo.Status = "Nenhum arquivo de configuração encontrado. Configure os dados no aplicativo.";
                isOk = true;
                return;
            }

            // Caso exista, pede a senha de acesso
            var janelaSenha = new JanelaSenha();
            var result = await janelaSenha.ShowDialog<bool?>(owner);

            // Usuário cancelou ou não digitou a senha
            if (result != true || string.IsNullOrEmpty(janelaSenha.SenhaAcesso))
            {
                Modelo.Status = "Operação cancelada pelo usuário.";
                return;
            }

            var config = ConfigService.LoadConfig(janelaSenha.SenhaAcesso);

            SessionContext.CurrentConfig = config;
            SessionContext.AccessPassword = janelaSenha.SenhaAcesso;

            appConfig = config;

            Modelo.DbHost = config.PostgresHost;
            Modelo.DbPort = config.PostgresPort;
            Modelo.DbUser = config.PostgresUser;
            Modelo.DbPassword = config.PostgresPassword;
            Modelo.DbName = config.PostgresDbName;

            isOk = true;
            Modelo.Status = "Configuração carregada com sucesso.";
        }
        catch (Exception ex)
        {
            Modelo.Status = $"Erro ao carregar configuração: {ex.Message}";
        }
    }

    [RelayCommand]
    private void BackupDatabase()
    {
        try
        {
            Modelo.Status = "Iniciando dump do PostgreSQL...";
            
            //Gera o dump do banco de dados
            string dumpPath = pgService.BackupDatabase(
                Modelo.DbHost, Modelo.DbPort, Modelo.DbName, Modelo.DbUser, Modelo.DbPassword, Path.GetTempPath());
            
            Modelo.Status = $"Dump criado: {Path.GetFileName(dumpPath)}\nCalculando hash...";

            //Calcula o hash do arquivo
            string hash = hashService.ComputeSha256(dumpPath);
            Modelo.Status = $"Hash: {hash.Substring(0, 12)}...\nCriptografando...";

            //Criptografa o arquivo antes do envio
            string encPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(dumpPath) + ".enc");
            criptoService.EncryptFile(dumpPath, encPath, appConfig.AccessPassword);
            Modelo.Status = "Enviando para Azure Blob...";

            //Realiza o Upload para o Azure Blob Storage
            string blobFileName = $"{Modelo.DbName}_{DateTime.Now:yyyyMMdd_HHmmss}_{hash.Substring(0, 16)}.enc";
            azureService.Upload(encPath, blobFileName);

            Modelo.Status = "Backup concluído! Limpando arquivos...";

            //Limpa os arquivos temporários
            if (File.Exists(dumpPath)) File.Delete(dumpPath);
            if (File.Exists(encPath)) File.Delete(encPath);

            Modelo.Status = "✅ Backup finalizado com sucesso!";
        }
        catch (Exception ex)
        {
            Modelo.Status = $"❌ Erro no backup: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TestarConexao()
    {
        try
        {
            Modelo.Status = "Testando conexão com PostgreSQL...";

            if (pgService.TestarConexao(Modelo.DbHost, Modelo.DbPort, Modelo.DbUser, Modelo.DbPassword, Modelo.DbName))
                Modelo.Status = "✅ Conexão com PostgreSQL OK!";
            else
                Modelo.Status = "❌ Falha na conexão: Vefique os dados e tente novamente.";
        }
        catch (Exception ex)
        {
            Modelo.Status = $"❌ Falha na conexão: Vefique os dados e tente novamente.";
        }
    }

    [RelayCommand]
    private async Task GravarDadosPostgres()
    {
        try
        {
            if (string.IsNullOrEmpty(Modelo.DbHost) ||
               string.IsNullOrEmpty(Modelo.DbUser) ||
               string.IsNullOrEmpty(Modelo.DbName))
            {
                Modelo.Status = "❌ Preencha todos os campos obrigatórios.";
                return;
            }

            if (Modelo.DbPort <= 0)
            {
                Modelo.Status = "❌ A porta deve ser um número válido.";
                return;
            }

            if (string.IsNullOrEmpty(Modelo.DbPassword))
            {
                Modelo.Status = "❌ A senha do banco de dados não pode ser vazia.";
                return;
            }

            if (appConfig == null)
            {
                Modelo.Status = "⚠️ É necessário cadastrar uma senha de acesso em configurações antes de gravar os dados.";
                return;
            }

            var janelaSenha = new JanelaSenha();
            var result = await janelaSenha.ShowDialog<bool?>(ownerWindow);

            // Usuário cancelou ou não digitou a senha
            if (result != true || string.IsNullOrEmpty(janelaSenha.SenhaAcesso))
            {
                Modelo.Status = "Operação cancelada pelo usuário.";
                return;
            }

            var config = new AppConfig
            {
                PostgresConnectionString = $"Host={Modelo.DbHost};Port={Modelo.DbPort};Database={Modelo.DbName};Username={Modelo.DbUser};Password={Modelo.DbPassword}",
                PostgresHost = Modelo.DbHost,
                PostgresPort = Modelo.DbPort,
                PostgresUser = Modelo.DbUser,
                PostgresPassword = Modelo.DbPassword,
                PostgresDbName = Modelo.DbName
            };

            ConfigService.SaveConfig(config, janelaSenha.SenhaAcesso);

            Modelo.Status = "✅ Dados do PostgreSQL gravados com sucesso!";
        }
        catch (Exception ex)
        {
            Modelo.Status = $"❌ Erro ao gravar dados: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task BackupLocalAsync()
    {
        try
        {
            Modelo.Status = "📦 Iniciando backup local...";

            var dialog = new SaveFileDialog
            {
                Title = "Salvar backup do banco",
                Filters =
                {
                    new FileDialogFilter() { Name = "Arquivos de backup PostgreSQL", Extensions = { "dump" } },
                    new FileDialogFilter() { Name = "Todos os arquivos", Extensions = { "*" } }
                },
                InitialFileName = $"{Modelo.DbName}_backup_{DateTime.Now:yyyyMMdd_HHmm}.dump"
            };

            string? filePath = await dialog.ShowAsync(ownerWindow);
            if (string.IsNullOrEmpty(filePath))
            {
                Modelo.Status = "Operação cancelada pelo usuário.";
                return;
            }

            Modelo.Status = "⏳ Gerando dump...";

            // Chama o serviço de dump
            string dumpPath = pgService.BackupDatabase(
                Modelo.DbHost,
                Modelo.DbPort,
                Modelo.DbName,
                Modelo.DbUser,
                Modelo.DbPassword,
                filePath
            );

            Modelo.Status = $"✅ Backup salvo com sucesso em:\n{filePath}";
        }
        catch (Exception ex)
        {
            Modelo.Status = $"❌ Erro ao realizar backup local: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AbrirJanelaBackup()
    {
        JanelaBackup janela = new();

        janela.Show();
    }

    [RelayCommand]
    private void AbrirJanelaConfig()
    {
        JanelaConfig janela = new();

        janela.Show();
    }

    [RelayCommand]
    private void AbrirRestore()
    {
        var janela = new JanelaRestore();
        janela.ShowDialog(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
    }
}