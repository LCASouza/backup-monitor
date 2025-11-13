using System;
using Avalonia.Controls;
using BackupMonitor.ViewModels;

namespace BackupMonitor.Views;

public partial class JanelaSenha : Window
{
    public string SenhaAcesso { get; private set; }

    public JanelaSenha(bool primeiroAcesso)
    {
        InitializeComponent();
        DataContext = new JanelaSenhaViewModel(primeiroAcesso);
    }

    public JanelaSenha()
    {
        InitializeComponent();
        DataContext = new JanelaSenhaViewModel(this);
    }

    public void Confirmar(string senha)
    {
        if (string.IsNullOrEmpty(senha))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));

        SenhaAcesso = senha;
        Close(true);
    }

    public void Cancelar()
    {
        SenhaAcesso = string.Empty;
        Close(false);
    }
}