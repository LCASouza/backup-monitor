using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BackupMonitor.Views;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaSenhaViewModel : ObservableObject
    {
        private readonly JanelaSenha janela;

        [ObservableProperty]
        private string senhaAcesso;

        [ObservableProperty]
        private string status = "";

        public JanelaSenhaViewModel(JanelaSenha window)
        {
            janela = window;
        }

        [RelayCommand]
        private void ConfirmarSenha()
        {
            try
            {
                var config = ConfigService.LoadConfig(SenhaAcesso);
                Status = "Senha correta!";
                janela.Confirmar(SenhaAcesso);
            }
            catch
            {
                Status = "Senha incorreta. Tente novamente.";
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            janela.Cancelar();
        }
    }
}
