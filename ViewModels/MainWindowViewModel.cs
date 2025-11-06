using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BackupMonitor.Services;
using BackupMonitor.Views;
using Avalonia.Controls;
using BackupMonitor.Models;

namespace BackupMonitor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Window ownerWindow;
    private AppConfig appConfig;
    private readonly PostgresService pgService = new();
    private readonly HashService hashService = new();
    private readonly CriptografiaService criptoService = new();
    private readonly AzureBlobService azureService = new();

    [ObservableProperty]
    private string dbHost = "localhost";
    
    [ObservableProperty]
    private int dbPort = 5432;
    
    [ObservableProperty]
    private string dbUser = "usuario";
    
    [ObservableProperty]
    private string dbPassword = string.Empty;
    
    [ObservableProperty] 
    private string dbName = "testes";

    [ObservableProperty]
    private string status = "Pronto";
    
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
                Status = "Nenhum arquivo de configuração encontrado. Configure os dados no aplicativo.";
                isOk = true;
                return;
            }

            // Caso exista, pede a senha de acesso
            var janelaSenha = new JanelaSenha();
            var result = await janelaSenha.ShowDialog<bool?>(owner);

            // Usuário cancelou ou não digitou a senha
            if (result != true || string.IsNullOrEmpty(janelaSenha.SenhaAcesso))
            {
                Status = "Operação cancelada pelo usuário.";
                return;
            }

            var config = ConfigService.LoadConfig(janelaSenha.SenhaAcesso);
            appConfig = config;

            DbHost = config.PostgresHost;
            DbPort = config.PostgresPort;
            DbUser = config.PostgresUser;
            DbPassword = config.PostgresPassword;
            DbName = config.PostgresDbName;

            isOk = true;
            Status = "Configuração carregada com sucesso.";
        }
        catch (Exception ex)
        {
            Status = $"Erro ao carregar configuração: {ex.Message}";
        }
    }

    [RelayCommand]
    private void BackupDatabase()
    {
        try
        {
            Status = "Iniciando dump do PostgreSQL...";
            
            //Gera o dump do banco de dados
            string dumpPath = pgService.BackupDatabase(
                DbHost, DbPort, DbName, DbUser, DbPassword, Path.GetTempPath());
            
            Status = $"Dump criado: {Path.GetFileName(dumpPath)}\nCalculando hash...";

            //Calcula o hash do arquivo
            string hash = hashService.ComputeSha256(dumpPath);
            Status = $"Hash: {hash.Substring(0, 12)}...\nCriptografando...";

            //Criptografa o arquivo antes do envio
            string encPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(dumpPath) + ".enc");
            criptoService.EncryptFile(dumpPath, encPath, appConfig.AccessPassword);
            Status = "Enviando para Azure Blob...";

            //Realiza o Upload para o Azure Blob Storage
            string blobFileName = $"{DbName}_{DateTime.Now:yyyyMMdd_HHmmss}_{hash.Substring(0, 16)}.enc";
            azureService.Upload(encPath, blobFileName);

            Status = "Backup concluído! Limpando arquivos...";

            //Limpa os arquivos temporários
            if (File.Exists(dumpPath)) File.Delete(dumpPath);
            if (File.Exists(encPath)) File.Delete(encPath);

            Status = "✅ Backup finalizado com sucesso!";
        }
        catch (Exception ex)
        {
            Status = $"❌ Erro no backup: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TestarConexao()
    {
        try
        {
            Status = "Testando conexão com PostgreSQL...";

            if (pgService.TestarConexao(dbHost, dbPort, dbUser, dbPassword, dbName))
                Status = "✅ Conexão com PostgreSQL OK!";
            else
                Status = "❌ Falha na conexão: Vefique os dados e tente novamente.";
        }
        catch (Exception ex)
        {
            Status = $"❌ Falha na conexão: Vefique os dados e tente novamente.";
        }
    }

    [RelayCommand]
    private async Task GravarDadosPostgres()
    {
        try
        {
            if (string.IsNullOrEmpty(DbHost) ||
               string.IsNullOrEmpty(DbUser) ||
               string.IsNullOrEmpty(DbName))
            {
                Status = "❌ Preencha todos os campos obrigatórios.";
                return;
            }

            if (DbPort <= 0)
            {
                Status = "❌ A porta deve ser um número válido.";
                return;
            }

            if (string.IsNullOrEmpty(DbPassword))
            {
                Status = "❌ A senha do banco de dados não pode ser vazia.";
                return;
            }
            
            if (appConfig == null)
            {
                Status = "⚠️ É necessário cadastrar uma senha de acesso em configurações antes de gravar os dados.";
                return;
            }

            var janelaSenha = new JanelaSenha();
            var result = await janelaSenha.ShowDialog<bool?>(ownerWindow);

            // Usuário cancelou ou não digitou a senha
            if (result != true || string.IsNullOrEmpty(janelaSenha.SenhaAcesso))
            {
                Status = "Operação cancelada pelo usuário.";
                return;
            }

            var config = new AppConfig
            {
                PostgresConnectionString = $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}",
                PostgresHost = DbHost,
                PostgresPort = DbPort,
                PostgresUser = DbUser,
                PostgresPassword = DbPassword,
                PostgresDbName = DbName
            };

            ConfigService.SaveConfig(config, janelaSenha.SenhaAcesso);

            Status = "✅ Dados do PostgreSQL gravados com sucesso!";
        }
        catch (Exception ex)
        {
            Status = $"❌ Erro ao gravar dados: {ex.Message}";
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
        JanelaConfig janela;

        if (appConfig == null)
        {
            janela = new();
        }
        else
        {
            janela = new(appConfig.PostgresConnectionString, appConfig.AccessPassword);
        }

        janela.Show();
    }
}