using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Views;

public partial class AssignmentPanelView : UserControl
{
    private List<AssignmentDTO> _assignments = new();
    private string? _selectedFilePath;

    public AssignmentPanelView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await LoadAssignments();
        };
    }

    private async Task LoadAssignments()
    {
        try
        {
            var cursos = await App.Api.GetCoursesAsync(App.CurrentUserId);
            _assignments.Clear();

            foreach (var curso in cursos)
            {
                var tareas = await App.Api.GetAssignmentsByCourseAsync(curso.IdCurso);
                _assignments.AddRange(tareas);
            }

            var now = DateTime.UtcNow;
            PendingList.ItemsSource = _assignments
                .Where(a => a.FechaLimite > now)
                .Select(a => $"{a.Titulo} - Vence: {a.FechaLimite:g}")
                .ToList();
            DeliveredList.ItemsSource = _assignments
                .Where(a => a.FechaLimite < now)
                .Select(a => $"{a.Titulo} - Entregada")
                .ToList();
            OverdueList.ItemsSource = _assignments
                .Where(a => a.FechaLimite < now)
                .Select(a => $"{a.Titulo} - Vencida")
                .ToList();

            if (_assignments.Any())
            {
                var first = _assignments.First();
                AssignmentTitle.Text = first.Titulo;
                AssignmentDescription.Text = first.Descripcion ?? "Sin descripción";
                DeadlineText.Text = $"Fecha límite: {first.FechaLimite:g}";
            }
        }
        catch { }
    }

    private async void OnSubmit(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Python Files|*.py" };
        if (dialog.ShowDialog() != true) return;

        _selectedFilePath = dialog.FileName;
        var content = await File.ReadAllBytesAsync(_selectedFilePath);

        if (!_assignments.Any()) return;
        var assignment = _assignments.First();

        var dto = new DeliveryDTO
        {
            IdAsignacion = assignment.IdAsignacion,
            IdEstudiante = App.CurrentUserId,
            RutaArchivo = _selectedFilePath,
            ContenidoBase64 = Convert.ToBase64String(content)
        };

        var result = await App.Api.SubmitDeliveryAsync(dto);
        if (result != null && result.Exitoso)
        {
            MessageBox.Show($"Entrega exitosa\nFirma: {result.FirmaDigital?[..20]}...\nHora: {result.Timestamp:g}",
                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            ResubmitButton.Visibility = Visibility.Visible;
        }
        else
        {
            MessageBox.Show("Error al entregar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnResubmit(object sender, RoutedEventArgs e)
    {
        OnSubmit(sender, e);
    }
}
