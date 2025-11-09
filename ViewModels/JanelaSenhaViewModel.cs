using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BackupMonitor.Views;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaSenhaViewModel : ViewModelBase
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
                var cfg = SessionContext.CurrentConfig!;
                janela.Confirmar(SenhaAcesso);

                Status = "Senha confirmada com sucesso.";
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
